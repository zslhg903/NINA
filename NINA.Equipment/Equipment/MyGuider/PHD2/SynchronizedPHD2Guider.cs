#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Locale;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Profile.Interfaces;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Utility.WindowService;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Core.Interfaces;
using NINA.Core.Model;
using NINA.Equipment.Interfaces;

#pragma warning disable 4014

namespace NINA.Equipment.Equipment.MyGuider.PHD2 {

    public class SynchronizedPHD2Guider : BaseINPC, IGuider, ICameraConsumer {
        private const string LocalHostUri = "net.pipe://localhost";
        private const string ServiceEndPoint = "SynchronizedPHD2Guider";
        private readonly ICameraMediator cameraMediator;
        private readonly IProfileService profileService;
        private readonly IWindowServiceFactory windowServiceFactory;
        private bool cameraIsExposing;
        private bool connected;
        private CancellationTokenSource disconnectTokenSource;
        private DateTime exposureEndTime;
        private ISynchronizedPHD2GuiderService guiderService;
        private double lastDownloadTime;
        private double nextExposureLength;
        private double pixelScale;
        private TaskCompletionSource<bool> startServiceTcs;
        private string state;

        public event EventHandler<IGuideStep> GuideEvent;

        public SynchronizedPHD2Guider(IProfileService profileService, ICameraMediator cameraMediator, IWindowServiceFactory windowServiceFactory) {
            this.profileService = profileService;
            this.cameraMediator = cameraMediator;
            this.windowServiceFactory = windowServiceFactory;
        }

        private ILoc Locale { get; set; } = Loc.Instance;

