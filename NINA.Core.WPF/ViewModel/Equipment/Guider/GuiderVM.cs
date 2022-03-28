#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Equipment.MyGuider;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using NINA.Equipment.Equipment.MyGuider.PHD2;
using NINA.Core.Utility.Notification;
using NINA.Core.Enum;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.Core.Model;
using NINA.Core.Locale;
using NINA.Core.Interfaces;
using NINA.Equipment.Equipment;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using Nito.AsyncEx;
using System.Linq;
using NINA.Astrometry;

namespace NINA.WPF.Base.ViewModel.Equipment.Guider {

    public class GuiderVM : DockableVM, IGuiderVM {

        public GuiderVM(IProfileService profileService, IGuiderMediator guiderMediator, IApplicationStatusMediator applicationStatusMediator, IDeviceChooserVM deviceChooser) : base(profileService) {
            Title = Loc.Instance["LblGuider"];
            ImageGeometry = (System.Windows.Media.GeometryGroup)System.Windows.Application.Current.Resources["GuiderSVG"];

            this.guiderMediator = guiderMediator;
            this.guiderMediator.RegisterHandler(this);
            GuiderChooserVM = deviceChooser;
            _ = Rescan();

            this.applicationStatusMediator = applicationStatusMediator;

            ConnectGuiderCommand = new AsyncCommand<bool>(() => Task.Run(Connect));
            CancelConnectGuiderCommand = new RelayCommand(CancelConnectGuider);
            RefreshGuiderListCommand = new AsyncCommand<bool>(async o => { await Rescan(); return true; }, o => !GuiderInfo.Connected);
            DisconnectGuiderCommand = new RelayCommand((object o) => Disconnect(), (object o) => GuiderInfo.Connected);
            ClearGraphCommand = new RelayCommand((object o) => ResetGraphValues());
            SetShiftRateCommand = new AsyncCommand<bool>(SetShiftRateVM);
            StopShiftCommand = new AsyncCommand<bool>(StopShiftVM);

            GuideStepsHistory = new GuideStepsHistory(HistorySize, GuiderScale, GuiderMaxY);

            profileService.ProfileChanged += async (object sender, EventArgs e) => {
                await Rescan();
            };
        }

        public async Task<IList<string>> Rescan() {
            return await Task.Run(() => {
                GuiderChooserVM.GetEquipment();
                return GuiderChooserVM.Devices.Select(x => x.Id).ToList();
            });
        }

        public IDeviceChooserVM GuiderChooserVM { get; private set; }

        /// <summary>
        /// Starts recording RMS until StopRMSRecording is called
        /// </summary>
        /// <returns>Handle for the recording rms session</returns>
        public Guid StartRMSRecording() {
            var handle = Guid.NewGuid();
            var rms = new RMS();
            rms.SetScale(GuideStepsHistory.PixelScale);
            recordedRMS.Add(handle, rms);
            return handle;
        }

        /// <summary>
        /// Stops and returns RMS for the given rms session handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns>recorded RMS</returns>
        public RMS StopRMSRecording(Guid handle) {
            if (recordedRMS.ContainsKey(handle)) {
                var rms = recordedRMS[handle];
                recordedRMS.Remove(handle);
                return rms;
            } else {
                return null;
            }
        }

        public async Task<bool> AutoSelectGuideStar(CancellationToken token) {
            if (Guider?.Connected == true) {
                var result = await Guider?.AutoSelectGuideStar();
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                return result;
            } else {
                return false;
            }
        }

        public int HistorySize {
            get {
                return profileService.ActiveProfile.GuiderSettings.PHD2HistorySize;
            }
            set {
                profileService.ActiveProfile.GuiderSettings.PHD2HistorySize = value;
                GuideStepsHistory.HistorySize = value;
                RaisePropertyChanged();
            }
        }

        private static Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;

        private void ResetGraphValues() {
            GuideStepsHistory.Clear();
        }

