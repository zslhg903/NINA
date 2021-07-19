#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Locale;
using NINA.Core.Model.Equipment;
using NINA.Core.MyMessageBox;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Model;
using NINA.Image.ImageAnalysis;
using NINA.Image.ImageData;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.ViewModel;

namespace NINA.ViewModel.Imaging {

    internal class ExposureCalculatorVM : DockableVM, IExposureCalculatorVM, ICameraConsumer {
        private double _recommendedExposureTime;
        private FilterInfo _snapFilter;
        private ISharpCapSensorAnalysisReader _sharpCapSensorAnalysisReader;
        private readonly ICameraMediator cameraMediator;
        private CancellationTokenSource _cts;
        private string _sharpCapSensorAnalysisDisabledValue;
        private ImmutableDictionary<string, SharpCapSensorAnalysisData> _sharpCapSensorAnalysisData;

        public ExposureCalculatorVM(IProfileService profileService, IImagingMediator imagingMediator, ISharpCapSensorAnalysisReader sharpCapSensorAnalysisReader,
            ICameraMediator cameraMediator)
            : base(profileService) {
            this._imagingMediator = imagingMediator;
            this.Title = Loc.Instance["LblExposureCalculator"];
            this._sharpCapSensorAnalysisReader = sharpCapSensorAnalysisReader;
            this.cameraMediator = cameraMediator;
            if (Application.Current != null) {
                ImageGeometry = (System.Windows.Media.GeometryGroup)Application.Current.Resources["CalculatorSVG"];
            }

            cameraMediator.RegisterConsumer(this);

            DetermineExposureTimeCommand = new AsyncCommand<bool>(async (o) => {
                cameraMediator.RegisterCaptureBlock(this);
                try {
                    var result = await DetermineExposureTime(o);
                    return result;
                } finally {
                    cameraMediator.ReleaseCaptureBlock(this);
                }
            }, (o) => cameraMediator.IsFreeToCapture(this));
            CancelDetermineExposureTimeCommand = new RelayCommand(TriggerCancelToken);
            DetermineBiasCommand = new AsyncCommand<bool>(async (o) => {
                cameraMediator.RegisterCaptureBlock(this);
                try {
                    var result = await DetermineBias(o);
                    return result;
                } finally {
                    cameraMediator.ReleaseCaptureBlock(this);
                }
            }, (o) => cameraMediator.IsFreeToCapture(this));
            ReloadSensorAnalysisCommand = new AsyncCommand<bool>(ReloadSensorAnalysis);
            CancelDetermineBiasCommand = new RelayCommand(TriggerCancelToken);

            this._sharpCapSensorAnalysisDisabledValue = "(" + Loc.Instance["LblDisabled"] + ")";
            this._sharpCapSensorNames = new ObservableCollection<string>();
            var configuredPath = this.profileService.ActiveProfile.ImageSettings.SharpCapSensorAnalysisFolder;
            if (String.IsNullOrEmpty(configuredPath)) {
                // Attempt load for default configuration only if the directory exists to avoid log spam
                if (Directory.Exists(SharpCapSensorAnalysisConstants.DEFAULT_SHARPCAP_SENSOR_ANALYSIS_PATH)) {
                    LoadSensorAnalysisData(SharpCapSensorAnalysisConstants.DEFAULT_SHARPCAP_SENSOR_ANALYSIS_PATH);
                }
            } else {
                LoadSensorAnalysisData(configuredPath);
            }
        }

        private static ImmutableDictionary<string, SharpCapSensorAnalysisData> ReadSensorAnalysisData(ISharpCapSensorAnalysisReader sharpCapSensorAnalysisReader, string path) {
            try {
                return sharpCapSensorAnalysisReader.Read(path);
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to read SharpCap sensor analysis data");
                return ImmutableDictionary.Create<string, SharpCapSensorAnalysisData>();
            }
        }

        public ImmutableDictionary<string, SharpCapSensorAnalysisData> LoadSensorAnalysisData(string path) {
            SharpCapSensorNames.Clear();
            this._sharpCapSensorAnalysisData = ReadSensorAnalysisData(this._sharpCapSensorAnalysisReader, path);
            if (!this._sharpCapSensorAnalysisData.IsEmpty) {
                SharpCapSensorNames.Add(this._sharpCapSensorAnalysisDisabledValue);
                foreach (var key in this._sharpCapSensorAnalysisData.Keys.OrderBy(x => x)) {
                    SharpCapSensorNames.Add(key);
                }
            }
            RaisePropertyChanged("SharpCapSensorNames");
            return this._sharpCapSensorAnalysisData;
        }

