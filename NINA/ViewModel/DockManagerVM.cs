#region "copyright"
/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using NINA.Utility;
using NINA.Profile.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using NINA.WPF.Base.ViewModel.Equipment.FilterWheel;
using NINA.WPF.Base.ViewModel.Equipment.Rotator;
using NINA.WPF.Base.ViewModel.Equipment.Guider;
using NINA.ViewModel.Interfaces;
using NINA.WPF.Base.ViewModel.Equipment.Camera;
using NINA.WPF.Base.ViewModel.Equipment.Focuser;
using NINA.ViewModel.Imaging;
using NINA.WPF.Base.ViewModel.Equipment.Dome;
using NINA.WPF.Base.ViewModel.Equipment.Switch;
using NINA.WPF.Base.ViewModel.Equipment.Telescope;
using NINA.WPF.Base.ViewModel.Equipment.WeatherData;
using NINA.WPF.Base.ViewModel.Equipment.FlatDevice;
using NINA.ViewModel.Sequencer;
using NINA.ViewModel.ImageHistory;
using NINA.WPF.Base.ViewModel.Equipment.SafetyMonitor;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using NINA.Core.Utility;
using NINA.Core.Locale;
using NINA.Core.MyMessageBox;
using NINA.Profile;
using System.Linq;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.ViewModel;
using NINA.Plugin.Interfaces;
using System.Diagnostics;
using System.Threading;

namespace NINA.ViewModel {

    internal class DockManagerVM : BaseVM, IDockManagerVM {

        public DockManagerVM(IProfileService profileService,
                             ICameraVM cameraVM,
                             ISequenceNavigationVM sequenceNavigationVM,
                             IThumbnailVM thumbnailVM,
                             ISwitchVM switchVM,
                             IFilterWheelVM filterWheelVM,
                             IFocuserVM focuserVM,
                             IRotatorVM rotatorVM,
                             IWeatherDataVM weatherDataVM,
                             IDomeVM domeVM,
                             IAnchorableSnapshotVM snapshotVM,
                             IAnchorablePlateSolverVM plateSolverVM,
                             ITelescopeVM telescopeVM,
                             IGuiderVM guiderVM,
                             IFocusTargetsVM focusTargetsVM,
                             IAutoFocusToolVM autoFocusToolVM,
                             IImageHistoryVM imageHistoryVM,
                             IImageControlVM imageControlVM,
                             IImageStatisticsVM imageStatisticsVM,
                             IFlatDeviceVM flatDeviceVM,
                             ISafetyMonitorVM safetyMonitorVM,
                             IPluginLoader pluginProvider) : base(profileService) {
            LoadAvalonDockLayoutCommand = new AsyncCommand<bool>((object o) => Task.Run(() => InitializeAvalonDockLayout(o)));
            ResetDockLayoutCommand = new RelayCommand(ResetDockLayout, (object o) => _dockmanager != null);

            var initAnchorables = new List<IDockableVM>();
            var initAnchorableInfoPanels = new List<IDockableVM>();
            var initAnchorableTools = new List<IDockableVM>();

            initAnchorables.Add(imageControlVM);
            initAnchorables.Add(cameraVM);
            initAnchorables.Add(filterWheelVM);
            initAnchorables.Add(focuserVM);
            initAnchorables.Add(rotatorVM);
            initAnchorables.Add(telescopeVM);
            initAnchorables.Add(guiderVM);
            initAnchorables.Add(switchVM);
            initAnchorables.Add(weatherDataVM);
            initAnchorables.Add(domeVM);

            initAnchorables.Add(sequenceNavigationVM);
            initAnchorables.Add(imageStatisticsVM);
            initAnchorables.Add(imageHistoryVM);

            initAnchorables.Add(snapshotVM);
            initAnchorables.Add(thumbnailVM);
            initAnchorables.Add(plateSolverVM);
            initAnchorables.Add(autoFocusToolVM);
            initAnchorables.Add(focusTargetsVM);
            initAnchorables.Add(flatDeviceVM);
            initAnchorables.Add(safetyMonitorVM);

            initAnchorableInfoPanels.Add(imageControlVM);
            initAnchorableInfoPanels.Add(cameraVM);
            initAnchorableInfoPanels.Add(filterWheelVM);
            initAnchorableInfoPanels.Add(focuserVM);
            initAnchorableInfoPanels.Add(rotatorVM);
            initAnchorableInfoPanels.Add(telescopeVM);
            initAnchorableInfoPanels.Add(guiderVM);
            initAnchorableInfoPanels.Add(sequenceNavigationVM);
            initAnchorableInfoPanels.Add(switchVM);
            initAnchorableInfoPanels.Add(weatherDataVM);
            initAnchorableInfoPanels.Add(domeVM);
            initAnchorableInfoPanels.Add(imageStatisticsVM);
            initAnchorableInfoPanels.Add(imageHistoryVM);
            initAnchorableInfoPanels.Add(flatDeviceVM);
            initAnchorableInfoPanels.Add(safetyMonitorVM);

            initAnchorableTools.Add(snapshotVM);
            initAnchorableTools.Add(thumbnailVM);
            initAnchorableTools.Add(plateSolverVM);
            initAnchorableTools.Add(autoFocusToolVM);
            initAnchorableTools.Add(focusTargetsVM);

            ClosingCommand = new RelayCommand(ClosingApplication);

            profileService.ProfileChanged += ProfileService_ProfileChanged;

            Task.Run(async () => {
                await pluginProvider.Load();
                foreach (var dockable in pluginProvider.DockableVMs) {
                    initAnchorables.Add(dockable);
                    if (dockable.IsTool) {
                        initAnchorableTools.Add(dockable);
                    } else {
                        initAnchorableInfoPanels.Add(dockable);
                    }
                }
                Anchorables = initAnchorables;
                AnchorableInfoPanels = initAnchorableInfoPanels;
                AnchorableTools = initAnchorableTools;
                Initialized = true;
            });
        }