        public async Task<bool> Connect() {
            ResetGraphValues();

            bool connected = false;

            try {
                if (Guider != null) {
                    Guider.PropertyChanged -= Guider_PropertyChanged;
                    Guider.GuideEvent -= Guider_GuideEvent;
                }
                _cancelConnectGuiderSource?.Dispose();
                _cancelConnectGuiderSource = new CancellationTokenSource();
                Guider = (IGuider)GuiderChooserVM.SelectedDevice;
                Guider.PropertyChanged += Guider_PropertyChanged;
                connected = await Guider.Connect(_cancelConnectGuiderSource.Token);
                _cancelConnectGuiderSource.Token.ThrowIfCancellationRequested();

                if (connected) {
                    Guider.GuideEvent += Guider_GuideEvent;

                    GuiderInfo = new GuiderInfo {
                        Connected = connected,
                        CanClearCalibration = Guider.CanClearCalibration,
                        CanSetShiftRate = Guider.CanSetShiftRate,
                        Name = Guider.Name,
                        Description = Guider.Description,
                        DriverInfo = Guider.DriverInfo,
                        DriverVersion = Guider.DriverVersion,
                        DeviceId = Guider.Id,
                        SupportedActions = Guider.SupportedActions,
                    };
                    BroadcastGuiderInfo();
                    Notification.ShowSuccess(Loc.Instance["LblGuiderConnected"]);
                    RaisePropertyChanged(nameof(Guider));
                    profileService.ActiveProfile.GuiderSettings.GuiderName = Guider.Id;
                }
            } catch (OperationCanceledException) {
                connected = false;
            }

            if (!connected) {
                await Disconnect();
            }

            return connected;
        }

        private void Guider_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "PixelScale") {
                GuideStepsHistory.PixelScale = Guider.PixelScale;
            }

