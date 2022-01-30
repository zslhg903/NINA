#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Image.ImageData;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Profile.Interfaces;
using NINA.Core.Utility;
using NINA.Astrometry;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Core.Utility.WindowService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NINA.Core.Model.Equipment;
using NINA.Image.RawConverter;
using NINA.Image.Interfaces;
using NINA.Equipment.Model;
using NINA.Equipment.Interfaces;
using NINA.WPF.Base.SkySurvey;

namespace NINA.WPF.Base.Model.Equipment.MyCamera.Simulator {

    public class SimulatorCamera : BaseINPC, ICamera, ITelescopeConsumer {

        public SimulatorCamera(IProfileService profileService, ITelescopeMediator telescopeMediator, IExposureDataFactory exposureDataFactory, IImageDataFactory imageDataFactory) {
            this.profileService = profileService;
            this.telescopeMediator = telescopeMediator;
            this.exposureDataFactory = exposureDataFactory;
            this.imageDataFactory = imageDataFactory;

            LoadImageCommand = new AsyncCommand<bool>(() => LoadImageDialog()/*, (object o) => Settings.ImageSettings.RAWImageStream == null*/);
            //UnloadImageCommand = new RelayCommand((object o) => Settings.ImageSettings.Image = null);
            //LoadRAWImageCommand = new AsyncCommand<bool>(() => LoadRAWImage(), (object o) => Settings.ImageSettings.Image == null);
            //UnloadRAWImageCommand = new RelayCommand((object o) => { Settings.ImageSettings.RAWImageStream.Dispose(); Settings.ImageSettings.RAWImageStream = null; });

            var p = new NINA.Profile.PluginOptionsAccessor(profileService, Guid.Parse("65CA9FD7-8984-4609-B206-86F3F9E8C19D"));
            settings = new Settings(p);

        }

        private Settings settings;

        public Settings Settings {
            get => settings;
            set {
                settings = value;
                RaisePropertyChanged();
            }
        }

        public string Category { get; } = "N.I.N.A.";

        public bool HasShutter {
            get {
                return false;
            }
        }

        public bool Connected { get; private set; }

        public double CCDTemperature {
            get {
                return SimulatorImage?.RawImageData?.MetaData?.Camera.Temperature ?? double.NaN;
            }
        }

        public double SetCCDTemperature {
            get {
                return double.NaN;
            }

            set {
            }
        }

        public short BinX {
            get {
                return -1;
            }

            set {
            }
        }

        public short BinY {
            get {
                return -1;
            }

            set {
            }
        }

        public string Description {
            get {
                return "A basic simulator to generate random noise for a specific median or load in an image that will be displayed on capture";
            }
        }

        public string DriverInfo {
            get {
                return string.Empty;
            }
        }

        public string DriverVersion {
            get {
                return CoreUtil.Version;
            }
        }

        public string SensorName {
            get {
                return "Simulated Sensor";
            }
        }

        public SensorType SensorType {
            get {
                return SimulatorImage?.RawImageData?.MetaData?.Camera.SensorType ?? SensorType.Monochrome;
            }
        }

        public short BayerOffsetX => 0;

        public short BayerOffsetY => 0;

        public int CameraXSize {
            get {
                return SimulatorImage?.RawImageData?.Properties?.Width ?? Settings.RandomSettings.ImageWidth;
            }
        }

        public int CameraYSize {
            get {
                return SimulatorImage?.RawImageData?.Properties?.Height ?? Settings.RandomSettings.ImageHeight;
            }
        }

        public double ExposureMin {
            get {
                return 0;
            }
        }

        public double ExposureMax {
            get {
                return double.MaxValue;
            }
        }

        public double ElectronsPerADU => double.NaN;

        public short MaxBinX {
            get {
                return 1;
            }
        }

        public short MaxBinY {
            get {
                return 1;
            }
        }

        public double PixelSizeX {
            get {
                return SimulatorImage?.RawImageData?.MetaData?.Camera?.PixelSize ?? profileService.ActiveProfile.CameraSettings.PixelSize;
            }
        }

        public double PixelSizeY {
            get {
                return SimulatorImage?.RawImageData?.MetaData?.Camera?.PixelSize ?? profileService.ActiveProfile.CameraSettings.PixelSize;
            }
        }

        public bool CanSetCCDTemperature {
            get {
                return false;
            }
        }

        public bool CoolerOn {
            get {
                return false;
            }

            set {
            }
        }

        public double CoolerPower {
            get {
                return double.NaN;
            }
        }

        public CameraStates CameraState { get => CameraStates.NoState; }

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

        private AsyncObservableCollection<BinningMode> binningModes;

        public AsyncObservableCollection<BinningMode> BinningModes {
            get {
                if (binningModes == null) {
                    binningModes = new AsyncObservableCollection<BinningMode> {
                        new BinningMode(1,1)
                    };
                }
                return binningModes;
            }
        }

        public bool HasSetupDialog {
            get {
                return true;
            }
        }

