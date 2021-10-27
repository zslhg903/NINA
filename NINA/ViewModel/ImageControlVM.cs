#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Equipment.MyCamera;
using NINA.Profile.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using NINA.PlateSolving;
using NINA.ViewModel.Interfaces;
using NINA.Core.Enum;
using NINA.Equipment.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Astrometry;
using NINA.WPF.Base.Behaviors;
using NINA.Core.Utility.Notification;
using NINA.Core.Locale;
using NINA.Image.ImageAnalysis;
using NINA.Core.Utility.WindowService;
using NINA.Image.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Equipment.Equipment;
using NINA.WPF.Base.ViewModel;

namespace NINA.ViewModel {

    internal class ImageControlVM : DockableVM, ICameraConsumer, IImageControlVM {

        public ImageControlVM(
            IProfileService profileService, 
            ICameraMediator cameraMediator, 
            ITelescopeMediator telescopeMediator, 
            IApplicationStatusMediator applicationStatusMediator) : base(profileService) {
            Title = Loc.Instance["LblImage"];
            ImageGeometry = (System.Windows.Media.GeometryGroup)System.Windows.Application.Current.Resources["PictureSVG"];

            this.cameraMediator = cameraMediator;
            this.cameraMediator.RegisterConsumer(this);

            this.telescopeMediator = telescopeMediator;
            this.applicationStatusMediator = applicationStatusMediator;
            AutoStretch = profileService.ActiveProfile.ImageSettings.AutoStretch;
            DetectStars = profileService.ActiveProfile.ImageSettings.DetectStars;
            ShowCrossHair = false;
            ShowBahtinovAnalyzer = false;

            _progress = new Progress<ApplicationStatus>(p => Status = p);

            PrepareImageCommand = new AsyncCommand<bool>(() => ProcessImageHelper());
            PlateSolveImageCommand = new AsyncCommand<bool>(() => PlateSolveImage(), (object o) => Image != null);
            CancelPlateSolveImageCommand = new RelayCommand(CancelPlateSolveImage);
            DragStartCommand = new RelayCommand(BahtinovDragStart);
            DragStopCommand = new RelayCommand(BahtinovDragStop);
            DragMoveCommand = new RelayCommand(BahtinovDragMove);
            InspectAberrationCommand = new AsyncCommand<bool>(() => InspectAberration(), (object o) => Image != null);

            PixelPeepStartCommand = new RelayCommand(PixelPeeperStart);
            PixelPeepMoveCommand = new RelayCommand(PixelPeeperMove);
            PixelPeepEndCommand = new RelayCommand(PixelPeeperStop);

            BahtinovRectangle = new ObservableRectangle(0, 0, 200, 200);
            BahtinovRectangle.PropertyChanged += Rectangle_PropertyChanged;
        }

        private bool showPixelPeeper;

