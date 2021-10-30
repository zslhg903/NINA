#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

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
using NINA.Equipment.Interfaces.ViewModel;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.ViewModel;
using NINA.WPF.Base.ViewModel.Imaging;
using NINA.Plugin.Interfaces;

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
                             IPolarAlignmentVM polarAlignmentVM,
                             IAnchorablePlateSolverVM plateSolverVM,
                             ITelescopeVM telescopeVM,
                             IGuiderVM guiderVM,
                             IFocusTargetsVM focusTargetsVM,
                             IAutoFocusToolVM autoFocusToolVM,
                             IExposureCalculatorVM exposureCalculatorVM,
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
            initAnchorables.Add(polarAlignmentVM);
            initAnchorables.Add(autoFocusToolVM);
            initAnchorables.Add(focusTargetsVM);
            initAnchorables.Add(exposureCalculatorVM);
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
            initAnchorableTools.Add(polarAlignmentVM);
            initAnchorableTools.Add(autoFocusToolVM);
            initAnchorableTools.Add(focusTargetsVM);
            initAnchorableTools.Add(exposureCalculatorVM);

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
                        var d = (DockableVM)args.Content;
                        d.IsVisible = true;
                        args.Content = d;
                    };

                    LoadDefaultLayout(serializer);
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
            while (!Initialized) {
                await Task.Delay(100);
            }
            lock (lockObj) {
                if (!_dockloaded) {
                    _dispatcher.Invoke(new Action(() => {
                        _dockmanager = (AvalonDock.DockingManager)o;

                        foreach (var item in Anchorables) {
                            item.IsVisible = false;
                        }

                        var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(_dockmanager);
                        serializer.LayoutSerializationCallback += (s, args) => {
                            args.Content = args.Content;
                        };

                        var profileId = profileService.ActiveProfile.Id;
                        var profilePath = Path.Combine(ProfileService.PROFILEFOLDER, $"{profileId}.dock.config");
                        if (System.IO.File.Exists(profilePath)) {
                            try {
                                serializer.Deserialize(profilePath);
                                _dockloaded = true;
                            } catch (Exception ex) {
                                Logger.Error("Failed to load AvalonDock Layout. Loading default Layout!", ex);
                                LoadDefaultLayout(serializer);
                            }
                        } else if (File.Exists(Path.Combine(NINA.Core.Utility.CoreUtil.APPLICATIONTEMPPATH, "avalondock.config"))) {
                            try {
                                Logger.Info("Migrating AvalonDock layout from old path");
                                serializer.Deserialize(Path.Combine(NINA.Core.Utility.CoreUtil.APPLICATIONTEMPPATH, "avalondock.config"));
                                _dockloaded = true;
                            } catch (Exception ex) {
                                Logger.Error("Failed to load AvalonDock Layout. Loading default Layout!", ex);
                                LoadDefaultLayout(serializer);
                            }
                        } else {
                            LoadDefaultLayout(serializer);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                return true;
            }
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