        public string Id {
            get {
                return "4C0BBF74-0D95-41F6-AAD8-D6D58668CF2C";
            }
        }

        public string Name {
            get {
                string cameraName = "N.I.N.A. Simulator Camera";

                if (SimulatorImage?.RawImageData?.MetaData?.Camera.Name.Length > 0) {
                    cameraName = cameraName + " (" + SimulatorImage.RawImageData.MetaData.Camera.Name + ")";
                }

                return cameraName;
            }
        }

        public double Temperature {
            get {
                return SimulatorImage?.RawImageData?.MetaData?.Camera.Temperature ?? double.NaN;
            }
        }

        public double TemperatureSetPoint {
            get {
                return double.NaN;
            }

            set {
                throw new NotImplementedException();
            }
        }

        public bool CanSetTemperature {
            get {
                return false;
            }
        }

        public bool CanSubSample {
            get {
                return false;
            }
        }

        public bool EnableSubSample {
            get {
                return false;
            }

            set {
            }
        }

        public int SubSampleX { get; set; }

        public int SubSampleY { get; set; }

        public int SubSampleWidth { get; set; }

        public int SubSampleHeight { get; set; }

        public bool CanShowLiveView {
            get {
                return true;
            }
        }

        private bool _liveViewEnabled;

        public bool LiveViewEnabled {
            get {
                return _liveViewEnabled;
            }
            set {
                _liveViewEnabled = value;
                RaisePropertyChanged();
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

        public bool HasBattery {
            get {
                return false;
            }
        }

        public int BatteryLevel {
            get {
                return -1;
            }
        }

        public int BitDepth {
            get {
                return (int)profileService.ActiveProfile.CameraSettings.BitDepth;
            }
        }

        public IList<string> ReadoutModes => new List<string> { "Default" };

        public short ReadoutMode {
            get => 0;
            set { }
        }

        public short ReadoutModeForSnapImages {
            get {
                return 0;
            }

            set {
            }
        }

        public short ReadoutModeForNormalImages {
            get {
                return 0;
            }

            set {
            }
        }

        public void AbortExposure() {
        }

        public async Task<bool> Connect(CancellationToken token) {
            return await Task.Run(() => {
                telescopeMediator.RegisterConsumer(this);
                Connected = true;
                return true;
            }, token);
        }

        public void Disconnect() {
            this.telescopeMediator.RemoveConsumer(this);
            this.SimulatorImage = null;
            Connected = false;
        }

        public async Task WaitUntilExposureIsReady(CancellationToken token) {
            using (token.Register(() => AbortExposure())) {
                var remaining = exposureTime - (DateTime.Now - exposureStart);
                await Task.Delay(remaining, token);
            }
        }

        public async Task<IExposureData> DownloadExposure(CancellationToken token) {
            switch (Settings.Type) {
                case CameraType.RANDOM:
                    int width, height, mean, stdev;

                    width = Settings.RandomSettings.ImageWidth;
                    height = Settings.RandomSettings.ImageHeight;
                    mean = Settings.RandomSettings.ImageMean;
                    stdev = Settings.RandomSettings.ImageStdDev;

                    ushort[] input = new ushort[width * height];

                    Random rand = new Random();
                    for (int i = 0; i < width * height; i++) {
                        double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                        double u2 = 1.0 - rand.NextDouble();
                        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                        double randNormal = mean + stdev * randStdNormal; //random normal(mean,stdDev^2)
                        input[i] = (ushort)randNormal;
                    }

                    return exposureDataFactory.CreateImageArrayExposureData(
                        input: input,
                        width: width,
                        height: height,
                        bitDepth: this.BitDepth,
                        isBayered: false,
                        metaData: new ImageMetaData());

                case CameraType.IMAGE:
                    if (SimulatorImage == null && !string.IsNullOrWhiteSpace(Settings.ImageSettings.ImagePath)) {
                        await LoadImage(Settings.ImageSettings.ImagePath);
                    }

                    if (SimulatorImage != null) {
                        return exposureDataFactory.CreateCachedExposureData(SimulatorImage.RawImageData);
                    }

                    //if (Settings.ImageSettings.RAWImageStream != null) {
                    //    byte[] rawBytes = Settings.ImageSettings.RAWImageStream.ToArray();
                    //    Settings.ImageSettings.RAWImageStream.Position = 0;
                    //    return exposureDataFactory.CreateRAWExposureData(
                    //        converter: profileService.ActiveProfile.CameraSettings.RawConverter,
                    //        rawBytes: rawBytes,
                    //        rawType: Settings.ImageSettings.RawType,
                    //        bitDepth: this.BitDepth,
                    //        metaData: new ImageMetaData());
                    //}
                    throw new Exception("No Image source set in Simulator!");

                case CameraType.SKYSURVEY:
                    if (!telescopeInfo.Connected) {
                        throw new Exception("Telescope is not connected to get reference coordinates for Simulator Camera Image");
                    }

                    var survey = new ESOSkySurvey();
                    var initial = telescopeInfo.Coordinates.Transform(Astrometry.Epoch.J2000);

                    var coordinates = new Coordinates(Angle.ByDegree(initial.RADegrees + AstroUtil.ArcsecToDegree(Settings.SkySurveySettings.RAError)), Angle.ByDegree(initial.Dec + AstroUtil.ArcsecToDegree(Settings.SkySurveySettings.DecError)), Epoch.J2000);

                    var altAz = coordinates.Transform(Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Latitude), Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Longitude));

                    if (Settings.SkySurveySettings.AzShift != 0 || Settings.SkySurveySettings.AltShift != 0) {
                        coordinates = new TopocentricCoordinates(altAz.Azimuth + Angle.ByDegree(Settings.SkySurveySettings.AzShift / 60d / 60d), altAz.Altitude + Angle.ByDegree(Settings.SkySurveySettings.AltShift / 60d / 60d), altAz.Latitude, altAz.Longitude).Transform(Epoch.J2000);
                    }

                    if (!ImageCache.TryGetValue(coordinates.ToString(), out var image)) {
                        image = await survey.GetImage(string.Empty, coordinates, AstroUtil.DegreeToArcmin(settings.SkySurveySettings.FieldOfView), 2500, 2500, token, default);
                        ImageCache[coordinates.ToString()] = image;
                    }
                    var data = await exposureDataFactory.CreateImageArrayExposureDataFromBitmapSource(image.Image);

                    var arcsecPerPix = image.Image.PixelWidth / AstroUtil.ArcminToArcsec(image.FoVWidth);
                    // Assume a fixed pixel size
                    var pixelSize = 3.76;
                    var factor = AstroUtil.DegreeToArcsec(AstroUtil.ToDegree(1)) / 1000d;
                    // Calculate focal length based on assumed pixel size and result image
                    var focalLength = (factor * pixelSize) / arcsecPerPix;

                    profileService.ActiveProfile.CameraSettings.PixelSize = 3.8;
                    profileService.ActiveProfile.TelescopeSettings.FocalLength = (int)focalLength;

                    return data;
            }
            throw new NotSupportedException();
        }

