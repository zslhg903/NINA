#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Enum;
using NINA.Equipment.SDK.CameraSDKs.AtikSDK;
using NINA.Core.Model.Equipment;
using NINA.Image.Interfaces;
using NINA.Equipment.Model;
using NINA.Equipment.Interfaces;

namespace NINA.Equipment.Equipment.MyCamera {

    public class AtikCamera : BaseINPC, ICamera {

        public AtikCamera(int id, IProfileService profileService, IExposureDataFactory exposureDataFactory) {
            this.profileService = profileService;
            this.exposureDataFactory = exposureDataFactory;
            _cameraId = id;
            Name = AtikCameraDll.GetDeviceName(_cameraId);
        }

        private int _cameraId;
        private IntPtr _cameraP = IntPtr.Zero;

        public string Category { get; } = "Atik";

        private AtikCameraDll.ArtemisPropertiesStruct _info;

        private AtikCameraDll.ArtemisPropertiesStruct Info {
            get {
                return _info;
            }
        }

        public bool HasShutter {
            get {
                var bitNumber = 5;
                var bit = (Info.cameraflags & (1 << bitNumber - 1)) != 0;
                return bit;
            }
        }

        public bool Connected {
            get {
                return _cameraP == IntPtr.Zero ? false : AtikCameraDll.IsConnected(_cameraP);
            }
        }

        public double Temperature {
            get {
                return AtikCameraDll.GetTemperature(_cameraP);
            }
        }

        public bool CanShowLiveView { get => false; }

        private double _temperature;

