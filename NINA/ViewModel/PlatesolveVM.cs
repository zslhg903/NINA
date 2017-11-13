﻿using NINA.Model;
using NINA.Model.MyCamera;
using NINA.Model.MyTelescope;
using NINA.PlateSolving;
using NINA.Utility;
using NINA.Utility.Astrometry;
using NINA.Utility.Notification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NINA.ViewModel {
    class PlatesolveVM : DockableVM {

        public const string ASTROMETRYNETURL = "http://nova.astrometry.net";

        public PlatesolveVM() : base() {
            Title = "LblPlateSolving";
            ContentId = nameof(PlatesolveVM);
            SyncScope = false;
            SlewToTarget = false;
            ImageGeometry = (System.Windows.Media.GeometryGroup)System.Windows.Application.Current.Resources["PlatesolveSVG"];

            SolveCommand = new AsyncCommand<bool>(() => CaptureSolveSyncAndReslew(new Progress<string>(p => Status = p)));
            CancelSolveCommand = new RelayCommand(CancelSolve);

            SnapExposureDuration = 2;
            Repeat = false;
            RepeatThreshold = 1.0d;

            RegisterMediatorMessages();
        }

        private void RegisterMediatorMessages() {
            Mediator.Instance.Register((object o) => {
                Telescope = (ITelescope)o;
            }, MediatorMessages.TelescopeChanged);

            Mediator.Instance.Register((object o) => {
                Cam = (ICamera)o;
            }, MediatorMessages.CameraChanged);

            Mediator.Instance.Register((object o) => {
                Image = (BitmapSource)o;
            }, MediatorMessages.ImageChanged);

            Mediator.Instance.Register((object o) => {
                _autoStretch = (bool)o;
            }, MediatorMessages.AutoStrechChanged);
            Mediator.Instance.Register((object o) => {
                _detectStars = (bool)o;
            }, MediatorMessages.DetectStarsChanged);

            Mediator.Instance.RegisterAsync(async (object o) => {
                var args = (object[])o;
                CaptureSequence seq = args[0] == null ? new CaptureSequence(this.SnapExposureDuration, CaptureSequence.ImageTypes.SNAP, this.SnapFilter, this.SnapBin, 1) : (CaptureSequence)args[0];
                IProgress<string> progress = (IProgress<string>)args[1];
                CancellationTokenSource token = (CancellationTokenSource)args[2];

                await SolveWithCapture(seq, progress, token);
            }, AsyncMediatorMessages.SolveWithCapture);

            Mediator.Instance.RegisterAsync(async (object o) => {
                var args = (object[])o;
                CancellationTokenSource token = (CancellationTokenSource)args[0];
                IProgress<string> progress = (IProgress<string>)args[1];
                Repeat = true;
                await CaptureSolveSyncAndReslew(token, progress);
            }, AsyncMediatorMessages.CaputureSolveSyncAndReslew);

            Mediator.Instance.RegisterAsync(async (object o) => {
                var args = (object[])o;
                IProgress<string> progress = (IProgress<string>)args[0];
                CancellationTokenSource token = (CancellationTokenSource)args[1];
                await Solve(progress, token);
            }, AsyncMediatorMessages.Solve);

            Mediator.Instance.Register((object o) => {
                SyncronizeTelescope();
            }, MediatorMessages.SyncronizeTelescope);
        }

        private string _status;
        public string Status {
            get {
                return _status;
            }
            set {
                _status = value;
                RaisePropertyChanged();

                Mediator.Instance.Notify(MediatorMessages.StatusUpdate, _status);
            }
        }

        private bool _syncScope;
        public bool SyncScope {
            get {
                return _syncScope;
            }
            set {
                _syncScope = value;
                if (!_syncScope && SlewToTarget) {
                    SlewToTarget = false;
                }
                RaisePropertyChanged();
            }
        }

        private bool _slewToTarget;
        public bool SlewToTarget {
            get {
                return _slewToTarget;
            }
            set {
                _slewToTarget = value;
                if (_slewToTarget && !SyncScope) {
                    SyncScope = true;
                }
                if (!_slewToTarget && Repeat) {
                    Repeat = false;
                }
                RaisePropertyChanged();
            }
        }

        private bool _repeat;
        public bool Repeat {
            get {
                return _repeat;
            }
            set {
                _repeat = value;
                if (_repeat && !SlewToTarget) {
                    SlewToTarget = true;
                }
                RaisePropertyChanged();
            }
        }

        private double _repeatThreshold;
        public double RepeatThreshold {
            get {
                return _repeatThreshold;
            }
            set {
                _repeatThreshold = value;
                RaisePropertyChanged();
            }
        }



        private BinningMode _snapBin;
        private Model.MyFilterWheel.FilterInfo _snapFilter;
        private double _snapExposureDuration;

        public BinningMode SnapBin {
            get {
                return _snapBin;
            }

            set {
                _snapBin = value;
                RaisePropertyChanged();
            }
        }

        public Model.MyFilterWheel.FilterInfo SnapFilter {
            get {
                return _snapFilter;
            }

            set {
                _snapFilter = value;
                RaisePropertyChanged();
            }
        }

        public double SnapExposureDuration {
            get {
                return _snapExposureDuration;
            }

            set {
                _snapExposureDuration = value;
                RaisePropertyChanged();
            }
        }

        private void ImageChanged(Object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Image") {
                this.PlateSolveResult = null;
            }
        }

        /// <summary>
        /// Syncs telescope to solved coordinates
        /// </summary>
        /// <returns></returns>
        private bool SyncronizeTelescope() {
            var success = false;

            if (Telescope?.Connected != true) {
                Notification.ShowWarning(Locale.Loc.Instance["LblUnableToSync"]);
                return false;
            }

            if (PlateSolveResult != null && PlateSolveResult.Success) {

                Coordinates solved = PlateSolveResult.Coordinates;
                solved = solved.Transform(Settings.EpochType);  //Transform to JNow if required

                if (Telescope.Sync(solved.RA, solved.Dec) == true) {
                    Notification.ShowSuccess(Locale.Loc.Instance["LblTelescopeSynced"]);
                    success = true;
                } else {
                    Notification.ShowWarning(Locale.Loc.Instance["LblSyncFailed"]);
                }

            } else {
                Notification.ShowWarning(Locale.Loc.Instance["LblNoCoordinatesForSync"]);
            }
            return success;
        }

        private BitmapSource _image;
        public BitmapSource Image {
            get {
                return _image;
            }
            set {
                _image = value;
                RaisePropertyChanged();
            }
        }

        private bool _autoStretch;
        private bool _detectStars;

        /// <summary>
        /// Captures an image and solves it
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="progress"></param>
        /// <param name="canceltoken"></param>
        /// <param name="filter"></param>
        /// <param name="binning"></param>
        /// <returns></returns>
        private async Task<bool> SolveWithCapture(CaptureSequence seq, IProgress<string> progress, CancellationTokenSource canceltoken) {
            var oldAutoStretch = _autoStretch;
            var oldDetectStars = _detectStars;
            Mediator.Instance.Notify(MediatorMessages.ChangeAutoStretch, true);
            Mediator.Instance.Notify(MediatorMessages.ChangeDetectStars, false);


            await Mediator.Instance.NotifyAsync(AsyncMediatorMessages.CaptureImage, new object[] { seq, false, progress, canceltoken });

            Mediator.Instance.Notify(MediatorMessages.ChangeAutoStretch, oldAutoStretch);
            Mediator.Instance.Notify(MediatorMessages.ChangeDetectStars, oldDetectStars);

            canceltoken.Token.ThrowIfCancellationRequested();

            return await Solve(progress, canceltoken); ;
        }

        private async Task<bool> CaptureSolveSyncAndReslew(IProgress<string> progress) {
            _solveCancelToken = new CancellationTokenSource();
            return await this.CaptureSolveSyncAndReslew(_solveCancelToken, progress);
        }

        /// <summary>
        /// Calls "SolveWithCaputre" and syncs + reslews afterwards if set
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private async Task<bool> CaptureSolveSyncAndReslew(CancellationTokenSource tokensource, IProgress<string> progress) {
            bool solvedSuccessfully = false;
            bool repeatPlateSolve = false;
            do {

                var seq = new CaptureSequence(SnapExposureDuration, CaptureSequence.ImageTypes.SNAP, SnapFilter, SnapBin, 1);
                solvedSuccessfully = await SolveWithCapture(seq, progress, tokensource);

                if (solvedSuccessfully) {
                    if (SyncScope) {
                        if (Telescope?.Connected != true) {
                            Notification.ShowWarning(Locale.Loc.Instance["LblUnableToSync"]);
                            return false;
                        }
                        var coords = new Coordinates(Telescope.RightAscension, Telescope.Declination, Settings.EpochType, Coordinates.RAType.Hours);
                        if (SyncronizeTelescope() && SlewToTarget) {
                            Telescope.SlewToCoordinates(coords.RA, coords.Dec);
                        }
                    }
                }

                if (solvedSuccessfully && Repeat && Math.Abs(Astrometry.DegreeToArcmin(PlateSolveResult.RaError)) > RepeatThreshold) {
                    repeatPlateSolve = true;
                    progress.Report("Telescope not inside tolerance. Repeating...");
                    //Let the scope settle
                    await Task.Delay(2000);
                } else {
                    repeatPlateSolve = false;
                }

            } while (repeatPlateSolve);

            RaiseAllPropertiesChanged();
            return solvedSuccessfully;
        }

        /// <summary>
        /// Calculates the error based on the solved coordinates and the actual telescope coordinates and puts them into the PlateSolveResult
        /// </summary>
        private void CalculateError() {
            if (Telescope?.Connected == true) {

                Coordinates solved = PlateSolveResult.Coordinates;
                solved = solved.Transform(Settings.EpochType);

                var coords = new Coordinates(Telescope.RightAscension, Telescope.Declination, Settings.EpochType, Coordinates.RAType.Hours);

                PlateSolveResult.RaError = coords.RADegrees - solved.RADegrees;
                PlateSolveResult.DecError = coords.Dec - solved.Dec;
            }
        }

        /// <summary>
        /// Creates an instance of IPlatesolver, reads the image into memory and calls solve logic of platesolver
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="canceltoken"></param>
        /// <returns>true: success; false: fail</returns>
        public async Task<bool> Solve(IProgress<string> progress, CancellationTokenSource canceltoken) {
            var solver = GetPlateSolver();
            if (solver == null) {
                return false;
            }

            BitmapSource source = Image;
            BitmapFrame image = null;

            image = BitmapFrame.Create(source);

            /* Read image into memorystream */
            using (var ms = new MemoryStream()) {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder() { QualityLevel = 100 };
                encoder.Frames.Add(image);

                encoder.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                PlateSolveResult = await solver.SolveAsync(ms, progress, canceltoken);
            }

            if (!PlateSolveResult?.Success == true) {
                Notification.ShowWarning(Locale.Loc.Instance["LblPlatesolveFailed"]);
            }

            return PlateSolveResult?.Success ?? false;
        }

        private CancellationTokenSource _solveCancelToken;
        private void CancelSolve(object o) {
            _solveCancelToken?.Cancel();
        }

        private IPlateSolver GetPlateSolver() {
            IPlateSolver solver = null;
            if (Image != null) {
                Coordinates coords = null;
                if (Telescope?.Connected == true) {
                    coords = new Coordinates(Telescope.RightAscension, Telescope.Declination, Settings.EpochType, Coordinates.RAType.Hours);
                }
                var binning = Cam?.BinX ?? 1;

                solver = PlateSolverFactory.CreateInstance(binning, Image.Width, Image.Height, coords);
            }

            return solver;
        }

        private ITelescope _telescope;
        public ITelescope Telescope {
            get {
                return _telescope;
            }
            set {
                _telescope = value;
                RaisePropertyChanged();
            }
        }

        private ICamera _cam;
        public ICamera Cam {
            get {
                return _cam;
            }
            set {
                _cam = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand SolveCommand { get; private set; }

        public ICommand CancelSolveCommand { get; private set; }

        private AsyncObservableLimitedSizedStack<PlateSolveResult> _plateSolveResultList;
        public AsyncObservableLimitedSizedStack<PlateSolveResult> PlateSolveResultList {
            get {
                if (_plateSolveResultList == null) {
                    _plateSolveResultList = new AsyncObservableLimitedSizedStack<PlateSolveResult>(15);
                }
                return _plateSolveResultList;
            }
            set {
                _plateSolveResultList = value;
                RaisePropertyChanged();
            }
        }

        private PlateSolveResult _plateSolveResult;
        public PlateSolveResult PlateSolveResult {
            get {
                return _plateSolveResult;
            }

            set {
                _plateSolveResult = value;

                if (_plateSolveResult.Success) {
                    CalculateError();
                }
                PlateSolveResultList.Add(_plateSolveResult);

                RaisePropertyChanged();
                Mediator.Instance.Notify(MediatorMessages.PlateSolveResultChanged, _plateSolveResult);
            }
        }
    }
}