            if (e.PropertyName == nameof(IGuider.Connected)) {
                GuiderInfo.Connected = Guider.Connected;
                BroadcastGuiderInfo();
                if (!GuiderInfo.Connected) {
                    Disconnect();
                }
            }
        }

        private void Guider_GuideEvent(object sender, IGuideStep e) {
            try {
                var step = e;

                GuideStepsHistory.AddGuideStep(step);

                foreach (RMS rms in recordedRMS.Values) {
                    rms.AddDataPoint(step.RADistanceRaw, step.DECDistanceRaw);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public Task Disconnect() {
            if (Guider != null) {
                Guider.PropertyChanged -= Guider_PropertyChanged;
                Guider.GuideEvent -= Guider_GuideEvent;
            }
            Guider?.Disconnect();
            Guider = null;
            GuiderInfo = DeviceInfo.CreateDefaultInstance<GuiderInfo>();
            BroadcastGuiderInfo();
            return Task.CompletedTask;
        }

        private GuiderInfo guiderInfo;

        public GuiderInfo GuiderInfo {
            get {
                if (guiderInfo == null) {
                    guiderInfo = DeviceInfo.CreateDefaultInstance<GuiderInfo>();
                }
                return guiderInfo;
            }
            set {
                guiderInfo = value;
                RaisePropertyChanged();
            }
        }

        private void BroadcastGuiderInfo() {
            this.guiderMediator.Broadcast(GuiderInfo);
        }

        public GuiderScaleEnum GuiderScale {
            get {
                return profileService.ActiveProfile.GuiderSettings.PHD2GuiderScale;
            }
            set {
                profileService.ActiveProfile.GuiderSettings.PHD2GuiderScale = value;
                GuideStepsHistory.Scale = value;
            }
        }

        public double GuiderMaxY {
            get {
                return profileService.ActiveProfile.GuiderSettings.MaxY;
            }
            set {
                profileService.ActiveProfile.GuiderSettings.MaxY = value;
                GuideStepsHistory.MaxY = value;
            }
        }

        public async Task<bool> StartGuiding(bool forceCalibration, IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (Guider?.Connected == true) {
                IProgress<ApplicationStatus> prog = new Progress<ApplicationStatus>(p => {
                    p.Source = Loc.Instance["LblGuider"];
                    this.applicationStatusMediator.StatusUpdate(p);
                });
                try {
                    progress?.Report(new ApplicationStatus { Status = Loc.Instance["LblStartGuiding"] });
                    var guiding = await Guider.StartGuiding(forceCalibration, prog, token);
                    return guiding;
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception ex) {
                    Logger.Error(ex);
                    return false;
                } finally {
                    progress?.Report(new ApplicationStatus { Status = string.Empty });
                }
            } else {
                return false;
            }
        }

        public async Task<bool> StopGuiding(CancellationToken token) {
            if (Guider?.Connected == true) {
                return await Guider.StopGuiding(token);
            } else {
                return false;
            }
        }

        public async Task<bool> ClearCalibration(CancellationToken token) {
            if (Guider?.Connected == true) {
                return await Guider.ClearCalibration(token);
            } else {
                return false;
            }
        }

        public async Task<bool> Dither(CancellationToken token) {
            if (Guider?.Connected == true) {
                IProgress<ApplicationStatus> prog = new Progress<ApplicationStatus>(p => {
                    p.Source = Loc.Instance["LblDither"];
                    this.applicationStatusMediator.StatusUpdate(p);
                });
                try {
                    Guider.GuideEvent -= Guider_GuideEvent;
                    applicationStatusMediator.StatusUpdate(new ApplicationStatus() { Status = Loc.Instance["LblDither"], Source = Title });
                    GuideStepsHistory.AddDitherIndicator();
                    await Guider.Dither(prog, token);
                } finally {
                    Guider.GuideEvent += Guider_GuideEvent;
                    applicationStatusMediator.StatusUpdate(new ApplicationStatus() { Status = string.Empty, Source = Title });
                }
                return true;
            } else {
                await Disconnect();
                return false;
            }
        }

        public GuiderInfo GetDeviceInfo() {
            return GuiderInfo;
        }

        private GuideStepsHistory guideStepsHistory;

        public GuideStepsHistory GuideStepsHistory {
            get {
                return guideStepsHistory;
            }
            private set {
                guideStepsHistory = value;
                RaisePropertyChanged();
            }
        }

        private Dictionary<Guid, RMS> recordedRMS = new Dictionary<Guid, RMS>();

        private IGuider guider;

        public IGuider Guider {
            get => guider;
            set {
                guider = value;
                RaisePropertyChanged();
            }
        }

        private double raShiftRate = 0.0d;
        public double RAShiftRate {
            get => raShiftRate;
            set {
                raShiftRate = value;
                RaisePropertyChanged();
            }
        }

        private double decShiftRate = 0.0d;
        public double DecShiftRate {
            get => decShiftRate;
            set {
                decShiftRate = value;
                RaisePropertyChanged();
            }
        }

        private void CancelConnectGuider(object o) {
            _cancelConnectGuiderSource?.Cancel();
        }

        public string Action(string actionName, string actionParameters = "") {
            return GuiderInfo?.Connected == true ? Guider.Action(actionName, actionParameters) : null;
        }

        public string SendCommandString(string command, bool raw = true) {
            return GuiderInfo?.Connected == true ? Guider.SendCommandString(command, raw) : null;
        }

        public bool SendCommandBool(string command, bool raw = true) {
            return GuiderInfo?.Connected == true ? Guider.SendCommandBool(command, raw) : false;
        }

        public void SendCommandBlind(string command, bool raw = true) {
            if (GuiderInfo?.Connected == true) {
                Guider.SendCommandBlind(command, raw);
            }
        }

        private Task<bool> SetShiftRateVM() {
            return Task.Run(async () => {
                try {
                    var shiftRate = SiderealShiftTrackingRate.Create(RAShiftRate, DecShiftRate);
                    return await SetShiftRate(shiftRate, CancellationToken.None);
                } catch (Exception e) {
                    Notification.ShowError($"Set shift rate failed. {e.Message}");
                    Logger.Error(e, "Failed to set shift rate");
                    return false;
                }
            });
        }

        private Task<bool> StopShiftVM() {
            return Task.Run(async () => {
                try {
                    return await StopShifting(CancellationToken.None);
                } catch (Exception e) {
                    Notification.ShowError($"Stop shifting failed. {e.Message}");
                    Logger.Error(e, "Failed to stop shifting");
                    return false;
                }
            });
        }

        public async Task<bool> SetShiftRate(SiderealShiftTrackingRate shiftTrackingRate, CancellationToken ct) {
            if (!Guider.Connected) {
                Logger.Error("Attempted to set shift rate when guider is not connected");
                return false;
            }
            if (!Guider.CanSetShiftRate) {
                Logger.Error("Attempted to set shift rate when guider does not support it");
                return false;
            }

            return await Guider.SetShiftRate(shiftTrackingRate, ct);
        }

        public async Task<bool> StopShifting(CancellationToken ct) {
            if (!Guider.Connected) {
                Logger.Error("Attempted to disable shift when guider is not connected");
                return false;
            }
            if (!Guider.ShiftEnabled) {
                Logger.Info("Guider shifter is not enabled. Nothing to disable");
                return true;
            }

            await Guider.StopShifting(ct);
            return true;
        }

        private IGuiderMediator guiderMediator;
        private IApplicationStatusMediator applicationStatusMediator;
        private CancellationTokenSource _cancelConnectGuiderSource;

        public ICommand ConnectGuiderCommand { get; private set; }
        public ICommand RefreshGuiderListCommand { get; private set; }
        public ICommand DisconnectGuiderCommand { get; private set; }
        public ICommand ClearGraphCommand { get; private set; }
        public ICommand CancelConnectGuiderCommand { get; }
        public ICommand SetShiftRateCommand { get; private set; }
        public ICommand StopShiftCommand { get; private set; }
    }
}