        public bool ShowPixelPeeper {
            get {
                return showPixelPeeper;
            }
            set {
                showPixelPeeper = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource pixelPeepImage;

        public BitmapSource PixelPeepImage {
            get => pixelPeepImage;
            private set {
                pixelPeepImage = value;
                RaisePropertyChanged();
            }
        }

        private double pixelPeepX;

        public double PixelPeepX {
            get => pixelPeepX;
            set {
                pixelPeepX = value;
                RaisePropertyChanged();
            }
        }

        private double pixelPeepY;

        public double PixelPeepY {
            get => pixelPeepY;
            set {
                pixelPeepY = value;
                RaisePropertyChanged();
            }
        }

        private double pixelPeepValue;

        public double PixelPeepValue {
            get => pixelPeepValue;
            set {
                pixelPeepValue = value;
                RaisePropertyChanged();
            }
        }

        private void PixelPeeperStart(object o) {
            ShowPixelPeeper = true;
            PixelPeeperMove(o);
        }

        private void PixelPeeperMove(object o) {
            if (o != null) {
                var p = (Point)o;
                var x = (int)p.X;
                var y = (int)p.Y;

                var width = RenderedImage.Image.PixelWidth;
                var height = RenderedImage.Image.PixelHeight;

                var idx = x + y * width;
                if (idx < 0) {
                    idx = 0;
                } else if (idx > RenderedImage.RawImageData.Data.FlatArray.Length - 1) {
                    idx = RenderedImage.RawImageData.Data.FlatArray.Length - 1;
                }

                PixelPeepX = x;
                PixelPeepY = y;
                PixelPeepValue = RenderedImage.RawImageData.Data.FlatArray[idx];

                var rectX = Math.Max(x - 12, 0);
                var rectY = Math.Max(y - 12, 0);
                var rectWidth = 25;
                var rectHeight = 25;

                if ((rectX + 25) > width) {
                    rectX = width - rectWidth;
                }
                if ((rectY + 25) > height) {
                    rectY = height - rectHeight;
                }

                var rect = new Int32Rect(Math.Max(0, rectX), Math.Max(0, rectY), Math.Min(this.Image.PixelWidth, rectWidth), Math.Min(this.Image.PixelHeight, rectHeight));
                var crop = new CroppedBitmap(this.Image, rect);
                PixelPeepImage = new WriteableBitmap(crop);
            }
        }

        private void PixelPeeperStop(object o) {
            ShowPixelPeeper = false;

            PixelPeepX = 0;
            PixelPeepY = 0;
            PixelPeepValue = 0;
            PixelPeepImage = null;
        }

        private async Task<bool> InspectAberration() {
            try {
                var vm = new AberrationInspectorVM(profileService);
                await vm.Initialize(Image);
                var service = WindowServiceFactory.Create();
                service.Show(vm, Loc.Instance["LblAberrationInspector"], ResizeMode.CanResize, WindowStyle.ToolWindow);
                return true;
            } catch (Exception ex) {
                Logger.Error(ex);
                Notification.ShowError(ex.Message);
                return false;
            }
        }

        private void Rectangle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (BahtinovRectangle.Width > (Image?.Width * 0.8)) {
                BahtinovRectangle.Width = Image.Width * 0.8;
            }
            if (BahtinovRectangle.Height > (Image?.Height * 0.8)) {
                BahtinovRectangle.Height = Image.Height * 0.8;
            }
            BahtinovDragMove(new DragResult() { Delta = new Vector(0, 0), Mode = DragMode.Move });
        }

        private bool _showBahtinovAnalyzer;

        public bool ShowBahtinovAnalyzer {
            get {
                return _showBahtinovAnalyzer;
            }
            set {
                _showBahtinovAnalyzer = value;
                if (value) {
                    ShowCrossHair = false;
                    BahtinovDragMove(new DragResult() { Delta = new Vector(0, 0), Mode = DragMode.Move });
                }
                RaisePropertyChanged();
            }
        }

        private ObservableRectangle _rectangle;

        public ObservableRectangle BahtinovRectangle {
            get {
                return _rectangle;
            }
            set {
                _rectangle = value;
                RaisePropertyChanged();
            }
        }

        public double DragResizeBoundary { get; } = 10;

        private void BahtinovDragStart(object obj) {
        }

        private void BahtinovDragStop(object obj) {
        }

        private void BahtinovDragMove(object obj) {
            BahtinovRectangle.PropertyChanged -= Rectangle_PropertyChanged;
            if (ShowBahtinovAnalyzer && Image != null) {
                var dragResult = (DragResult)obj;

                if (dragResult.Mode == DragMode.Move) {
                    MoveRectangleInBounds(BahtinovRectangle, dragResult.Delta);
                } else {
                    ResizeRectangleBounds(BahtinovRectangle, dragResult.Delta, dragResult.Mode);
                }

                if (!IsLiveViewEnabled) {
                    AnalyzeBahtinov();
                }

                BahtinovRectangle.PropertyChanged += Rectangle_PropertyChanged;
            }
        }

        private void AnalyzeBahtinov() {
            /* Get Pixels */
            var crop = new CroppedBitmap(Image, new Int32Rect((int)BahtinovRectangle.X, (int)BahtinovRectangle.Y, (int)BahtinovRectangle.Width, (int)BahtinovRectangle.Height));
            BahtinovImage = new BahtinovAnalysis(crop, profileService.ActiveProfile.ColorSchemaSettings.ColorSchema.BackgroundColor).GrabBahtinov();
        }

        private void ResizeRectangleBounds(ObservableRectangle rect, Vector vector, DragMode mode) {
            if (mode == DragMode.Resize_Top_Left) {
                rect.X += vector.X;
                rect.Y += vector.Y;
                rect.Width -= vector.X;
                rect.Height -= vector.Y;
            } else if (mode == DragMode.Resize_Top_Right) {
                rect.Y += vector.Y;
                rect.Width += vector.X;
                rect.Height -= vector.Y;
            } else if (mode == DragMode.Resize_Bottom_Left) {
                rect.X += vector.X;
                rect.Width -= vector.X;
                rect.Height += vector.Y;
            } else if (mode == DragMode.Resize_Left) {
                rect.X += vector.X;
                rect.Width -= vector.X;
            } else if (mode == DragMode.Resize_Right) {
                rect.Width += vector.X;
            } else if (mode == DragMode.Resize_Top) {
                rect.Y += vector.Y;
                rect.Height -= vector.Y;
            } else if (mode == DragMode.Resize_Bottom) {
                rect.Height += vector.Y;
            } else {
                rect.Width += vector.X;
                rect.Height += vector.Y;
            }

            CheckRectangleBounds(rect);
        }

        private void CheckRectangleBounds(ObservableRectangle rect) {
            rect.Width = Math.Round(rect.Width, 0);
            rect.Height = Math.Round(rect.Height, 0);
            /* Check boundaries */
            if (rect.X + rect.Width > Image.Width) {
                rect.X = Image.Width - rect.Width;
            }
            if (rect.Y + rect.Height > Image.Height) {
                rect.Y = Image.Height - rect.Height;
            }
            if (rect.X < 0) {
                rect.X = 0;
            }
            if (rect.Y < 0) {
                rect.Y = 0;
            }
            if (rect.Width > Image.Width) {
                rect.Width = Image.Width;
            }
            if (rect.Height > Image.Height) {
                rect.Height = Image.Height;
            }
            if (rect.Width < 20) {
                rect.Width = 20;
            }
            if (rect.Height < 20) {
                rect.Height = 20;
            }
        }

        private void MoveRectangleInBounds(ObservableRectangle rect, Vector vector) {
            rect.X += vector.X;
            rect.Y += vector.Y;
            CheckRectangleBounds(rect);
        }

        private BahtinovImage _bahtinovImage;

        public BahtinovImage BahtinovImage {
            get {
                return _bahtinovImage;
            }
            private set {
                _bahtinovImage = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand InspectAberrationCommand { get; private set; }
        public ICommand DragStartCommand { get; private set; }
        public ICommand DragStopCommand { get; private set; }
        public ICommand DragMoveCommand { get; private set; }
        public ICommand PixelPeepStartCommand { get; private set; }
        public ICommand PixelPeepEndCommand { get; private set; }
        public ICommand PixelPeepMoveCommand { get; private set; }

        private IWindowServiceFactory windowServiceFactory;

        public IWindowServiceFactory WindowServiceFactory {
            get {
                if (windowServiceFactory == null) {
                    windowServiceFactory = new WindowServiceFactory();
                }
                return windowServiceFactory;
            }
            set {
                windowServiceFactory = value;
            }
        }

        private async Task<bool> PlateSolveImage() {
            if (this.RenderedImage != null) {
                try {
                    _plateSolveToken?.Dispose();
                    _plateSolveToken = new CancellationTokenSource();

                    var plateSolver = PlateSolverFactory.GetPlateSolver(profileService.ActiveProfile.PlateSolveSettings);
                    var blindSolver = PlateSolverFactory.GetBlindSolver(profileService.ActiveProfile.PlateSolveSettings);
                    var parameter = new PlateSolveParameter() {
                        Binning = cameraInfo?.BinX ?? 1,
                        Coordinates = telescopeMediator.GetCurrentPosition(),
                        DownSampleFactor = profileService.ActiveProfile.PlateSolveSettings.DownSampleFactor,
                        FocalLength = profileService.ActiveProfile.TelescopeSettings.FocalLength,
                        MaxObjects = profileService.ActiveProfile.PlateSolveSettings.MaxObjects,
                        PixelSize = profileService.ActiveProfile.CameraSettings.PixelSize,
                        Regions = profileService.ActiveProfile.PlateSolveSettings.Regions,
                        SearchRadius = profileService.ActiveProfile.PlateSolveSettings.SearchRadius,
                    };

                    var imageSolver = new ImageSolver(plateSolver, blindSolver);

                    var service = WindowServiceFactory.Create();
                    var plateSolveStatusVM = new PlateSolvingStatusVM();
                    service.Show(plateSolveStatusVM, this.Title + " - " + plateSolveStatusVM.Title, ResizeMode.CanResize, WindowStyle.ToolWindow);

                    var result = await imageSolver.Solve(this.RenderedImage.RawImageData, parameter, _progress, _plateSolveToken.Token);

                    result.Separation = result.DetermineSeparation(telescopeMediator.GetCurrentPosition());
                    plateSolveStatusVM.PlateSolveResult = result;
                } catch (OperationCanceledException) {
                } catch (Exception ex) {
                    Logger.Error(ex);
                    Notification.ShowError(ex.Message);
                    _progress.Report(new ApplicationStatus() { Status = string.Empty });
                }
                return true;
            } else {
                return false;
            }
        }

        private IProgress<ApplicationStatus> _progress;

        private void CancelPlateSolveImage(object o) {
            _plateSolveToken?.Cancel();
        }

        private CancellationTokenSource _plateSolveToken;

        private IRenderedImage _renderedImage;

        public IRenderedImage RenderedImage {
            get {
                return _renderedImage;
            }
            set {
                _renderedImage = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _image;

        public BitmapSource Image {
            get {
                return _image;
            }
            set {
                _image = value;
                if (_image != null) {
                    if (ShowBahtinovAnalyzer) {
                        ResizeRectangleToImageSize(_image, BahtinovRectangle);
                    }
                }
                RaisePropertyChanged();
            }
        }

        private void ResizeRectangleToImageSize(BitmapSource image, ObservableRectangle rectangle) {
            if (rectangle.X < 0 || rectangle.Y < 0
                || rectangle.X + rectangle.Width > image.PixelWidth
                || rectangle.Y + rectangle.Height > image.PixelHeight) {
                rectangle.X = image.PixelWidth / 2 - rectangle.Width / 2;
                rectangle.Y = image.PixelHeight / 2 - rectangle.Height / 2;
            }
        }

        private bool _autoStretch;

        public bool AutoStretch {
            get {
                return _autoStretch;
            }
            set {
                _autoStretch = value;
                profileService.ActiveProfile.ImageSettings.AutoStretch = value;
                if (!_autoStretch && _detectStars) { _detectStars = false; RaisePropertyChanged(nameof(DetectStars)); }
                RaisePropertyChanged();
            }
        }

        private async Task<bool> ProcessImageHelper() {
            _prepImageCancellationSource?.Cancel();
            try {
                _prepImageTask?.Wait(_prepImageCancellationSource.Token);
            } catch (OperationCanceledException) {
            }
            _prepImageCancellationSource?.Dispose();
            _prepImageCancellationSource = new CancellationTokenSource();
            if (RenderedImage != null) {
                _prepImageTask = ProcessAndUpdateImage(RenderedImage.ReRender(), new PrepareImageParameters(), _prepImageCancellationSource.Token);
                await _prepImageTask;
            }
            return true;
        }

        public AsyncCommand<bool> PrepareImageCommand { get; private set; }

        private Task _prepImageTask;
        private CancellationTokenSource _prepImageCancellationSource;

        private bool _showCrossHair;

        public bool ShowCrossHair {
            get {
                return _showCrossHair;
            }
            set {
                _showCrossHair = value;
                if (value) {
                    ShowBahtinovAnalyzer = false;
                }
                RaisePropertyChanged();
            }
        }

        private bool _detectStars;

        public bool DetectStars {
            get {
                return _detectStars;
            }
            set {
                _detectStars = value;
                profileService.ActiveProfile.ImageSettings.DetectStars = value;
                if (_detectStars) { _autoStretch = true; RaisePropertyChanged(nameof(AutoStretch)); }
                RaisePropertyChanged();
            }
        }

        private ApplicationStatus _status;

        public ApplicationStatus Status {
            get {
                return _status;
            }
            set {
                _status = value;
                _status.Source = Title;
                RaisePropertyChanged();

                applicationStatusMediator.StatusUpdate(_status);
            }
        }

        public IAsyncCommand PlateSolveImageCommand { get; private set; }

        public ICommand CancelPlateSolveImageCommand { get; private set; }

        public bool IsLiveViewEnabled { get; internal set; }

        public static SemaphoreSlim ss = new SemaphoreSlim(1, 1);
        private ICameraMediator cameraMediator;
        private CameraInfo cameraInfo = DeviceInfo.CreateDefaultInstance<CameraInfo>();
        private ITelescopeMediator telescopeMediator;
        private IApplicationStatusMediator applicationStatusMediator;

        public async Task<IRenderedImage> PrepareImage(
            IImageData data,
            PrepareImageParameters parameters,
            CancellationToken cancelToken) {
            await ss.WaitAsync(cancelToken);

            try {
                if (data == null) {
                    return null;
                }

                _progress.Report(new ApplicationStatus() { Status = Loc.Instance["LblPrepareImage"] });

                var renderedImage = data.RenderImage();
                if (data.Properties.IsBayered && profileService.ActiveProfile.ImageSettings.DebayerImage) {
                    _progress.Report(new ApplicationStatus() { Status = Loc.Instance["LblDebayeringImage"] });
                    var unlinkedStretch = profileService.ActiveProfile.ImageSettings.UnlinkedStretch;
                    var detectStars = parameters.DetectStars.HasValue ? parameters.DetectStars.Value : DetectStars;
                    var starDetection = profileService.ActiveProfile.ImageSettings.DebayeredHFR && detectStars;

                    var bayerPattern = cameraInfo.SensorType;
                    if (profileService.ActiveProfile.CameraSettings.BayerPattern != BayerPatternEnum.Auto) {
                        bayerPattern = (SensorType)profileService.ActiveProfile.CameraSettings.BayerPattern;
                    }

                    renderedImage = renderedImage.Debayer(saveColorChannels: unlinkedStretch, saveLumChannel: starDetection, bayerPattern: bayerPattern);
                }

                return await ProcessAndUpdateImage(renderedImage, parameters, cancelToken);
            } finally {
                _progress.Report(new ApplicationStatus() { Status = string.Empty });
                ss.Release();
            }
        }

        private async Task<IRenderedImage> ProcessAndUpdateImage(
            IRenderedImage renderedImage,
            PrepareImageParameters parameters,
            CancellationToken cancelToken) {
            var processedImage = await ProcessImage(renderedImage, parameters, cancelToken);

            this.RenderedImage = processedImage;
            this.Image = processedImage.Image;
            GC.Collect();

            if (ShowBahtinovAnalyzer) {
                AnalyzeBahtinov();
            }
            return processedImage;
        }

        private async Task<IRenderedImage> ProcessImage(
            IRenderedImage renderedImage,
            PrepareImageParameters parameters,
            CancellationToken cancelToken) {
            var detectStars = parameters.DetectStars.HasValue ? parameters.DetectStars.Value : DetectStars;
            var autoStretch = detectStars || (parameters.AutoStretch.HasValue ? parameters.AutoStretch.Value : AutoStretch);
            var processedImage = renderedImage;
            if (autoStretch) {
                _progress.Report(new ApplicationStatus() { Status = Loc.Instance["LblStretchImage"] });
                var unlinkedStretch = renderedImage.RawImageData.Properties.IsBayered && profileService.ActiveProfile.ImageSettings.DebayerImage && profileService.ActiveProfile.ImageSettings.UnlinkedStretch;
                processedImage = await processedImage.Stretch(
                    factor: profileService.ActiveProfile.ImageSettings.AutoStretchFactor,
                    blackClipping: profileService.ActiveProfile.ImageSettings.BlackClipping,
                    unlinked: unlinkedStretch);
            }

            if (detectStars) {
                processedImage = await processedImage.DetectStars(
                    annotateImage: profileService.ActiveProfile.ImageSettings.AnnotateImage,
                    sensitivity: profileService.ActiveProfile.ImageSettings.StarSensitivity,
                    noiseReduction: profileService.ActiveProfile.ImageSettings.NoiseReduction,
                    cancelToken: cancelToken,
                    progress: _progress);
            }
            _progress.Report(new ApplicationStatus() { Status = "" });
            return processedImage;
        }

        public CameraInfo CameraInfo {
            get => cameraInfo;
            set {
                cameraInfo = value;
                RaisePropertyChanged();
            }
        }

        public void UpdateDeviceInfo(CameraInfo cameraInfo) {
            CameraInfo = cameraInfo;
        }

        public void Dispose() {
            this.cameraMediator.RemoveConsumer(this);
        }
    }
}