        private void TriggerCancelToken(object obj) {
            _cts?.Cancel();
        }

        private async Task<AllImageStatistics> TakeExposure(double exposureDuration) {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var seq = new CaptureSequence(exposureDuration, CaptureSequence.ImageTypes.SNAPSHOT, SnapFilter, new BinningMode(1, 1), 1);
            seq.Gain = SnapGain;
            var prepareParameters = new PrepareImageParameters(autoStretch: true, detectStars: false);
            var capture = await _imagingMediator.CaptureAndPrepareImage(seq, prepareParameters, _cts.Token, null); //todo progress
            return AllImageStatistics.Create(capture.RawImageData);
        }

        private async Task<bool> DetermineExposureTime(object arg) {
            this.Statistics = null;
            this.Statistics = await TakeExposure(SnapExposureDuration);
            this.CalculateRecommendedExposureTime();
            return true;
        }

        private async Task<bool> DetermineBias(object arg) {
            MyMessageBox.Show(Loc.Instance["LblCoverScopeMsgBoxTitle"]);
            var imageStatistics = await TakeExposure(0);
            this.BiasMedian = (await imageStatistics.ImageStatistics).Median;
            return true;
        }

        private Task<bool> ReloadSensorAnalysis(object obj) {
            var path = String.IsNullOrEmpty(this.profileService.ActiveProfile.ImageSettings.SharpCapSensorAnalysisFolder)
                ? SharpCapSensorAnalysisConstants.DEFAULT_SHARPCAP_SENSOR_ANALYSIS_PATH
                : this.profileService.ActiveProfile.ImageSettings.SharpCapSensorAnalysisFolder;
            var sensorAnalysisData = LoadSensorAnalysisData(path);
            Notification.ShowInformation(String.Format(Loc.Instance["LblSharpCapSensorAnalysisLoadedFormat"], sensorAnalysisData.Count));
            return Task.FromResult(true);
        }

        public IAsyncCommand DetermineExposureTimeCommand { get; private set; }
        public ICommand CancelDetermineExposureTimeCommand { get; private set; }
        public IAsyncCommand DetermineBiasCommand { get; private set; }
        public ICommand CancelDetermineBiasCommand { get; private set; }
        public ICommand ReloadSensorAnalysisCommand { get; private set; }

        private ObservableCollection<string> _sharpCapSensorNames;

        public ObservableCollection<string> SharpCapSensorNames {
            get {
                if (_sharpCapSensorNames == null) {
                    _sharpCapSensorNames = new ObservableCollection<string>();
                }
                return _sharpCapSensorNames;
            }
            set {
                _sharpCapSensorNames = value;
            }
        }

        private void OnGainUpdated(int newValue) {
            if (this.IsSharpCapSensorAnalysisEnabled && newValue >= 0) {
                var analysisData = this._sharpCapSensorAnalysisData[this._mySharpCapSensor];
                var readNoiseEstimate = analysisData.EstimateReadNoise((double)newValue);
                var fullWellCapacityEstimate = analysisData.EstimateFullWellCapacity((double)newValue);
                this.ReadNoise = readNoiseEstimate.EstimatedValue;
                this.FullWellCapacity = fullWellCapacityEstimate.EstimatedValue;
            }
        }

        private void OnSharpCapSensorChanged(string newValue) {
            if (String.IsNullOrEmpty(newValue) || newValue == this._sharpCapSensorAnalysisDisabledValue) {
                this.IsSharpCapSensorAnalysisEnabled = false;
            } else {
                this.IsSharpCapSensorAnalysisEnabled = true;
                if (this.SnapGain < 0) {
                    // If gain isn't set yet, get the first gain value from the sensor analysis to initialize the UI
                    var analysisData = this._sharpCapSensorAnalysisData[this._mySharpCapSensor];
                    this.SnapGain = (int)analysisData.GainData[0].Gain;
                } else {
                    this.OnGainUpdated(this.SnapGain);
                }
            }
        }

        private bool _isSharpCapSensorAnalysisEnabled = false;

        public bool IsSharpCapSensorAnalysisEnabled {
            get => this._isSharpCapSensorAnalysisEnabled;
            set {
                this._isSharpCapSensorAnalysisEnabled = value;
                RaisePropertyChanged();
            }
        }