        public double TemperatureSetPoint {
            get {
                _temperature = AtikCameraDll.GetSetpoint(_cameraP);
                return _temperature;
            }

            set {
                if (CanSetTemperature) {
                    _temperature = value;
                    if (CoolerOn) {
                        AtikCameraDll.SetCooling(_cameraP, _temperature);
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public bool CanSubSample {
            get {
                var bitNumber = 5;
                var bit = (Info.cameraflags & (1 << bitNumber - 1)) != 0;
                return bit;
            }
        }

        public bool EnableSubSample { get; set; }
        public int SubSampleX { get; set; }
        public int SubSampleY { get; set; }
        public int SubSampleWidth { get; set; }
        public int SubSampleHeight { get; set; }

        private bool _coolerOn;

        public bool CoolerOn {
            get {
                return _coolerOn;
            }
            set {
                try {
                    if (Connected) {
                        if (_coolerOn != value) {
                            _coolerOn = value;
                            if (_coolerOn == false) {
                                AtikCameraDll.SetWarmup(_cameraP);
                            } else {
                                AtikCameraDll.SetCooling(_cameraP, _temperature);
                            }
                        }
                        RaisePropertyChanged();
                    }
                } catch (Exception) {
                    _coolerOn = false;
                }
            }
        }

        public short BinX {
            get {
                AtikCameraDll.GetBinning(_cameraP, out var x, out var y);
                return (short)x;
            }

            set {
                if (value < MaxBinX) {
                    AtikCameraDll.SetBinning(_cameraP, value, value);
                    RaisePropertyChanged();
                }
            }
        }

        public short BinY {
            get {
                AtikCameraDll.GetBinning(_cameraP, out var x, out var y);
                return (short)y;
            }

            set {
                if (value < MaxBinY) {
                    AtikCameraDll.SetBinning(_cameraP, value, value);
                    RaisePropertyChanged();
                }
            }
        }

        public string Description {
            get {
                return CleanedUpString(Info.Manufacturer) + " " + Name + " (SerialNo: " + AtikCameraDll.GetSerialNumber(_cameraP) + ")";
            }
        }

        public string DriverInfo {
            get {
                return AtikCameraDll.DriverName;
            }
        }

        public string DriverVersion {
            get {
                return AtikCameraDll.DriverVersion;
            }
        }

        public string SensorName {
            get {
                return string.Empty;
            }
        }

        public SensorType SensorType {
            get {
                return AtikCameraDll.GetColorInformation(_cameraP);
            }
        }

        public short BayerOffsetX => 0;

        public short BayerOffsetY => 0;

        public int CameraXSize {
            get {
                return Info.nPixelsX;
            }
        }

        public int CameraYSize {
            get {
                return Info.nPixelsY;
            }
        }

        public double ExposureMin {
            get {
                return 0;
            }
        }

        public double ExposureMax {
            get {
                return double.PositiveInfinity;
            }
        }

        public double ElectronsPerADU => double.NaN;

        public short MaxBinX {
            get {
                AtikCameraDll.GetMaxBinning(_cameraP, out var x, out var y);
                return (short)x > 10 ? (short)10 : (short)x;
            }
        }

        public short MaxBinY {
            get {
                AtikCameraDll.GetMaxBinning(_cameraP, out var x, out var y);
                return (short)y > 10 ? (short)10 : (short)y;
            }
        }

        public double PixelSizeX {
            get {
                return Info.PixelMicronsX;
            }
        }

        public double PixelSizeY {
            get {
                return Info.PixelMicronsY;
            }
        }

        public bool CanSetTemperature {
            get {
                return AtikCameraDll.HasCooler(_cameraP);
            }
        }

        public double CoolerPower {
            get {
                if (CanSetTemperature) {
                    return AtikCameraDll.CoolerPower(_cameraP);
                } else {
                    return double.NaN;
                }
            }
        }

        public bool HasDewHeater {
            get {
                return false;
            }
        }

        public bool DewHeaterOn {
            get {
                return false;
            }
            set {
            }
        }

        public CameraStates CameraState {
            get {
                CameraStates state = CameraStates.Idle;

                switch (AtikCameraDll.CameraState(_cameraP)) {
                    case AtikCameraDll.ArtemisCameraStateEnum.CAMERA_EXPOSING:
                        state = CameraStates.Exposing;
                        break;

                    case AtikCameraDll.ArtemisCameraStateEnum.CAMERA_WAITING:
                    case AtikCameraDll.ArtemisCameraStateEnum.CAMERA_FLUSHING:
                        state = CameraStates.Waiting;
                        break;

                    case AtikCameraDll.ArtemisCameraStateEnum.CAMERA_DOWNLOADING:
                        state = CameraStates.Download;
                        break;

                    case AtikCameraDll.ArtemisCameraStateEnum.CAMERA_ERROR:
                        state = CameraStates.Error;
                        break;
                }

                return state;
            }
        }

        public bool CanSetOffset {
            get {
                return false;
            }
        }

        public int OffsetMin {
            get {
                return 0;
            }
        }

        public int OffsetMax {
            get {
                return 0;
            }
        }

        public bool CanSetUSBLimit {
            get {
                return false;
            }
        }

        public int Offset {
            get {
                return -1;
            }
            set {
            }
        }

        public int USBLimit {
            get {
                return -1;
            }
            set {
            }
        }

        public int USBLimitMax => -1;
        public int USBLimitMin => -1;
        public int USBLimitStep => -1;

        public bool CanGetGain {
            get {
                return false;
            }
        }

        public bool CanSetGain {
            get {
                return false;
            }
        }

        public int GainMax {
            get {
                return -1;
            }
        }

        public int GainMin {
            get {
                return -1;
            }
        }

        public int Gain {
            get {
                return -1;
            }

            set {
            }
        }

        public IList<int> Gains {
            get {
                return new List<int>();
            }
        }

        public IList<string> ReadoutModes => new List<string> { "Default" };

        public short ReadoutMode {
            get => 0;
            set { }
        }

        private short _readoutModeForSnapImages;

        public short ReadoutModeForSnapImages {
            get => _readoutModeForSnapImages;
            set {
                _readoutModeForSnapImages = value;
                RaisePropertyChanged();
            }
        }

        private short _readoutModeForNormalImages;

        public short ReadoutModeForNormalImages {
            get => _readoutModeForNormalImages;
            set {
                _readoutModeForNormalImages = value;
                RaisePropertyChanged();
            }
        }

        public int BitDepth {
            get {
                //currently unknown if the values are stretched to 16 bit or not. Take profile value
                return (int)profileService.ActiveProfile.CameraSettings.BitDepth;
            }
        }

        private AsyncObservableCollection<BinningMode> _binningModes;

        public AsyncObservableCollection<BinningMode> BinningModes {
            get {
                if (_binningModes == null) {
                    _binningModes = new AsyncObservableCollection<BinningMode>();
                    for (short i = 1; i <= MaxBinX; i++) {
                        _binningModes.Add(new BinningMode(i, i));
                    }
                }
                return _binningModes;
            }
        }

        public bool HasSetupDialog {
            get {
                return false;
            }
        }

        public string Id => Name;

        public string Name { get; private set; }

        private string CleanedUpString(char[] values) {
            return string.Join("", values.Take(Array.IndexOf(values, '\0')));
        }

        public void AbortExposure() {
            AtikCameraDll.AbortExposure(_cameraP);
        }

        public async Task<bool> Connect(CancellationToken token) {
            return await Task.Run(() => {
                var success = false;
                try {
                    _cameraP = AtikCameraDll.Connect(_cameraId);
                    _info = AtikCameraDll.GetCameraProperties(_cameraP);

                    if (CanSetTemperature) {
                        TemperatureSetPoint = 20;
                    }

                    RaisePropertyChanged(nameof(BinningModes));
                    RaisePropertyChanged(nameof(Connected));
                    success = true;
                } catch (Exception e) {
                    Logger.Error(e);
                    Notification.ShowError(e.Message);
                }

                return success;
            });
        }

        public void Disconnect() {
            try {
                AtikCameraDll.ArtemisCoolerWarmUp(_cameraP);
            } catch (Exception) { }
            AtikCameraDll.Disconnect(_cameraP);
            _cameraP = IntPtr.Zero;
            _binningModes = null;
            RaisePropertyChanged(nameof(Connected));
        }

        public async Task WaitUntilExposureIsReady(CancellationToken token) {
            using (token.Register(() => AbortExposure())) {
                while (!AtikCameraDll.ImageReady(_cameraP)) {
                    await Task.Delay(100, token);
                }
            }
        }

        public async Task<IExposureData> DownloadExposure(CancellationToken token) {
            using (MyStopWatch.Measure("ATIK Download")) {
                return await Task.Run<IExposureData>(async () => {
                    try {
                        while (!AtikCameraDll.ImageReady(_cameraP)) {
                            await Task.Delay(100, token);
                        }

                        return AtikCameraDll.DownloadExposure(_cameraP, BitDepth, SensorType != SensorType.Monochrome, exposureDataFactory);
                    } catch (OperationCanceledException) {
                    } catch (Exception ex) {
                        Logger.Error(ex);
                        Notification.ShowError(ex.Message);
                    }
                    return null;
                });
            }
        }

        public void SetBinning(short x, short y) {
            AtikCameraDll.SetBinning(_cameraP, x, y);
        }

        public void SetupDialog() {
        }

        public void StartExposure(CaptureSequence sequence) {
            do {
                System.Threading.Thread.Sleep(100);
            } while (AtikCameraDll.CameraState(_cameraP) != AtikCameraDll.ArtemisCameraStateEnum.CAMERA_IDLE);
            if (EnableSubSample) {
                AtikCameraDll.SetSubFrame(_cameraP, SubSampleX, SubSampleY, SubSampleWidth, SubSampleHeight);
            } else {
                AtikCameraDll.SetSubFrame(_cameraP, 0, 0, CameraXSize, CameraYSize);
            }

            var isLightFrame = !(sequence.ImageType == CaptureSequence.ImageTypes.DARK ||
                  sequence.ImageType == CaptureSequence.ImageTypes.BIAS ||
                  sequence.ImageType == CaptureSequence.ImageTypes.DARKFLAT);

            if (HasShutter) {
                AtikCameraDll.SetDarkMode(_cameraP, !isLightFrame);
            }

            AtikCameraDll.StartExposure(_cameraP, sequence.ExposureTime);
        }

        public void StopExposure() {
            AtikCameraDll.StopExposure(_cameraP);
        }

        public void StartLiveView() {
            throw new NotImplementedException();
        }

        public Task<IExposureData> DownloadLiveView(CancellationToken token) {
            throw new NotImplementedException();
        }

        public void StopLiveView() {
            throw new NotImplementedException();
        }

        private IProfileService profileService;
        private readonly IExposureDataFactory exposureDataFactory;
        public bool LiveViewEnabled { get => false; set => throw new NotImplementedException(); }

        public int BatteryLevel => -1;

        public bool HasBattery => false;
    }
}