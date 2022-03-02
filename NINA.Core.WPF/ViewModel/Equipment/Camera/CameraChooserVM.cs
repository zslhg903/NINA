#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using EDSDKLib;
using FLI;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Equipment.MyCamera.ToupTekAlike;
using NINA.Profile.Interfaces;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using QHYCCD;
using System;
using System.Collections.Generic;
using ZWOptical.ASISDK;
using NINA.Equipment.SDK.CameraSDKs.AtikSDK;
using NINA.Equipment.Utility;
using NINA.Core.Locale;
using NINA.Equipment.Equipment;
using NINA.Equipment.Interfaces;
using NINA.WPF.Base.Model.Equipment.MyCamera.Simulator;
using NINA.Equipment.SDK.CameraSDKs.SVBonySDK;
using NINA.Equipment.SDK.CameraSDKs.SBIGSDK;
using NINA.Image.Interfaces;
using NINA.Equipment.SDK.CameraSDKs.PlayerOneSDK;

namespace NINA.WPF.Base.ViewModel.Equipment.Camera {

    public class CameraChooserVM : DeviceChooserVM {
        private readonly ITelescopeMediator telescopeMediator;
        private readonly ISbigSdk sbigSdk;
        private readonly IExposureDataFactory exposureDataFactory;
        private readonly IImageDataFactory imageDataFactory;
        private readonly IDeviceDispatcher deviceDispatcher;

        public CameraChooserVM(
            IProfileService profileService, ITelescopeMediator telescopeMediator, ISbigSdk sbigSdk, IExposureDataFactory exposureDataFactory, IImageDataFactory imageDataFactory, IDeviceDispatcher deviceDispatcher
            ) : base(profileService) {
            this.telescopeMediator = telescopeMediator;
            this.sbigSdk = sbigSdk;
            this.exposureDataFactory = exposureDataFactory;
            this.imageDataFactory = imageDataFactory;
            this.deviceDispatcher = deviceDispatcher;
        }