        private string _mySharpCapSensor;

        public string MySharpCapSensor {
            get => _mySharpCapSensor;

            set {
                _mySharpCapSensor = value;
                this.OnSharpCapSensorChanged(value);
                RaisePropertyChanged();
            }
        }

        public int SnapGain {
            get => profileService.ActiveProfile.ExposureCalculatorSettings.Gain;

            set {
                profileService.ActiveProfile.ExposureCalculatorSettings.Gain = value;
                this.OnGainUpdated(value);
                RaisePropertyChanged();
            }
        }

        public FilterInfo SnapFilter {
            get => _snapFilter;

            set {
                _snapFilter = value;
                RaisePropertyChanged();
            }
        }

        public double SnapExposureDuration {
            get => profileService.ActiveProfile.ExposureCalculatorSettings.ExposureDuration;

            set {
                profileService.ActiveProfile.ExposureCalculatorSettings.ExposureDuration = value;
                RaisePropertyChanged();
            }
        }

        public double FullWellCapacity {
            get => profileService.ActiveProfile.ExposureCalculatorSettings.FullWellCapacity;
            set {
                profileService.ActiveProfile.ExposureCalculatorSettings.FullWellCapacity = value;
                RaisePropertyChanged();
            }
        }

        public double ReadNoise {
            get => profileService.ActiveProfile.ExposureCalculatorSettings.ReadNoise;
            set {
                profileService.ActiveProfile.ExposureCalculatorSettings.ReadNoise = value;
                RaisePropertyChanged();
            }
        }

        public double BiasMedian {
            get => profileService.ActiveProfile.ExposureCalculatorSettings.BiasMedian;
            set {
                profileService.ActiveProfile.ExposureCalculatorSettings.BiasMedian = value;
                RaisePropertyChanged();
            }
        }

        public double RecommendedExposureTime {
            get {
                return _recommendedExposureTime;
            }
            set {
                _recommendedExposureTime = value;
                RaisePropertyChanged();
            }
        }

        private AllImageStatistics _statistics;
        private IImagingMediator _imagingMediator;

        public AllImageStatistics Statistics {
            get {
                return _statistics;
            }
            set {
                _statistics = value;
                RaisePropertyChanged();
            }
        }

        private void CalculateRecommendedExposureTime() {
            if (Statistics.ImageStatistics.Result.Median - BiasMedian < 0) {
                this.Statistics = null;
                Notification.ShowError(Loc.Instance["LblExposureCalculatorMeanLessThanOffset"]);
            } else {
                // Optimal exposure time is: 10 * ReadNoiseSquared / LightPollutionRate
                // Read noise units is electrons per ADU
                // Light pollution units is electrons per ADU per second, which we get by:
                //   1) Subtract the bias mean from the median of a snapshot (= skyglow in ADU)
                //   2) Convert to electrons by multiplying the electrons per ADU
                //   3) Divide by the snapshot exposure duration
                var maxAdu = 1 << Statistics.ImageProperties.BitDepth;
                var electronsPerAdu = FullWellCapacity / maxAdu;
                var skyglowFluxPerSecond = (Statistics.ImageStatistics.Result.Median - BiasMedian) * electronsPerAdu / SnapExposureDuration;
                var readNoiseSquared = ReadNoise * ReadNoise;
                RecommendedExposureTime = 10 * readNoiseSquared / skyglowFluxPerSecond;

                var debugMessage = $@"Recommended exposure calculation
Read Noise = {ReadNoise} electrons
Full Well Capacity = {FullWellCapacity} electrons
Bit Depth = {Statistics.ImageProperties.BitDepth}
Electrons per ADU = {electronsPerAdu}
Skyglow = Image Median ({Statistics.ImageStatistics.Result.Median}) - Bias Median ({BiasMedian}) = {Statistics.ImageStatistics.Result.Median - BiasMedian} ADU
Skyglow Flux per Second = {skyglowFluxPerSecond} electrons per pixel per second
Recommended exposure time = 10 * Read Noise ^ 2 = {RecommendedExposureTime} seconds";
                Logger.Debug(debugMessage);
            }
        }

        private CameraInfo cameraInfo;

        public CameraInfo CameraInfo {
            get => cameraInfo;
            set {
                cameraInfo = value;
                RaisePropertyChanged();
            }
        }

        public void UpdateDeviceInfo(CameraInfo deviceInfo) {
            CameraInfo = deviceInfo;
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}