        private Dictionary<string, SkySurveyImage> ImageCache = new Dictionary<string, SkySurveyImage>();

        private IProfileService profileService;
        private ITelescopeMediator telescopeMediator;
        private readonly IExposureDataFactory exposureDataFactory;
        private readonly IImageDataFactory imageDataFactory;

        public void SetBinning(short x, short y) {
        }

        private IWindowService windowService;

        public IWindowService WindowService {
            get {
                if (windowService == null) {
                    windowService = new WindowService();
                }
                return windowService;
            }
            set {
                windowService = value;
            }
        }

        public void SetupDialog() {
            WindowService.Show(this, "Simulator Setup", System.Windows.ResizeMode.NoResize, System.Windows.WindowStyle.ToolWindow);
        }

        private async Task<bool> LoadImageDialog() {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Load Image";
            dialog.FileName = "Image";
            dialog.DefaultExt = ".tiff";

            if (dialog.ShowDialog() == true) {
                settings.ImageSettings.ImagePath = dialog.FileName;
                await LoadImage(dialog.FileName);

                return true;
            }
            return false;
        }

        private async Task LoadImage(string path) {
            if(File.Exists(path)) { 
                var rawData = await imageDataFactory.CreateFromFile(path, BitDepth, Settings.ImageSettings.IsBayered, profileService.ActiveProfile.CameraSettings.RawConverter);
                this.SimulatorImage = rawData.RenderImage();

                if (SimulatorImage.RawImageData.MetaData?.Camera.SensorType != SensorType.Monochrome) {
                    Settings.ImageSettings.IsBayered = true;
                }
            }
        }

        public IAsyncCommand LoadImageCommand { get; private set; }
        public ICommand UnloadImageCommand { get; private set; }
        public IAsyncCommand LoadRAWImageCommand { get; private set; }
        public ICommand UnloadRAWImageCommand { get; private set; }
        public IRenderedImage SimulatorImage { get; private set; }

        private TelescopeInfo telescopeInfo;

        private DateTime exposureStart;
        private TimeSpan exposureTime;

        public void StartExposure(CaptureSequence captureSequence) {
            exposureStart = DateTime.Now;
            exposureTime = TimeSpan.FromSeconds(captureSequence.ExposureTime);
        }

        public void StopExposure() {
        }

        public void UpdateValues() {
        }

        public void StartLiveView() {
            LiveViewEnabled = true;
        }

        public Task<IExposureData> DownloadLiveView(CancellationToken token) {
            return DownloadExposure(token);
        }

        public void StopLiveView() {
            LiveViewEnabled = false;
        }

        public void UpdateDeviceInfo(TelescopeInfo deviceInfo) {
            this.telescopeInfo = deviceInfo;
        }

        public void Dispose() {
            this.telescopeMediator.RemoveConsumer(this);
        }
    }
}