        /// <inheritdoc />
        public bool Connected {
            get => connected;
            set {
                connected = value;
                RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
        public string Name => Locale["LblSynchronizedPHD2Guider"];

        /// <inheritdoc />
        public string Id => "PHD2_Synchronized";

        /// <inheritdoc />
        public double PixelScale {
            get => pixelScale;
            set {
                pixelScale = value;
                RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
        public string State {
            get => state;
            set {
                state = value;
                RaisePropertyChanged(nameof(State));
            }
        }

        public bool HasSetupDialog => false;

        public string Category => "Guiders";

        public string Description => "Synchronized PHD2 Guider";

        public string DriverInfo => "Synchronized PHD2 Guider";

        public string DriverVersion => "1.0";

        private ISynchronizedPHD2GuiderService ConnectToService() {
            var guiderServiceChannelFactory
                = new ChannelFactory<ISynchronizedPHD2GuiderService>(new NetNamedPipeBinding(), new EndpointAddress(LocalHostUri + "/" + ServiceEndPoint));
            guiderServiceChannelFactory.Endpoint.Binding.OpenTimeout = TimeSpan.FromSeconds(1);
            guiderServiceChannelFactory.Endpoint.Binding.CloseTimeout = TimeSpan.FromSeconds(1);
            guiderServiceChannelFactory.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromSeconds(600);
            guiderServiceChannelFactory.Open();
            ISynchronizedPHD2GuiderService serviceChannel = guiderServiceChannelFactory.CreateChannel();
            // timespan for waiting for a method to return, to be on the safe side set to 10 minutes
            // the 'suspicious' cast is not suspicious at all, the IDE doesn't understand it's a channel we get returned
            ((IContextChannel)serviceChannel).OperationTimeout = TimeSpan.FromMinutes(10);
            return serviceChannel;
        }

        private async Task RunClientListener(CancellationToken ct) {
            bool faulted = false;
            while (!faulted) {
                try {
                    // connect and register to the service and get the pixelscale (that's the only information we need only once)
                    PixelScale = await guiderService.ConnectAndGetPixelScale(profileService.ActiveProfile.Id);
                    // register to camera consumer so the camera info data is updated to the service
                    cameraMediator.RegisterConsumer(this);
                    // loop getting the latest phd2 service information indefinitely
                    while (true) {
                        var guideInfos = await guiderService.GetGuideInfo(profileService.ActiveProfile.Id);

                        State = guideInfos.State;
                        if (guideInfos.GuideStep != null) {
                            GuideEvent?.Invoke(this, guideInfos.GuideStep);
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(1000), ct);
                        ct.ThrowIfCancellationRequested();
                    }
                } catch (FaultException<ClientAlreadyExistsFault>) {
                    faulted = true;
                    Connected = false;
                    State = "";
                    PixelScale = 0;
                    disconnectTokenSource.Cancel();
                    Notification.ShowError(Locale["LblSynchronizedPHD2ServiceClientAlreadyExists"]);
                } catch (FaultException<PHD2Fault>) {
                    // phd2 connection lost
                    faulted = true;
                    Connected = false;
                    State = "";
                    PixelScale = 0;
                    disconnectTokenSource.Cancel();
                } catch (OperationCanceledException) {
                    // user disconnected
                    disconnectTokenSource.Cancel();
                    Connected = false;
                    State = "";
                    PixelScale = 0;
                    break;
                } catch (Exception) {
                    // assume nina other instance crash, restart server
                    Notification.ShowWarning(Locale["LblSynchronizedPHD2ServiceCrashed"]);
                    // remove the consumer since we will restart the loop getting all information and re-registering on the service
                    cameraMediator.RemoveConsumer(this);
                    // try to restart the server
                    await TryStartServiceAndConnect();
                    faulted = !Connected;
                }
            }

            // faulted is false when the client disconnected willfully, not forcefully
            if (!faulted) {
                guiderService.DisconnectClient(profileService.ActiveProfile.Id);
            }

            cameraMediator.RemoveConsumer(this);
        }

        private async Task RunService(CancellationToken ct) {
            // here we initialize a service singleton so we can call initialize without making the phd2guider serializable
            using (ServiceHost host = new ServiceHost(new SynchronizedPHD2GuiderService(), new Uri(LocalHostUri))) {
                host.AddServiceEndpoint(typeof(ISynchronizedPHD2GuiderService), new NetNamedPipeBinding(),
                    ServiceEndPoint);
                var behavior = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
                behavior.IncludeExceptionDetailInFaults = true;
                // single instance => singleton
                behavior.InstanceContextMode = InstanceContextMode.Single;
                // multiple concurrencymode => multithreading, it is not on by default and async Task methods will actually lock
                behavior.ConcurrencyMode = ConcurrencyMode.Multiple;

                // try to open the host, if it crashes this instance is most definitely not the service
                try {
                    host.Open();
                } catch (Exception) {
                    startServiceTcs.TrySetResult(false);
                    return;
                }

                // initialize the service, try to start and connect to phd2
                startServiceTcs.TrySetResult(await ((SynchronizedPHD2GuiderService)host.SingletonInstance)
                    .Initialize(new PHD2Guider(profileService, windowServiceFactory), ct));

                // loop to keep the service alive
                while (!ct.IsCancellationRequested) {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000), ct);
                }
            }
        }

        private async Task TryStartServiceAndConnect() {
            startServiceTcs = new TaskCompletionSource<bool>();
            // try to run the server loop
            Task.Run(() => RunService(disconnectTokenSource.Token));
            // wait for the server to be either started or failed to be started
            await startServiceTcs.Task;
            // try to connect to any existing server
            guiderService = ConnectToService();
            Connected = guiderService != null;
        }

        /// <inheritdoc />
        public Task<bool> AutoSelectGuideStar() {
            return Task.Run(() => guiderService.AutoSelectGuideStar());
        }

        /// <inheritdoc />
        public async Task<bool> Connect(CancellationToken token) {
            disconnectTokenSource?.Dispose();
            disconnectTokenSource = new CancellationTokenSource();

            await TryStartServiceAndConnect();

            if (Connected) {
                Task.Run(() => RunClientListener(disconnectTokenSource.Token));
            } else {
                disconnectTokenSource.Cancel();
            }

            return Connected;
        }

        /// <inheritdoc />
        public void Disconnect() {
            disconnectTokenSource.Cancel();

            Connected = false;
        }

        /// <inheritdoc />
        public Task<bool> Dither(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            return Task.Run(async () => {
                ct.Register(guiderService.CancelSynchronizedDither);
                return await guiderService.SynchronizedDither(profileService.ActiveProfile.Id);
            }, ct);
        }

        /// <inheritdoc />
        public Task<bool> StartGuiding(bool forceCalibration, IProgress<ApplicationStatus> progress, CancellationToken ct) {
            return Task.Run(async () => {
                ct.Register(guiderService.CancelStartGuiding);
                return await guiderService.StartGuiding(forceCalibration);
            }, ct);
        }

        /// <inheritdoc />
        public Task<bool> StopGuiding(CancellationToken ct) {
            return Task.Run(async () => {
                ct.Register(guiderService.CancelStopGuiding);
                return await guiderService.StopGuiding();
            }, ct);
        }

        public bool CanClearCalibration {
            get => true;
        }

        /// <inheritdoc />
        public Task<bool> ClearCalibration(CancellationToken ct) {
            return Task.Run(async () => {
                ct.Register(guiderService.CancelStopGuiding);
                return await guiderService.ClearCalibration();
            }, ct);
        }

        public async void UpdateDeviceInfo(CameraInfo deviceInfo) {
            if ((exposureEndTime != deviceInfo.ExposureEndTime || cameraIsExposing != deviceInfo.IsExposing ||
                nextExposureLength != deviceInfo.NextExposureLength || lastDownloadTime != deviceInfo.LastDownloadTime) && connected) {
                exposureEndTime = deviceInfo.ExposureEndTime;
                cameraIsExposing = deviceInfo.IsExposing;
                nextExposureLength = deviceInfo.NextExposureLength;
                lastDownloadTime = deviceInfo.LastDownloadTime == -1 ? lastDownloadTime : deviceInfo.LastDownloadTime;

                try {
                    await guiderService.UpdateCameraInfo(new ProfileCameraState() {
                        ExposureEndTime = exposureEndTime,
                        InstanceId = profileService.ActiveProfile.Id,
                        IsExposing = cameraIsExposing,
                        NextExposureTime = nextExposureLength,
                        LastDownloadTime = lastDownloadTime
                    });
                } catch {
                    // catch everything, handling is done in the loop of the client
                }
            }
        }

        public void Dispose() {
            cameraMediator.RemoveConsumer(this);
        }

        public void SetupDialog() {
        }
    }
}