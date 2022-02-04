#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Astrometry.Interfaces;
using NINA.Core.Enum;
using NINA.Core.Locale;
using NINA.Core.Model;
using NINA.Core.Model.Equipment;
using NINA.Core.MyMessageBox;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Core.Utility.WindowService;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Exceptions;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Image.Interfaces;
using NINA.PlateSolving;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Container;
using NINA.Sequencer.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem.Platesolving;
using NINA.WPF.Base.Behaviors;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.SkySurvey;
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace NINA.ViewModel.FramingAssistant {

    internal class FramingAssistantVM : BaseVM, ICameraConsumer, IFramingAssistantVM {

        public FramingAssistantVM(IProfileService profileService,
                                  ICameraMediator cameraMediator,
                                  ITelescopeMediator telescopeMediator,
                                  IApplicationStatusMediator applicationStatusMediator,
                                  INighttimeCalculator nighttimeCalculator,
                                  IPlanetariumFactory planetariumFactory,
                                  ISequenceMediator sequenceMediator,
                                  IApplicationMediator applicationMediator,
                                  IDeepSkyObjectSearchVM deepSkyObjectSearchVM,
                                  IImagingMediator imagingMediator,
                                  IFilterWheelMediator filterWheelMediator,
                                  IGuiderMediator guiderMediator,
                                  IRotatorMediator rotatorMediator,
                                  IDomeMediator domeMediator,
                                  IDomeFollower domeFollower,
                                  IImageDataFactory imageDataFactory) : base(profileService) {
            this.cameraMediator = cameraMediator;
            this.cameraMediator.RegisterConsumer(this);
            this.telescopeMediator = telescopeMediator;
            this.applicationStatusMediator = applicationStatusMediator;
            this.nighttimeCalculator = nighttimeCalculator;
            this.planetariumFactory = planetariumFactory;
            this.sequenceMediator = sequenceMediator;
            this.applicationMediator = applicationMediator;
            this.imagingMediator = imagingMediator;
            this.filterWheelMediator = filterWheelMediator;
            this.guiderMediator = guiderMediator;
            this.rotatorMediator = rotatorMediator;
            this.domeMediator = domeMediator;
            this.domeFollower = domeFollower;
            this.imageDataFactory = imageDataFactory;

            SkyMapAnnotator = new SkyMapAnnotator(telescopeMediator);

            var defaultCoordinates = new Coordinates(0, 0, Epoch.J2000, Coordinates.RAType.Degrees);
            DSO = new DeepSkyObject(string.Empty, defaultCoordinates, profileService.ActiveProfile.ApplicationSettings.SkyAtlasImageRepository, profileService.ActiveProfile.AstrometrySettings.Horizon);

            FramingAssistantSource = profileService.ActiveProfile.FramingAssistantSettings.LastSelectedImageSource;

            CameraPixelSize = profileService.ActiveProfile.CameraSettings.PixelSize;
            FocalLength = profileService.ActiveProfile.TelescopeSettings.FocalLength;

            Rectangle = new FramingRectangle(0, 0, 0, 0, 0) {
                Rotation = 0
            };

            _statusUpdate = new Progress<ApplicationStatus>(p => Status = p);
            _progress = new Progress<int>((p) => DownloadProgressValue = p);

            DeepSkyObjectSearchVM = deepSkyObjectSearchVM;
            DeepSkyObjectSearchVM.PropertyChanged += DeepSkyObjectSearchVM_PropertyChanged;

            var appSettings = profileService.ActiveProfile.ApplicationSettings;
            appSettings.PropertyChanged += ApplicationSettings_PropertyChanged;

            profileService.ProfileChanged += (object sender, EventArgs e) => {
                appSettings.PropertyChanged -= ApplicationSettings_PropertyChanged;

                this.FocalLength = profileService.ActiveProfile.TelescopeSettings.FocalLength;
                this.CameraPixelSize = profileService.ActiveProfile.CameraSettings.PixelSize;

                RaisePropertyChanged(nameof(CameraPixelSize));
                RaisePropertyChanged(nameof(FocalLength));
                RaisePropertyChanged(nameof(FieldOfView));
                RaisePropertyChanged(nameof(CameraWidth));
                RaisePropertyChanged(nameof(CameraHeight));
                appSettings = profileService.ActiveProfile.ApplicationSettings;
                appSettings.PropertyChanged += ApplicationSettings_PropertyChanged;
                ApplicationSettings_PropertyChanged(null, null);
            };

            resizeTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, _dispatcher);
            resizeTimer.Interval = TimeSpan.FromMilliseconds(500);
            resizeTimer.Tick += ResizeTimer_Tick;

            profileService.LocationChanged += (object sender, EventArgs e) => {
                DSO = new DeepSkyObject(DSO.Name, DSO.Coordinates, profileService.ActiveProfile.ApplicationSettings.SkyAtlasImageRepository, profileService.ActiveProfile.AstrometrySettings.Horizon);
            };

            profileService.HorizonChanged += (object sender, EventArgs e) => {
                DSO?.SetCustomHorizon(profileService.ActiveProfile.AstrometrySettings.Horizon);
            };

            InitializeCommands();
            Task.Run(() => {
                this.NighttimeData = this.nighttimeCalculator.Calculate();
                InitializeCache();
            });
        }

        private bool sequencerActionsOpened;
        public bool SequencerActionsOpened {
            get => sequencerActionsOpened;
            set {
                sequencerActionsOpened = value;
                if(sequencerActionsOpened) {
                    GetDSOTemplatesCommand.Execute(null);
                    GetExistingSequencerTargetsCommand.Execute(null);
                }
                RaisePropertyChanged();
            }
        }

        private void InitializeCommands() {
            LoadImageCommand = new AsyncCommand<bool>(async () => { return await LoadImage(); });
            CancelLoadImageFromFileCommand = new RelayCommand((object o) => { CancelLoadImage(); });
            CancelLoadImageCommand = new RelayCommand((object o) => { CancelLoadImage(); });
            DragStartCommand = new RelayCommand(DragStart);
            DragStopCommand = new RelayCommand(DragStop);
            DragMoveCommand = new RelayCommand(DragMove);
            ClearCacheCommand = new RelayCommand(ClearCache, (object o) => Cache != null);
            RefreshSkyMapAnnotationCommand = new RelayCommand((object o) => SkyMapAnnotator.UpdateSkyMap(), (object o) => SkyMapAnnotator.Initialized);
            MouseWheelCommand = new RelayCommand(MouseWheel);
            GetRotationFromCameraCommand = new AsyncCommand<bool>(GetRotationFromCamera, (object o) => RectangleCalculated && cameraMediator.GetInfo().Connected && cameraMediator.IsFreeToCapture(this));
            CancelGetRotationFromCameraCommand = new RelayCommand(o => { try { getRotationTokenSource?.Cancel(); } catch (Exception) { } });

            CoordsFromPlanetariumCommand = new AsyncCommand<bool>(() => Task.Run(CoordsFromPlanetarium));
            CoordsFromScopeCommand = new AsyncCommand<bool>(() => Task.Run(CoordsFromScope));

            GetDSOTemplatesCommand = new RelayCommand((object o) => {
                DSOTemplates = sequenceMediator.GetDeepSkyObjectContainerTemplates();
                RaisePropertyChanged(nameof(DSOTemplates));
            }, (object o) => sequenceMediator.Initialized && RectangleCalculated);

            GetExistingSequencerTargetsCommand = new RelayCommand((object o) => {
                var simpleTargets = sequenceMediator.GetAllTargetsInSimpleSequence();
                var advancedTargets = sequenceMediator.GetAllTargetsInAdvancedSequence();

                List<IDeepSkyObjectContainer> targets = new List<IDeepSkyObjectContainer>(simpleTargets);
                targets.AddRange(advancedTargets);
                ExistingTargets = targets;
                RaisePropertyChanged(nameof(ExistingTargets));

            }, (object o) => sequenceMediator.Initialized && RectangleCalculated && !IsMosaic);

            UpdateExistingTargetInSequencerCommand = new RelayCommand((object o) => {
                if(o is IDeepSkyObjectContainer container) {

                    var rect = CameraRectangles.First();
                    var name = GetRectangleName(rect);

                    double rotation = rect.DSORotation;

                    container.Name = name;
                    container.Target.TargetName = name;
                    container.Target.InputCoordinates = new InputCoordinates() {
                        Coordinates = rect.Coordinates
                    };
                }
                ExistingTargets.Clear();
                applicationMediator.ChangeTab(ApplicationTab.SEQUENCE);
            }, (object o) => sequenceMediator.Initialized && RectangleCalculated && CameraRectangles?.Count > 0 && ExistingTargets?.Count > 0); ;

            SetOldSequencerTargetCommand = new RelayCommand((object o) => {
                applicationMediator.ChangeTab(ApplicationTab.SEQUENCE);

                var deepSkyObjects = new List<DeepSkyObject>();
                foreach (var rect in CameraRectangles) {
                    var name = GetRectangleName(rect);
                    var dso = new DeepSkyObject(name ?? rect.Name, rect.Coordinates, profileService.ActiveProfile.ApplicationSettings.SkyAtlasImageRepository, profileService.ActiveProfile.AstrometrySettings.Horizon);

                    double rotation = rect.DSORotation;

                    dso.Rotation = AstroUtil.EuclidianModulus(rotation, 360);

                    dso.SetDateAndPosition(NighttimeCalculator.GetReferenceDate(DateTime.Now), profileService.ActiveProfile.AstrometrySettings.Latitude, profileService.ActiveProfile.AstrometrySettings.Longitude);

                    Logger.Info($"Adding target to simple sequencer: {dso.Name} - {dso.Coordinates}");
                    sequenceMediator.AddSimpleTarget(dso);
                }
            }, (object o) => sequenceMediator.Initialized && RectangleCalculated);
            SetSequencerTargetCommand = new RelayCommand((object o) => {
                applicationMediator.ChangeTab(ApplicationTab.SEQUENCE);

                var template = o as IDeepSkyObjectContainer;
                foreach (var container in GetDSOContainerListFromFraming(template)) {
                    Logger.Info($"Adding target to advanced sequencer: {container.Target.DeepSkyObject.Name} - {container.Target.DeepSkyObject.Coordinates}");
                    sequenceMediator.AddAdvancedTarget(container);
                }
            }, (object o) => sequenceMediator.Initialized && RectangleCalculated);

            AddTargetToTargetListCommand = new RelayCommand((object o) => {
                var template = o as IDeepSkyObjectContainer;
                foreach (var container in GetDSOContainerListFromFraming(template)) {
                    Logger.Info($"Adding target to target list: {container.Target.DeepSkyObject.Name} - {container.Target.DeepSkyObject.Coordinates}");
                    sequenceMediator.AddTargetToTargetList(container);
                }
            }, (object o) => sequenceMediator.Initialized && RectangleCalculated);

            SlewToCoordinatesCommand = new AsyncCommand<bool>(async (object o) => {
                slewTokenSource?.Cancel();
                slewTokenSource?.Dispose();
                slewTokenSource = new CancellationTokenSource();
                bool result;
                try {
                    cameraMediator.RegisterCaptureBlock(this);
                    switch (o.ToString()) {
                        case "Center":
                            Logger.Info($"Centering from framing assistant to {Rectangle.Coordinates}");
                            result = await Center(Rectangle.Coordinates, slewTokenSource.Token);
                            break;

                        case "Rotate":
                            Logger.Info($"Centering and rotating from framing assistant to {Rectangle.Coordinates} and angle {Rectangle.TotalRotation}");
                            result = await CenterAndRotate(Rectangle.Coordinates, Rectangle.TotalRotation, slewTokenSource.Token);
                            break;

                        default:
                            Logger.Info($"Slewing from framing assistant to {Rectangle.Coordinates}");
                            result = await SlewToCoordinates(Rectangle.Coordinates, slewTokenSource.Token);
                            break;
                    }

                    if (!result) {
                        Logger.Error($"Failed to {o} from the framing wizard");
                    }
                } catch (Exception e) {
                    Logger.Error($"Failed to {o} from the framing wizard", e);
                    result = false;
                } finally {
                    cameraMediator.ReleaseCaptureBlock(this);
                }

                if (!result) {
                    Notification.ShowError(String.Format(Loc.Instance["LblFramingWizardSlewFailed"], o.ToString()));
                }
                return result;
            }, (object o) => RectangleCalculated && cameraMediator.IsFreeToCapture(this));
            CancelSlewToCoordinatesCommand = new RelayCommand((object o) => slewTokenSource?.Cancel());

            ScrollViewerSizeChangedCommand = new RelayCommand((parameter) => {
                resizeTimer.Stop();
                if (ImageParameter != null && FramingAssistantSource == SkySurveySource.SKYATLAS) {
                    resizeTimer.Start();
                }
            });
        }

        private async Task<bool> GetRotationFromCamera(object arg) {
            try {
                using (getRotationTokenSource = new CancellationTokenSource()) {
                    Logger.Info("Determining camera rotation for framing");
                    var camerainfo = cameraMediator.GetInfo();

                    var seq = new Equipment.Model.CaptureSequence() {
                        Binning = new BinningMode(profileService.ActiveProfile.PlateSolveSettings.Binning, profileService.ActiveProfile.PlateSolveSettings.Binning),
                        Gain = profileService.ActiveProfile.PlateSolveSettings.Gain,
                        FilterType = profileService.ActiveProfile.PlateSolveSettings.Filter,
                        ExposureTime = profileService.ActiveProfile.PlateSolveSettings.ExposureTime,
                        TotalExposureCount = 1
                    };

                    var plateSolver = PlateSolverFactory.GetPlateSolver(profileService.ActiveProfile.PlateSolveSettings);
                    var blindSolver = PlateSolverFactory.GetBlindSolver(profileService.ActiveProfile.PlateSolveSettings);

                    var parameter = new CaptureSolverParameter() {
                        Attempts = 1,
                        Binning = profileService.ActiveProfile.PlateSolveSettings.Binning,
                        DownSampleFactor = profileService.ActiveProfile.PlateSolveSettings.DownSampleFactor,
                        FocalLength = profileService.ActiveProfile.TelescopeSettings.FocalLength,
                        MaxObjects = profileService.ActiveProfile.PlateSolveSettings.MaxObjects,
                        PixelSize = profileService.ActiveProfile.CameraSettings.PixelSize,
                        ReattemptDelay = TimeSpan.FromMinutes(profileService.ActiveProfile.PlateSolveSettings.ReattemptDelay),
                        Regions = profileService.ActiveProfile.PlateSolveSettings.Regions,
                        SearchRadius = profileService.ActiveProfile.PlateSolveSettings.SearchRadius,
                        Coordinates = telescopeMediator.GetCurrentPosition(),
                        BlindFailoverEnabled = profileService.ActiveProfile.PlateSolveSettings.BlindFailoverEnabled
                    };

                    var captureSolver = new CaptureSolver(plateSolver, blindSolver, imagingMediator, filterWheelMediator);
                    var result = await captureSolver.Solve(seq, parameter, default, _statusUpdate, getRotationTokenSource.Token);

                    if (result.Success) {
                        RectangleTotalRotation = result.Orientation;
                        Logger.Info($"Camera rotation has been determined: {result.Orientation}°");
                        Notification.ShowInformation(string.Format(Loc.Instance["LblCameraRotationSolved"], Math.Round(result.Orientation, 2)));
                    } else {
                        Logger.Info("Camera rotation import failed. Plate sovling was unsuccessful");
                        Notification.ShowError(Loc.Instance["LblCameraRotationImportFailed"]);
                    }
                }
            } catch (OperationCanceledException) {
            } catch (Exception ex) {
                Logger.Error("Camera rotation import failed", ex);
                Notification.ShowError(Loc.Instance["LblCameraRotationImportFailed"]);
            }
            return true;
        }

        private async Task<bool> Center(Coordinates coordinates, CancellationToken token) {
            var center = new Center(profileService, telescopeMediator, imagingMediator, filterWheelMediator, guiderMediator, domeMediator, domeFollower, new PlateSolverFactoryProxy(), new WindowServiceFactory());

            center.Coordinates = new InputCoordinates(coordinates);
            var isValid = center.Validate();

            if (!isValid) {
                Notification.ShowError(string.Join(Environment.NewLine, center.Issues));
                return false;
            }

            await center.Run(_statusUpdate, token);
            return true;
        }

        private async Task<bool> CenterAndRotate(Coordinates coordinates, double rotation, CancellationToken token) {
            var centerAndRotate = new CenterAndRotate(profileService, telescopeMediator, imagingMediator, rotatorMediator, filterWheelMediator, guiderMediator, domeMediator, domeFollower, new PlateSolverFactoryProxy(), new WindowServiceFactory());

            centerAndRotate.Coordinates = new InputCoordinates(coordinates);
            centerAndRotate.Rotation = rotation;
            var isValid = centerAndRotate.Validate();

            if (!isValid) {
                Notification.ShowError(string.Join(Environment.NewLine, centerAndRotate.Issues));
                return false;
            }

            await centerAndRotate.Run(_statusUpdate, token);
            return true;
        }

        private Task<bool> SlewToCoordinates(
            Coordinates coordinates,
            CancellationToken token) {
            return telescopeMediator.SlewToCoordinatesAsync(coordinates, token);
        }

        private string GetRectangleName(FramingRectangle rect) {
            return rect.Id > 0 ? DSO?.Name + string.Format(" {0} ", Loc.Instance["LblPanel"]) + rect.Id : DSO?.Name ?? string.Empty;
        }

        private IList<IDeepSkyObjectContainer> GetDSOContainerListFromFraming(IDeepSkyObjectContainer template) {
            var l = new List<IDeepSkyObjectContainer>();
            var first = true;

            foreach (var rect in CameraRectangles) {
                var container = (IDeepSkyObjectContainer)template.Clone();
                var name = GetRectangleName(rect);
                container.Name = name;

                double rotation = rect.DSORotation;

                container.Target = new InputTarget(Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Latitude), Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Longitude), profileService.ActiveProfile.AstrometrySettings.Horizon) {
                    TargetName = name,
                    Rotation = AstroUtil.EuclidianModulus(rotation, 360),
                    InputCoordinates = new InputCoordinates() {
                        Coordinates = rect.Coordinates
                    }
                };
                l.Add(container);
                container.IsExpanded = first;
                first = false;
            }
            return l;
        }

        public IList<IDeepSkyObjectContainer> DSOTemplates { get; private set; }
        public IList<IDeepSkyObjectContainer> ExistingTargets { get; private set; }

        private void InitializeCache() {
            try {
                Cache = new CacheSkySurvey(profileService.ActiveProfile.ApplicationSettings.SkySurveyCacheDirectory);
                ImageCacheInfo = Cache.Cache;
                _selectedImageCacheInfo = (XElement)ImageCacheInfo?.FirstNode ?? null;
                RaisePropertyChanged(nameof(ImageCacheInfo));
            } catch (Exception ex) {
                Logger.Error(ex);
                Cache = null;
                ImageCacheInfo = null;
            }
            RaisePropertyChanged(nameof(ImageCacheInfo));
        }

        private void MouseWheel(object obj) {
            var delta = ((MouseWheelResult)obj).Delta;
            var stepSize = 2;
            if (delta > 0) {
                if (FieldOfView > 1) {
                    FieldOfView = Math.Max(1, FieldOfView - stepSize);
                }
            } else {
                if (FieldOfView < 200) {
                    FieldOfView = Math.Min(200, FieldOfView + stepSize);
                }
            }
            CalculateRectangle(SkyMapAnnotator.ChangeFoV(FieldOfView));
        }

        private async void ResizeTimer_Tick(object sender, EventArgs e) {
            using (MyStopWatch.Measure()) {
                (sender as DispatcherTimer).Stop();
                await LoadImage();
            }
        }

        private readonly DispatcherTimer resizeTimer;

        private void DeepSkyObjectSearchVM_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(DeepSkyObjectSearchVM.Coordinates) && DeepSkyObjectSearchVM.Coordinates != null) {
                DSO = new DeepSkyObject(DeepSkyObjectSearchVM.TargetName, DeepSkyObjectSearchVM.Coordinates, profileService.ActiveProfile.ApplicationSettings.SkyAtlasImageRepository, profileService.ActiveProfile.AstrometrySettings.Horizon);
                RaiseCoordinatesChanged();
            } else if (e.PropertyName == nameof(DeepSkyObjectSearchVM.TargetName) && DSO != null) {
                DSO.Name = DeepSkyObjectSearchVM.TargetName;
            }
        }

        public IDeepSkyObjectSearchVM DeepSkyObjectSearchVM { get; private set; }

        private void ApplicationSettings_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            InitializeCache();
        }

        public double Opacity {
            get => profileService.ActiveProfile.FramingAssistantSettings.Opacity;
            set {
                profileService.ActiveProfile.FramingAssistantSettings.Opacity = value;
                RaisePropertyChanged();
            }
        }

        private bool preserveAlignment;

        // When enabled the rotation for the framing rectangle rotation will be adjusted for field curvature
        public bool PreserveAlignment {
            get => preserveAlignment;
            set {
                preserveAlignment = value;
                RaisePropertyChanged();
                DragMove(new DragResult() { Delta = new Vector() });
            }
        }

        // Proxy Property to be able to recalculate rectangle on change
        public double RectangleRotation {
            get => Rectangle?.Rotation ?? 0;
            set {
                if (RectangleCalculated) {
                    Rectangle.Rotation = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(RectangleTotalRotation));
                    RaisePropertyChanged(nameof(InverseRectangleRotation));
                    DragMove(new DragResult() { Delta = new Vector() });
                }
            }
        }


        // Flag that indicates if the sky background should be rotated instead of the rectangle
        private bool rotateSky;
        public bool RotateSky {
            get => rotateSky;
            set {
                rotateSky = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(InverseRectangleRotation));
            }
        }

        // Proxy Property for derotating the image according to the rectangle rotation
        public double InverseRectangleRotation {
            get => RotateSky ? (- Rectangle?.Rotation ?? 0) : 0;
        }

        // Proxy Property to be able to recalculate rectangle on change
        public double RectangleTotalRotation {
            get => Rectangle?.TotalRotation ?? 0;
            set {
                if (RectangleCalculated) {
                    Rectangle.TotalRotation = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(RectangleRotation));
                    DragMove(new DragResult() { Delta = new Vector() });
                }
            }
        }

        private int fontSize;

        public int FontSize {
            get => fontSize;
            set {
                fontSize = value;
                RaisePropertyChanged();
            }
        }

        private ISkySurveyFactory skySurveyFactory;

        public ISkySurveyFactory SkySurveyFactory {
            get {
                if (skySurveyFactory == null) {
                    skySurveyFactory = new SkySurveyFactory(imageDataFactory);
                }
                return skySurveyFactory;
            }
            set {
                skySurveyFactory = value;
            }
        }

        private bool _rectangleCalculated;

        public bool RectangleCalculated {
            get {
                return _rectangleCalculated;
            }
            private set {
                _rectangleCalculated = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(RectangleRotation));
                RaisePropertyChanged(nameof(RectangleTotalRotation));
            }
        }

        private void ClearCache(object obj) {
            if (Cache != null) {
                var diagResult = MyMessageBox.Show(Loc.Instance["LblClearCache"] + "?", "", MessageBoxButton.YesNo, MessageBoxResult.No);
                if (diagResult == MessageBoxResult.Yes) {
                    Cache.Clear();
                    ImageCacheInfo = Cache.Cache;
                    RaisePropertyChanged(nameof(ImageCacheInfo));
                }
            }
        }

        public static string FRAMINGASSISTANTCACHEPATH = Path.Combine(NINA.Core.Utility.CoreUtil.APPLICATIONTEMPPATH, "FramingAssistantCache");
        public static string FRAMINGASSISTANTCACHEINFOPATH = Path.Combine(FRAMINGASSISTANTCACHEPATH, "CacheInfo.xml");

        private ApplicationStatus _status;

        public ApplicationStatus Status {
            get {
                return _status;
            }
            set {
                _status = value;
                _status.Source = Loc.Instance["LblFramingAssistant"];
                RaisePropertyChanged();

                applicationStatusMediator.StatusUpdate(_status);
            }
        }

        public async Task<bool> SetCoordinates(DeepSkyObject dso) {
            DeepSkyObjectSearchVM.SetTargetNameWithoutSearch(dso.Name);
            this.DSO = new DeepSkyObject(dso.Name, dso.Coordinates, profileService.ActiveProfile.ApplicationSettings.SkyAtlasImageRepository, profileService.ActiveProfile.AstrometrySettings.Horizon);
            FramingAssistantSource = profileService.ActiveProfile.FramingAssistantSettings.LastSelectedImageSource;
            if (FramingAssistantSource == SkySurveySource.CACHE || FramingAssistantSource == SkySurveySource.FILE) {
                FramingAssistantSource = SkySurveySource.HIPS2FITS;
            }

            RaiseCoordinatesChanged();
            while (boundWidth == 0) {
                await Task.Delay(50);
            }
            await LoadImageCommand.ExecuteAsync(null);
            RectangleRotation = dso.Rotation;
            return true;
        }

        private void CancelLoadImage() {
            _loadImageSource?.Cancel();
        }

        private Dispatcher _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        private int boundWidth;

        public double BoundWidth {
            get => boundWidth;
            set => boundWidth = (int)value;
        }

        private int boundHeight;

        public double BoundHeight {
            get => boundHeight;
            set => boundHeight = (int)value;
        }

        private DeepSkyObject _dSO;

        public DeepSkyObject DSO {
            get {
                return _dSO;
            }
            set {
                _dSO = value;
                _dSO?.SetDateAndPosition(NighttimeCalculator.GetReferenceDate(DateTime.Now), profileService.ActiveProfile.AstrometrySettings.Latitude, profileService.ActiveProfile.AstrometrySettings.Longitude);
                RaisePropertyChanged();
            }
        }

        private ICameraMediator cameraMediator;
        private ITelescopeMediator telescopeMediator;
        private IApplicationStatusMediator applicationStatusMediator;
        private INighttimeCalculator nighttimeCalculator;
        private ISequenceMediator sequenceMediator;
        private IApplicationMediator applicationMediator;
        private IImagingMediator imagingMediator;
        private IFilterWheelMediator filterWheelMediator;
        private IGuiderMediator guiderMediator;
        private IRotatorMediator rotatorMediator;
        private IDomeMediator domeMediator;
        private IDomeFollower domeFollower;
        private NighttimeData nighttimeData;
        private IImageDataFactory imageDataFactory;

        public NighttimeData NighttimeData {
            get => nighttimeData;
            set {
                if (nighttimeData != value) {
                    nighttimeData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private readonly IPlanetariumFactory planetariumFactory;

        public int RAHours {
            get {
                return (int)Math.Truncate(DSO.Coordinates.RA);
            }
            set {
                if (value >= 0) {
                    DSO.Coordinates.RA = DSO.Coordinates.RA - RAHours + value;
                    RaiseCoordinatesChanged();
                }
            }
        }

        public int RAMinutes {
            get {
                return (int)(Math.Floor(DSO.Coordinates.RA * 60.0d) % 60);
            }
            set {
                if (value >= 0) {
                    DSO.Coordinates.RA = DSO.Coordinates.RA - RAMinutes / 60.0d + value / 60.0d;
                    RaiseCoordinatesChanged();
                }
            }
        }

        public int RASeconds {
            get {
                return (int)(Math.Floor(DSO.Coordinates.RA * 60.0d * 60.0d) % 60);
            }
            set {
                if (value >= 0) {
                    DSO.Coordinates.RA = DSO.Coordinates.RA - RASeconds / (60.0d * 60.0d) + value / (60.0d * 60.0d);
                    RaiseCoordinatesChanged();
                }
            }
        }

        private bool negativeDec;

        public bool NegativeDec {
            get => negativeDec;
            set {
                negativeDec = value;
                RaisePropertyChanged();
            }
        }

        public int DecDegrees {
            get {
                return (int)Math.Truncate(DSO.Coordinates.Dec);
            }
            set {
                if (NegativeDec) {
                    DSO.Coordinates.Dec = value - DecMinutes / 60.0d - DecSeconds / (60.0d * 60.0d);
                } else {
                    DSO.Coordinates.Dec = value + DecMinutes / 60.0d + DecSeconds / (60.0d * 60.0d);
                }
                RaiseCoordinatesChanged();
            }
        }

        public int DecMinutes {
            get {
                return (int)Math.Floor((Math.Abs(DSO.Coordinates.Dec * 60.0d) % 60));
            }
            set {
                if (DSO.Coordinates.Dec < 0) {
                    DSO.Coordinates.Dec = DSO.Coordinates.Dec + DecMinutes / 60.0d - value / 60.0d;
                } else {
                    DSO.Coordinates.Dec = DSO.Coordinates.Dec - DecMinutes / 60.0d + value / 60.0d;
                }

                RaiseCoordinatesChanged();
            }
        }

        public int DecSeconds {
            get {
                return (int)Math.Floor((Math.Abs(DSO.Coordinates.Dec * 60.0d * 60.0d) % 60));
            }
            set {
                if (DSO.Coordinates.Dec < 0) {
                    DSO.Coordinates.Dec = DSO.Coordinates.Dec + DecSeconds / (60.0d * 60.0d) - value / (60.0d * 60.0d);
                } else {
                    DSO.Coordinates.Dec = DSO.Coordinates.Dec - DecSeconds / (60.0d * 60.0d) + value / (60.0d * 60.0d);
                }

                RaiseCoordinatesChanged();
            }
        }

        private void RaiseCoordinatesChanged() {
            RaisePropertyChanged(nameof(RAHours));
            RaisePropertyChanged(nameof(RAMinutes));
            RaisePropertyChanged(nameof(RASeconds));
            RaisePropertyChanged(nameof(DecDegrees));
            RaisePropertyChanged(nameof(DecMinutes));
            RaisePropertyChanged(nameof(DecSeconds));
            NegativeDec = DSO?.Coordinates?.Dec < 0;
            NighttimeData = nighttimeCalculator.Calculate();
        }

        private int _downloadProgressValue;

        public int DownloadProgressValue {
            get {
                return _downloadProgressValue;
            }
            set {
                _downloadProgressValue = value;
                RaisePropertyChanged();
            }
        }

        public double FieldOfView {
            get {
                return profileService.ActiveProfile.FramingAssistantSettings.FieldOfView;
            }
            set {
                profileService.ActiveProfile.FramingAssistantSettings.FieldOfView = value;
                RaisePropertyChanged();
            }
        }

        public int CameraWidth {
            get {
                return profileService.ActiveProfile.FramingAssistantSettings.CameraWidth;
            }
            set {
                profileService.ActiveProfile.FramingAssistantSettings.CameraWidth = value;
                RaisePropertyChanged();
                CalculateRectangle(SkyMapAnnotator.ViewportFoV);
            }
        }

        public int CameraHeight {
            get {
                return profileService.ActiveProfile.FramingAssistantSettings.CameraHeight;
            }
            set {
                profileService.ActiveProfile.FramingAssistantSettings.CameraHeight = value;
                RaisePropertyChanged();
                CalculateRectangle(SkyMapAnnotator.ViewportFoV);
            }
        }

        private SkySurveySource _framingAssistantSource;

        public SkySurveySource FramingAssistantSource {
            get {
                return _framingAssistantSource;
            }
            set {
                _framingAssistantSource = value;
                if (profileService.ActiveProfile.FramingAssistantSettings.LastSelectedImageSource != value) {
                    profileService.ActiveProfile.FramingAssistantSettings.LastSelectedImageSource = _framingAssistantSource;
                }

                RaisePropertyChanged();
            }
        }

        private double _cameraPixelSize;

        public double CameraPixelSize {
            get {
                return _cameraPixelSize;
            }
            set {
                _cameraPixelSize = value;
                RaisePropertyChanged();
                CalculateRectangle(SkyMapAnnotator.ViewportFoV);
            }
        }

        private AsyncObservableCollection<FramingRectangle> cameraRectangles;

        public AsyncObservableCollection<FramingRectangle> CameraRectangles {
            get {
                if (cameraRectangles == null) {
                    cameraRectangles = new AsyncObservableCollection<FramingRectangle>();
                }
                return cameraRectangles;
            }
            set {
                cameraRectangles = value;
                RaisePropertyChanged();
            }
        }

        private int horizontalPanels = 1;

        public int HorizontalPanels {
            get {
                return horizontalPanels;
            }
            set {
                horizontalPanels = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsMosaic));
                CalculateRectangle(SkyMapAnnotator.ViewportFoV);
            }
        }

        private int verticalPanels = 1;

        public int VerticalPanels {
            get {
                return verticalPanels;
            }
            set {
                verticalPanels = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsMosaic));
                CalculateRectangle(SkyMapAnnotator.ViewportFoV);
            }
        }

        public bool IsMosaic {
            get => VerticalPanels > 1 || HorizontalPanels > 1;
        }

        private double overlapPercentage = 0.2;

        public double OverlapPercentage {
            get {
                return overlapPercentage;
            }
            set {
                overlapPercentage = value;
                RaisePropertyChanged();
                CalculateRectangle(SkyMapAnnotator.ViewportFoV);
            }
        }

        private double _focalLength;

        public double FocalLength {
            get {
                return _focalLength;
            }
            set {
                _focalLength = value;
                RaisePropertyChanged();
                CalculateRectangle(SkyMapAnnotator.ViewportFoV);
            }
        }

        private SkySurveyImage _imageParameter;

        public SkySurveyImage ImageParameter {
            get {
                return _imageParameter;
            }
            set {
                _imageParameter = value;
                RaisePropertyChanged();
            }
        }

        private FramingRectangle _rectangle;

        public FramingRectangle Rectangle {
            get {
                return _rectangle;
            }
            set {
                _rectangle = value;
                RaisePropertyChanged();
            }
        }

        private IProgress<int> _progress;

        private CancellationTokenSource _loadImageSource;
        private CancellationTokenSource slewTokenSource;
        private CancellationTokenSource getRotationTokenSource;

        private IProgress<ApplicationStatus> _statusUpdate;

        private async Task<bool> LoadImage() {
            using (MyStopWatch.Measure()) {
                CancelLoadImage();
                _loadImageSource?.Dispose();
                _loadImageSource = new CancellationTokenSource();
                try {
                    Logger.Info($"Loading image from source {FramingAssistantSource} with field of view {FieldOfView}° for coordinates {DSO?.Coordinates}");

                    if (DllLoader.IsX86()) {
                        await _dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
                            ImageParameter = null;
                            GC.Collect();
                        }));
                    }

                    SkySurveyImage skySurveyImage = null;

                    if (Cache != null && DSO != null) {
                        try {
                            skySurveyImage = await Cache.GetImage(FramingAssistantSource.GetCacheSourceString(), DSO.Coordinates.RA, DSO.Coordinates.Dec, DSO.Rotation, AstroUtil.DegreeToArcmin(FieldOfView));
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    }

                    if (skySurveyImage == null) {
                        if (FramingAssistantSource == SkySurveySource.CACHE) {
                            if (Cache == null) {
                                throw new Exception("Cache unavailable. Check log file for errors");
                            }
                            if (SelectedImageCacheInfo != null) {
                                skySurveyImage = await Cache.GetImage(Guid.Parse(SelectedImageCacheInfo.Attribute("Id").Value));
                            }
                        } else {
                            var skySurvey = SkySurveyFactory.Create(FramingAssistantSource);

                            skySurveyImage = await skySurvey.GetImage(DSO?.Name, DSO?.Coordinates,
                                AstroUtil.DegreeToArcmin(FieldOfView), boundWidth, boundHeight, _loadImageSource.Token, _progress);
                        }
                    }

                    if (skySurveyImage != null) {
                        skySurveyImage.Image.Freeze();
                        if (!string.IsNullOrWhiteSpace(DeepSkyObjectSearchVM.TargetName)) {
                            skySurveyImage.Name = DeepSkyObjectSearchVM.TargetName;
                        }

                        if (FramingAssistantSource == SkySurveySource.FILE) {
                            var fileSkySurveyImage = skySurveyImage as FileSkySurveyImage;

                            DeepSkyObjectSearchVM.SetTargetNameWithoutSearch(fileSkySurveyImage.Name);
                            DSO.Name = fileSkySurveyImage.Name;

                            if (fileSkySurveyImage.Data.MetaData.WorldCoordinateSystem == null) {
                                skySurveyImage = await PlateSolveSkySurvey(fileSkySurveyImage);
                            } else {
                                this.DSO.Coordinates = fileSkySurveyImage.Data.MetaData.WorldCoordinateSystem.Coordinates;
                                RaiseCoordinatesChanged();
                            }
                            Rectangle = null;
                        }

                        await _dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
                            ImageParameter = null;
                            GC.Collect();
                            ImageParameter = skySurveyImage;
                        }));

                        if (Cache != null && FramingAssistantSource != SkySurveySource.SKYATLAS && FramingAssistantSource != SkySurveySource.FILE) {
                            SelectedImageCacheInfo = Cache.SaveImageToCache(skySurveyImage);
                            RaisePropertyChanged(nameof(ImageCacheInfo));
                        }

                        await SkyMapAnnotator.Initialize(skySurveyImage.Coordinates, AstroUtil.ArcminToDegree(skySurveyImage.FoVHeight), ImageParameter.Image.PixelWidth, ImageParameter.Image.PixelHeight, ImageParameter.Rotation, _loadImageSource.Token);
                        SkyMapAnnotator.DynamicFoV = FramingAssistantSource == SkySurveySource.SKYATLAS;

                        CalculateRectangle(SkyMapAnnotator.ViewportFoV);
                    }
                } catch (OperationCanceledException) {
                    Logger.Info("Loading image for framing has been cancelled");
                } catch (Exception ex) {
                    Logger.Error($"Failed to load image from source {FramingAssistantSource} with field of view {FieldOfView}° for coordinates {DSO?.Coordinates}.", ex);
                    Notification.ShowError(ex.Message);
                }
                return true;
            }
        }

        private async Task<FileSkySurveyImage> PlateSolveSkySurvey(FileSkySurveyImage skySurveyImage) {
            var referenceCoordinates = skySurveyImage.Coordinates != null ? skySurveyImage.Coordinates : DSO.Coordinates;

            var diagResult = MyMessageBox.Show(string.Format(Loc.Instance["LblBlindSolveAttemptForFraming"], referenceCoordinates.RAString, referenceCoordinates.DecString), Loc.Instance["LblNoCoordinates"], MessageBoxButton.YesNo, MessageBoxResult.Yes);

            if (diagResult == MessageBoxResult.No) {
                referenceCoordinates = null;
            }
            var plateSolver = PlateSolverFactory.GetPlateSolver(profileService.ActiveProfile.PlateSolveSettings);
            var blindSolver = PlateSolverFactory.GetBlindSolver(profileService.ActiveProfile.PlateSolveSettings);

            var focalLength = double.IsNaN(skySurveyImage.Data.MetaData.Telescope.FocalLength) ? this.FocalLength : skySurveyImage.Data.MetaData.Telescope.FocalLength;
            var pixelSize = double.IsNaN(skySurveyImage.Data.MetaData.Camera.PixelSize) ? this.CameraPixelSize : skySurveyImage.Data.MetaData.Camera.PixelSize;

            var parameter = new PlateSolveParameter() {
                Binning = skySurveyImage.Data.MetaData.Camera.BinX,
                Coordinates = referenceCoordinates,
                DownSampleFactor = profileService.ActiveProfile.PlateSolveSettings.DownSampleFactor,
                FocalLength = focalLength,
                MaxObjects = profileService.ActiveProfile.PlateSolveSettings.MaxObjects,
                PixelSize = pixelSize,
                Regions = profileService.ActiveProfile.PlateSolveSettings.Regions,
                SearchRadius = profileService.ActiveProfile.PlateSolveSettings.SearchRadius,
                BlindFailoverEnabled = profileService.ActiveProfile.PlateSolveSettings.BlindFailoverEnabled
            };

            var imageSolver = new ImageSolver(plateSolver, blindSolver);
            var psResult = await imageSolver.Solve(skySurveyImage.Data, parameter, _statusUpdate, _loadImageSource.Token);

            if (psResult?.Success == true) {
                var rotation = psResult.Orientation;
                if (rotation < 0) {
                    rotation += 360;
                } else if (rotation >= 360) {
                    rotation -= 360;
                }
                skySurveyImage.Coordinates = psResult.Coordinates;
                skySurveyImage.FoVWidth = AstroUtil.ArcsecToArcmin(psResult.Pixscale * skySurveyImage.Image.PixelWidth);
                skySurveyImage.FoVHeight = AstroUtil.ArcsecToArcmin(psResult.Pixscale * skySurveyImage.Image.PixelHeight);
                skySurveyImage.Rotation = rotation;

                if (psResult.Flipped) {
                    var tb = new TransformedBitmap();
                    tb.BeginInit();
                    tb.Source = skySurveyImage.Image;
                    var transform = new ScaleTransform(-1, 1, 0, 0);
                    tb.Transform = transform;
                    tb.EndInit();
                    skySurveyImage.Image = tb;
                }

                this.DSO.Coordinates = psResult.Coordinates;
                RaiseCoordinatesChanged();
            } else {
                throw new Exception("Platesolve failed to retrieve coordinates for image");
            }

            return skySurveyImage;
        }

        public XElement ImageCacheInfo { get; set; }

        private CacheSkySurvey Cache { get; set; }

        private XElement _selectedImageCacheInfo;

        public XElement SelectedImageCacheInfo {
            get {
                return _selectedImageCacheInfo;
            }
            set {
                _selectedImageCacheInfo = value;
                if (_selectedImageCacheInfo != null) {
                    var ra = double.Parse(_selectedImageCacheInfo.Attribute("RA").Value, CultureInfo.InvariantCulture);
                    var dec = double.Parse(_selectedImageCacheInfo.Attribute("Dec").Value, CultureInfo.InvariantCulture);
                    var name = _selectedImageCacheInfo.Attribute("Name").Value;
                    var coordinates = new Coordinates(ra, dec, Epoch.J2000, Coordinates.RAType.Hours);
                    FieldOfView = AstroUtil.ArcminToDegree(double.Parse(_selectedImageCacheInfo.Attribute("FoVW").Value, CultureInfo.InvariantCulture));
                    DSO = new DeepSkyObject(name, coordinates, profileService.ActiveProfile.ApplicationSettings.SkyAtlasImageRepository, profileService.ActiveProfile.AstrometrySettings.Horizon);
                    DeepSkyObjectSearchVM.SetTargetNameWithoutSearch(name);
                    RaiseCoordinatesChanged();
                }
                RaisePropertyChanged();
            }
        }

        private void CalculateRectangle(ViewportFoV parameter) {
            if (parameter != null) {
                var previousRotation = 0d;
                if(Rectangle != null) {
                    previousRotation = AstroUtil.EuclidianModulus(Rectangle.TotalRotation - parameter.Rotation, 360);
                }                
                Rectangle = null;
                CameraRectangles.Clear();

                var centerCoordinates = parameter.CenterCoordinates;

                var imageArcsecWidth = AstroUtil.DegreeToArcsec(parameter.OriginalHFoV) / parameter.OriginalWidth;
                var imageArcsecHeight = AstroUtil.DegreeToArcsec(parameter.OriginalVFoV) / parameter.OriginalHeight;

                var arcsecPerPix = AstroUtil.ArcsecPerPixel(CameraPixelSize, FocalLength);
                var conversion = arcsecPerPix / imageArcsecWidth;

                var width = CameraWidth * conversion;
                var height = CameraHeight * conversion;
                var x = parameter.OriginalWidth / 2d - width / 2d;
                var y = parameter.OriginalHeight / 2d - height / 2d;

                if (HorizontalPanels == 1 && VerticalPanels == 1) {
                    var rect = new FramingRectangle(parameter.Rotation, 0, 0, width, height) {
                        Rotation = 0,
                        Coordinates = centerCoordinates,
                        DSORotation = AstroUtil.EuclidianModulus(previousRotation + parameter.Rotation, 360),
                        OriginalCoordinates = centerCoordinates
                    };
                    var name = GetRectangleName(rect);
                    rect.Name = name;
                    CameraRectangles.Add(rect);
                } else {
                    var panelWidth = CameraWidth * conversion;
                    var panelHeight = CameraHeight * conversion;

                    var panelOverlapWidth = CameraWidth * OverlapPercentage * conversion;
                    var panelOverlapHeight = CameraHeight * OverlapPercentage * conversion;

                    width = HorizontalPanels * panelWidth - (HorizontalPanels - 1) * panelOverlapWidth;
                    height = VerticalPanels * panelHeight - (VerticalPanels - 1) * panelOverlapHeight;
                    x = parameter.OriginalWidth / 2d - width / 2d;
                    y = parameter.OriginalHeight / 2d - height / 2d;
                    var center = new Point(x + width / 2d, y + height / 2d);

                    var id = 1;

                    for (int j = 0; j < VerticalPanels; j++) {
                        for (int i = 0; i < HorizontalPanels; i++) {
                            var panelId = id++;

                            var panelX = i * panelWidth - i * panelOverlapWidth;
                            var panelY = j * panelHeight - j * panelOverlapHeight;

                            var panelCenter = new Point(panelX + x + panelWidth / 2d, panelY + y + panelHeight / 2d);

                            var panelDeltaX = panelCenter.X - center.X;
                            var panelDeltaY = panelCenter.Y - center.Y;

                            var referenceCenter = centerCoordinates.Shift(Math.Abs(panelDeltaX) < 1E-10 ? 1 : 0, panelDeltaY, previousRotation, imageArcsecWidth, imageArcsecHeight);

                            var panelCenterCoordinates = centerCoordinates.Shift(panelDeltaX, panelDeltaY, previousRotation, imageArcsecWidth, imageArcsecHeight);

                            double positionAngle = 90;
                            if (Math.Abs(centerCoordinates.RADegrees - panelCenterCoordinates.RADegrees) > 0.001 || Math.Abs(centerCoordinates.Dec - panelCenterCoordinates.Dec) > 0.001) {
                                positionAngle = AstroUtil.CalculatePositionAngle(referenceCenter.RADegrees, panelCenterCoordinates.RADegrees, referenceCenter.Dec, panelCenterCoordinates.Dec) + previousRotation;
                            }

                            double panelRotation = -(90 - positionAngle);
                            double dsoRotation = previousRotation + parameter.Rotation;
                            if (PreserveAlignment) {
                                panelRotation = 0;
                                dsoRotation += (90 - positionAngle);
                            }

                            var rect = new FramingRectangle(parameter.Rotation, panelX, panelY, panelWidth, panelHeight) {
                                Id = panelId,
                                Rotation = panelRotation,
                                Coordinates = panelCenterCoordinates,
                                DSORotation = AstroUtil.EuclidianModulus(dsoRotation, 360),
                                OriginalCoordinates = panelCenterCoordinates
                            };
                            var name = GetRectangleName(rect);
                            rect.Name = name;
                            CameraRectangles.Add(rect);
                        }
                    }
                }

                Rectangle = new FramingRectangle(parameter.Rotation, x, y, width, height) {
                    Rotation = previousRotation,
                    Coordinates = centerCoordinates,
                    OriginalCoordinates = centerCoordinates
                };
                RectangleCalculated = Rectangle?.Coordinates != null;

                FontSize = Math.Max(1, (int)((height / verticalPanels) * 0.1));
            }
        }

        private void DragStart(object obj) {
        }

        private void DragStop(object obj) {
        }

        private void DragMove(object obj) {
            if (RectangleCalculated) {
                var delta = ((DragResult)obj).Delta;
                if (FramingAssistantSource == SkySurveySource.SKYATLAS) {
                    delta = new Vector(-delta.X, -delta.Y);

                    var newCenter = SkyMapAnnotator.ShiftViewport(delta);
                    DSO.Coordinates = newCenter;
                    ImageParameter.Coordinates = newCenter;
                    CalculateRectangle(SkyMapAnnotator.ViewportFoV); 
                    RaiseCoordinatesChanged();

                    SkyMapAnnotator.UpdateSkyMap();
                } else {
                    var imageArcsecWidth =
                        AstroUtil.ArcminToArcsec(ImageParameter.FoVWidth) / ImageParameter.Image.Width;
                    var imageArcsecHeight = AstroUtil.ArcminToArcsec(ImageParameter.FoVHeight) /
                                            ImageParameter.Image.Height;
                    this.Rectangle.X += delta.X;
                    this.Rectangle.Y += delta.Y;

                    var accumulatedDeltaX = this.Rectangle.X - this.Rectangle.OriginalX;
                    var accumulatedDeltaY = this.Rectangle.Y - this.Rectangle.OriginalY;

                    Rectangle.Coordinates = Rectangle.OriginalCoordinates.Shift(accumulatedDeltaX, accumulatedDeltaY, ImageParameter.Rotation,
                        imageArcsecWidth, imageArcsecHeight);
                    DSO.Coordinates = Rectangle.Coordinates;
                    RaiseCoordinatesChanged();


                    var mainRectangleReferenceCenter = Rectangle.OriginalCoordinates.Shift(Math.Abs(accumulatedDeltaX) < 1E-10 ? 1 : 0, accumulatedDeltaY, Rectangle.OriginalOffset, imageArcsecWidth, imageArcsecHeight);
                    double mainRectanglePA = 90;
                    var previousTotal = Rectangle.TotalRotation;
                    if (Math.Abs(Rectangle.OriginalCoordinates.RADegrees - Rectangle.Coordinates.RADegrees) > 0.001 || Math.Abs(Rectangle.OriginalCoordinates.Dec - Rectangle.Coordinates.Dec) > 0.001) {
                        mainRectanglePA = AstroUtil.CalculatePositionAngle(mainRectangleReferenceCenter.RADegrees, Rectangle.Coordinates.RADegrees, mainRectangleReferenceCenter.Dec, Rectangle.Coordinates.Dec) + Rectangle.OriginalOffset;

                        if(accumulatedDeltaX < 0 && Rectangle.Coordinates.Dec >= 0 || accumulatedDeltaX < 0 && Rectangle.Coordinates.Dec < 0) {
                            // When the rectangle is left of center, the PA has to be adjusted by 180°, otherwise it will end upside down
                           mainRectanglePA += 180;
                        }

                    }

                    Rectangle.RotationOffset = Rectangle.OriginalOffset - -(90 - mainRectanglePA);
                    Rectangle.Rotation = -(90 - mainRectanglePA) + previousTotal - Rectangle.OriginalOffset;
                    RaisePropertyChanged(nameof(RectangleRotation));
                    RaisePropertyChanged(nameof(RectangleTotalRotation));
                    RaisePropertyChanged(nameof(InverseRectangleRotation));

                    var center = new Point(Rectangle.X + Rectangle.Width / 2d, Rectangle.Y + Rectangle.Height / 2d);

                    foreach (var rect in CameraRectangles) {

                        var panelCenter = new Point(rect.X + Rectangle.X + rect.Width / 2d, rect.Y + Rectangle.Y + rect.Height / 2d);
                        var panelDeltaX = panelCenter.X - center.X;
                        var panelDeltaY = panelCenter.Y - center.Y;

                        rect.Coordinates = Rectangle.Coordinates.Shift(panelDeltaX, panelDeltaY, Rectangle.TotalRotation, imageArcsecWidth, imageArcsecHeight);

                        var referenceCenter = Rectangle.Coordinates.Shift(Math.Abs(panelDeltaX) < 1E-10 ? 1 : 0, panelDeltaY, Rectangle.Rotation, imageArcsecWidth, imageArcsecHeight);

                        var panelCenterCoordinates = Rectangle.Coordinates.Shift(panelDeltaX, panelDeltaY, Rectangle.Rotation, imageArcsecWidth, imageArcsecHeight);

                        double positionAngle = 90;
                        if (Math.Abs(Rectangle.Coordinates.RADegrees - panelCenterCoordinates.RADegrees) > 0.001 || Math.Abs(Rectangle.Coordinates.Dec - panelCenterCoordinates.Dec) > 0.001) {
                            positionAngle = AstroUtil.CalculatePositionAngle(referenceCenter.RADegrees, panelCenterCoordinates.RADegrees, referenceCenter.Dec, panelCenterCoordinates.Dec) + Rectangle.Rotation;
                            
                        }

                        double panelRotation = -(90 - positionAngle);
                        double dsoRotation = Rectangle.TotalRotation;
                        if (PreserveAlignment) {
                            panelRotation = 0;
                            dsoRotation += (90 - positionAngle);
                        }
                        
                        rect.Rotation = panelRotation;
                        rect.DSORotation = AstroUtil.EuclidianModulus(dsoRotation, 360);
                    }
                }
            }
        }

        private bool prevCameraConnected = false;
        private ISkyMapAnnotator skyMapAnnotator;

        public void UpdateDeviceInfo(CameraInfo cameraInfo) {
            if (cameraInfo != null) {
                if (cameraInfo.Connected == true && prevCameraConnected == false) {
                    if (this.CameraWidth != cameraInfo.XSize && cameraInfo.XSize > 0) {
                        this.CameraWidth = cameraInfo.XSize;
                    }
                    if (this.CameraHeight != cameraInfo.YSize && cameraInfo.YSize > 0) {
                        this.CameraHeight = cameraInfo.YSize;
                    }
                    if (Math.Abs(this.CameraPixelSize - cameraInfo.PixelSize) > 0.01d && cameraInfo.PixelSize > 0) {
                        CameraPixelSize = cameraInfo.PixelSize;
                    }
                }
                prevCameraConnected = cameraInfo.Connected;
            }
        }

        private async Task<bool> CoordsFromPlanetarium() {
            IPlanetarium s = planetariumFactory.GetPlanetarium();
            DeepSkyObject resp = null;

            try {
                resp = await s.GetTarget();

                if (resp != null) {
                    await SetCoordinates(resp);
                    Notification.ShowSuccess(string.Format(Loc.Instance["LblPlanetariumCoordsOk"], s.Name));

                    if (s.CanGetRotationAngle) {
                        double rotationAngle = await s.GetRotationAngle();

                        if (!double.IsNaN(rotationAngle)) {
                            RectangleRotation = rotationAngle;
                        }
                    }
                }
            } catch (PlanetariumObjectNotSelectedException) {
                Logger.Error($"Attempted to get coordinates from {s.Name} when no object was selected");
                Notification.ShowError(string.Format(Loc.Instance["LblPlanetariumObjectNotSelected"], s.Name));
            } catch (PlanetariumFailedToConnect ex) {
                Logger.Error($"Unable to connect to {s.Name}: {ex}");
                Notification.ShowError(string.Format(Loc.Instance["LblPlanetariumFailedToConnect"], s.Name));
            } catch (Exception ex) {
                Logger.Error($"Failed to get coordinates from {s.Name}: {ex}");
                Notification.ShowError(string.Format(Loc.Instance["LblPlanetariumCoordsError"], s.Name));
            }

            return (resp != null);
        }

        private async Task<bool> CoordsFromScope() {
            var telescopeInfo = telescopeMediator.GetInfo();
            if (!telescopeInfo.Connected) {
                Notification.ShowError(Loc.Instance["LblTelescopeNotConnected"]);
                return false;
            }

            var dso = new DeepSkyObject(string.Empty, telescopeInfo.Coordinates, string.Empty, null);
            await SetCoordinates(dso);
            return true;
        }

        public void Dispose() {
            this.cameraMediator.RemoveConsumer(this);
        }

        public ICommand CoordsFromPlanetariumCommand { get; set; }
        public ICommand CoordsFromScopeCommand { get; set; }
        public ICommand DragStartCommand { get; private set; }
        public ICommand DragStopCommand { get; private set; }
        public ICommand DragMoveCommand { get; private set; }
        public IAsyncCommand LoadImageCommand { get; private set; }
        public ICommand CancelLoadImageCommand { get; private set; }
        public ICommand SetOldSequencerTargetCommand { get; private set; }
        public ICommand SetSequencerTargetCommand { get; private set; }
        public ICommand AddTargetToTargetListCommand { get; private set; }
        public ICommand GetDSOTemplatesCommand { get; private set; }
        public ICommand GetExistingSequencerTargetsCommand { get; private set; }
        public ICommand UpdateExistingTargetInSequencerCommand { get; private set; }
        public IAsyncCommand SlewToCoordinatesCommand { get; private set; }
        public ICommand CancelSlewToCoordinatesCommand { get; private set; }
        public ICommand CancelLoadImageFromFileCommand { get; private set; }
        public ICommand ClearCacheCommand { get; private set; }
        public ICommand ScrollViewerSizeChangedCommand { get; private set; }
        public ICommand RefreshSkyMapAnnotationCommand { get; private set; }
        public ICommand MouseWheelCommand { get; private set; }
        public IAsyncCommand GetRotationFromCameraCommand { get; private set; }
        public ICommand CancelGetRotationFromCameraCommand { get; private set; }

        public ISkyMapAnnotator SkyMapAnnotator {
            get => skyMapAnnotator;
            set {
                skyMapAnnotator = value;
                RaisePropertyChanged();
            }
        }
    }
}