        private bool initialized;

        public bool Initialized {
            get {
                lock (lockObj) {
                    return initialized;
                }
            }
            private set {
                lock (lockObj) {
                    initialized = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            lock (lockObj) {
                _dockloaded = false;
            }
        }

        private void ResetDockLayout(object arg) {
            if (MyMessageBox.Show(Loc.Instance["LblResetDockLayoutConfirmation"], Loc.Instance["LblResetDockLayout"], System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No) == System.Windows.MessageBoxResult.Yes) {
                lock (lockObj) {
                    _dockloaded = false;

                    foreach (var item in Anchorables) {
                        item.IsVisible = false;
                    }

                    var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(_dockmanager);
                    serializer.LayoutSerializationCallback += (s, args) => {
                        if (args.Content is DockableVM d) {
                            d.IsVisible = true;
                            args.Content = d;
                        }
                    };

                    LoadDefaultLayout(serializer);
                    SaveAvalonDockLayout();
                }
            }
        }

        private List<IDockableVM> _anchorables;

        public List<IDockableVM> Anchorables {
            get {
                if (_anchorables == null) {
                    _anchorables = new List<IDockableVM>();
                }
                return _anchorables;
            }
            private set {
                _anchorables = value;
                RaisePropertyChanged();
            }
        }

        private List<IDockableVM> _anchorableTools;

        public List<IDockableVM> AnchorableTools {
            get {
                if (_anchorableTools == null) {
                    _anchorableTools = new List<IDockableVM>();
                }
                return _anchorableTools;
            }
            private set {
                _anchorableTools = value;
                RaisePropertyChanged();
            }
        }

        private List<IDockableVM> _anchorableInfoPanels;

        public List<IDockableVM> AnchorableInfoPanels {
            get {
                if (_anchorableInfoPanels == null) {
                    _anchorableInfoPanels = new List<IDockableVM>();
                }
                return _anchorableInfoPanels;
            }
            private set {
                _anchorableInfoPanels = value;
                RaisePropertyChanged();
            }
        }

        private AvalonDock.DockingManager _dockmanager;
        private bool _dockloaded = false;
        private object lockObj = new object();
        private Dispatcher _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        public async Task<bool> InitializeAvalonDockLayout(object o) {
            lock (lockObj) {
                if (Initialized && _dockloaded) { return true; }
            }

            while (!Initialized) {
                await Task.Delay(100);
            };

            await _dispatcher.BeginInvoke(new Action(() => {
                lock (lockObj) {
                    if (!_dockloaded) {
                        _dockmanager = (AvalonDock.DockingManager)o;

                        foreach (var item in Anchorables) {
                            item.IsVisible = false;
                        }

                        var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(_dockmanager);
                        var success = true;
                        var dupeCheck = new HashSet<string>();
                        serializer.LayoutSerializationCallback += (s, args) => {
                            if (args?.Model != null) {
                                if (dupeCheck.Contains(args.Model.ContentId)) {
                                    Logger.Trace($"Duplicate entry detected for content id: {args.Model.ContentId}");
                                    args.Cancel = true;
                                } else {
                                    dupeCheck.Add(args.Model.ContentId);
                                    if (args.Content == null) {
                                        var context = Anchorables.FirstOrDefault(x => x.ContentId == args.Model.ContentId);
                                        if (context == null) {
                                            Logger.Debug($"Content not found for content id: {args.Model.ContentId}");
                                            args.Cancel = true;
                                        } else {
                                            Logger.Trace($"Manually setting content for id: {args.Model.ContentId}");
                                            args.Content = context;
                                            success = false;
                                        }
                                    } else {
                                        args.Content = args.Content;
                                    }
                                }
                            } else {
                                args.Cancel = true;
                            }
                        };

                        var profileId = profileService.ActiveProfile.Id;
                        var profilePath = Path.Combine(ProfileService.PROFILEFOLDER, $"{profileId}.dock.config");
                        if (File.Exists(profilePath)) {
                            try {
                                Logger.Info($"Initializing imaging tab layout from {profilePath}");
                                using (var sw = new FileStream(profilePath, FileMode.Open, FileAccess.Read)) {
                                    serializer.Deserialize(sw);
                                    if (success) {
                                        _dockloaded = true;
                                    } else {
                                        // Retry
                                        Logger.Debug("The dock did not succeed to load. Trying again");
                                        _dockloaded = false;

                                        foreach (var item in Anchorables) {
                                            item.IsVisible = false;
                                        }
                                        dupeCheck.Clear();
                                        var serializer2 = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(_dockmanager);
                                        serializer2.LayoutSerializationCallback += (s, args) => {
                                            if (args?.Model != null) {
                                                if (dupeCheck.Contains(args.Model.ContentId)) {
                                                    Logger.Trace($"Duplicate entry detected for content id: {args.Model.ContentId}");
                                                    args.Cancel = true;
                                                } else {
                                                    dupeCheck.Add(args.Model.ContentId);
                                                    var d = (DockableVM)args.Content;
                                                    if (d != null) {
                                                        d.IsVisible = true;
                                                        args.Content = d;
                                                    }
                                                }
                                            } else {
                                                args.Cancel = true;
                                            }
                                        };
                                        sw.Seek(0, SeekOrigin.Begin);
                                        serializer2.Deserialize(sw);
                                        _dockloaded = true;
                                    }
                                }
                            } catch (Exception ex) {
                                Logger.Error("Failed to load imaging tab layout. Loading default Layout!", ex);
                                LoadDefaultLayout(serializer);
                            }
                        } else if (File.Exists(Path.Combine(CoreUtil.APPLICATIONTEMPPATH, "avalondock.config"))) {
                            try {
                                Logger.Info("Migrating imaging tab layout from old path");
                                serializer.Deserialize(Path.Combine(CoreUtil.APPLICATIONTEMPPATH, "avalondock.config"));
                                _dockloaded = true;
                            } catch (Exception ex) {
                                Logger.Error("Failed to load imaging tab layout. Loading default Layout!", ex);
                                LoadDefaultLayout(serializer);
                            }
                        } else {
                            LoadDefaultLayout(serializer);
                        }
                    }
                }
            }), DispatcherPriority.Normal);
            return true;
        }

        private void LoadDefaultLayout(AvalonDock.Layout.Serialization.XmlLayoutSerializer serializer) {
            using (var stream = new StringReader(Properties.Resources.avalondock)) {
                serializer.Deserialize(stream);
                _dockloaded = true;
            }
        }

        public void SaveAvalonDockLayout() {
            lock (lockObj) {
                if (_dockloaded) {
                    var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(_dockmanager);

                    var profileId = profileService.ActiveProfile.Id;
                    var profilePath = Path.Combine(ProfileService.PROFILEFOLDER, $"{profileId}.dock.config");
                    serializer.Serialize(profilePath);
                }
            }
        }

        private void ClosingApplication(object o) {
            try {
                SaveAvalonDockLayout();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public IAsyncCommand LoadAvalonDockLayoutCommand { get; private set; }
        public ICommand ResetDockLayoutCommand { get; }

        public ICommand ClosingCommand { get; private set; }
    }
}