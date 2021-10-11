#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

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

namespace NINA.WPF.Base.ViewModel.Equipment.Camera {

    public class CameraChooserVM : DeviceChooserVM {
        private readonly ITelescopeMediator telescopeMediator;
        private readonly ISbigSdk sbigSdk;

        public CameraChooserVM(
            IProfileService profileService, ITelescopeMediator telescopeMediator, ISbigSdk sbigSdk
            ) : base(profileService) {
            this.telescopeMediator = telescopeMediator;
            this.sbigSdk = sbigSdk;
        }

        public override void GetEquipment() {
            lock (lockObj) {
                var devices = new List<IDevice>();

                devices.Add(new DummyDevice(Loc.Instance["LblNoCamera"]));

                /* ASI */
                try {
                    Logger.Trace("Adding ASI Cameras");
                    for (int i = 0; i < ASICameras.Count; i++) {
                        var cam = ASICameras.GetCamera(i, profileService);
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
                    Logger.Trace("Adding Altair Cameras");
                    foreach (var instance in Altair.AltairCam.EnumV2()) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new AltairSDKWrapper(), profileService);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Atik */
                try {
                    Logger.Trace("Adding Atik Cameras");
                    var atikDevices = AtikCameraDll.GetDevicesCount();
                    Logger.Trace($"Cameras found: {atikDevices}");
                    if (atikDevices > 0) {
                        for (int i = 0; i < atikDevices; i++) {
                            var cam = new AtikCamera(i, profileService);
                            devices.Add(cam);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* FLI */
                try {
                    Logger.Trace("Adding FLI Cameras");
                    List<string> cameras = FLICameras.GetCameras();

                    if (cameras.Count > 0) {
                        foreach (var entry in cameras) {
                            var camera = new FLICamera(entry, profileService);

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
                    var qhy = new QHYCameras();
                    Logger.Trace("Adding QHYCCD Cameras");
                    uint numCameras = qhy.Count;

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

                /* ToupTek */
                try {
                    Logger.Debug("Adding ToupTek Cameras");
                    foreach (var instance in ToupTek.ToupCam.EnumV2()) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new ToupTekSDKWrapper(), profileService);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Omegon */
                try {
                    Logger.Debug("Adding Omegon Cameras");
                    foreach (var instance in Omegon.Omegonprocam.EnumV2()) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new OmegonSDKWrapper(), profileService);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* Risingcam */
                try {
                    Logger.Debug("Adding RisingCam Cameras");
                    foreach (var instance in Nncam.EnumV2()) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new RisingcamSDKWrapper(), profileService);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* MallinCam */
                try {
                    Logger.Trace("Adding MallinCam Cameras");
                    foreach (var instance in MallinCam.MallinCam.EnumV2()) {
                        var cam = new ToupTekAlikeCamera(instance.ToDeviceInfo(), new MallinCamSDKWrapper(), profileService);
                        devices.Add(cam);
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* SVBony */
                try {
                    var provider = new SVBonyProvider(profileService);
                    devices.AddRange(provider.GetEquipment());
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* SBIG */
                try {
                    var provider = new SBIGCameraProvider(sbigSdk, profileService);
                    devices.AddRange(provider.GetEquipment());
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* ASCOM */
                try {
                    foreach (ICamera cam in ASCOMInteraction.GetCameras(profileService)) {
                        devices.Add(cam);
                    }
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

                        for (int i = 0; i < count; i++) {
                            IntPtr cam;
                            err = EDSDK.EdsGetChildAtIndex(cameraList, i, out cam);

                            EDSDK.EdsDeviceInfo info;
                            err = EDSDK.EdsGetDeviceInfo(cam, out info);

                            Logger.Trace(string.Format("Adding {0}", info.szDeviceDescription));
                            devices.Add(new EDCamera(cam, info, profileService));
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                /* NIKON */
                try {
                    devices.Add(new NikonCamera(profileService, telescopeMediator));
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                devices.Add(new FileCamera(profileService, telescopeMediator));
                devices.Add(new SimulatorCamera(profileService, telescopeMediator));

                DetermineSelectedDevice(devices, profileService.ActiveProfile.CameraSettings.Id);
            }
        }
    }
}