        public override void GetEquipment() {
            lock (lockObj) {
                var ascomInteraction = new ASCOMInteraction(deviceDispatcher, profileService);

                var devices = new List<IDevice>();

                devices.Add(new DummyDevice(Loc.Instance["LblNoCamera"]));

                /* ASI */
                try {
                    var asiCameras = ASICameras.Count;
                    Logger.Info($"Found {asiCameras} ASI Cameras");
                    for (int i = 0; i < asiCameras; i++) {
                        var cam = ASICameras.GetCamera(i, profileService, exposureDataFactory);
                        if (!string.IsNullOrEmpty(cam.Name)) {
                            Logger.Trace(string.Format("Adding {0}", cam.Name));
                            devices.Add(cam);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Altair */
                try {
                    var altairCameras = Altair.AltairCam.EnumV2();
                    Logger.Info($"Found {altairCameras?.Length} Altair Cameras");
                    foreach (var instance in altairCameras) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new AltairSDKWrapper(), profileService, exposureDataFactory);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Atik */
                try {
                    var atikDevices = AtikCameraDll.GetDevicesCount();
                    Logger.Info($"Found {atikDevices} Atik Cameras");
                    if (atikDevices > 0) {
                        for (int i = 0; i < atikDevices; i++) {
                            var cam = new AtikCamera(i, profileService, exposureDataFactory);
                            devices.Add(cam);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* FLI */
                try {
                    List<string> cameras = FLICameras.GetCameras();
                    Logger.Info($"Found {cameras.Count} FLI Cameras");

                    if (cameras.Count > 0) {
                        foreach (var entry in cameras) {
                            var camera = new FLICamera(entry, profileService, exposureDataFactory);

                            if (!string.IsNullOrEmpty(camera.Name)) {
                                Logger.Debug($"Adding FLI camera {camera.Id} (as {camera.Name})");
                                devices.Add(camera);
                            }
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* QHYCCD */
                try {
                    var qhy = new QHYCameras(exposureDataFactory);
                    uint numCameras = qhy.Count;
                    Logger.Info($"Found {numCameras} QHYCCD Cameras");

                    if (numCameras > 0) {
                        for (uint i = 0; i < numCameras; i++) {
                            var cam = qhy.GetCamera(i, profileService);
                            if (!string.IsNullOrEmpty(cam.Name)) {
                                Logger.Debug($"Adding QHY camera {i}: {cam.Id} (as {cam.Name})");
                                devices.Add(cam);
                            }
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Player One */
                try {
                    var provider = new PlayerOneProvider(profileService, exposureDataFactory);
                    var playerOneCameras = provider.GetEquipment();
                    Logger.Info($"Found {playerOneCameras?.Count} Player One Cameras");
                    devices.AddRange(playerOneCameras);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* ToupTek */
                try {
                    var toupTekCameras = ToupTek.ToupCam.EnumV2();
                    Logger.Info($"Found {toupTekCameras?.Length} ToupTek Cameras");
                    foreach (var instance in toupTekCameras) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new ToupTekSDKWrapper(), profileService, exposureDataFactory);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Omegon */
                try {
                    var omegonCameras = Omegon.Omegonprocam.EnumV2();
                    Logger.Info($"Found {omegonCameras?.Length} Omegon Cameras");
                    foreach (var instance in omegonCameras) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new OmegonSDKWrapper(), profileService, exposureDataFactory);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Risingcam */
                try {
                    var risingCamCameras = Nncam.EnumV2();
                    Logger.Info($"Found {risingCamCameras?.Length} RisingCam Cameras");
                    foreach (var instance in risingCamCameras) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new RisingcamSDKWrapper(), profileService, exposureDataFactory);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* MallinCam */
                try {
                    var mallinCamCameras = MallinCam.MallinCam.EnumV2();
                    Logger.Info($"Found {mallinCamCameras?.Length} MallinCam Cameras");
                    foreach (var instance in mallinCamCameras) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new MallinCamSDKWrapper(), profileService, exposureDataFactory);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* SVBony */
                try {
                    var provider = new SVBonyProvider(profileService, exposureDataFactory);
                    var svBonyCameras = provider.GetEquipment();
                    Logger.Info($"Found {svBonyCameras?.Count} SVBony Cameras");
                    devices.AddRange(svBonyCameras);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* SBIG */
                try {
                    var provider = new SBIGCameraProvider(sbigSdk, profileService, exposureDataFactory);
                    var sbigCameras = provider.GetEquipment();
                    Logger.Info($"Found {sbigCameras?.Count} SBIG Cameras");
                    devices.AddRange(sbigCameras);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* ASCOM */
                try {
                    var ascomCameras = ascomInteraction.GetCameras(exposureDataFactory);
                    foreach (ICamera cam in ascomCameras) {
                        devices.Add(cam);
                    }
                    Logger.Info($"Found {ascomCameras?.Count} ASCOM Cameras");
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* CANON */
                try {
                    IntPtr cameraList;
                    uint err = EDSDK.EdsGetCameraList(out cameraList);
                    if (err == EDSDK.EDS_ERR_OK) {
                        int count;
                        err = EDSDK.EdsGetChildCount(cameraList, out count);

                        Logger.Info($"Found {count} Canon Cameras");
                        for (int i = 0; i < count; i++) {
                            IntPtr cam;
                            err = EDSDK.EdsGetChildAtIndex(cameraList, i, out cam);

                            EDSDK.EdsDeviceInfo info;
                            err = EDSDK.EdsGetDeviceInfo(cam, out info);

                            Logger.Trace(string.Format("Adding {0}", info.szDeviceDescription));
                            devices.Add(new EDCamera(cam, info, profileService, exposureDataFactory));
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* NIKON */
                if (!DllLoader.IsX86()) {
                    try {
                        devices.Add(new NikonCamera(profileService, telescopeMediator, exposureDataFactory));
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }
                }

                devices.Add(new FileCamera(profileService, telescopeMediator, imageDataFactory, exposureDataFactory));
                devices.Add(new SimulatorCamera(profileService, telescopeMediator, exposureDataFactory, imageDataFactory));

                DetermineSelectedDevice(devices, profileService.ActiveProfile.CameraSettings.Id);
            }
        }
    }
}