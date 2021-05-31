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
using NINA.Core.MyMessageBox;
using NINA.Core.Utility;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.Utility;
using NINA.ViewModel.Interfaces;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NINA {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private ProfileService _profileService;
        private IMainWindowVM _mainWindowViewModel;

        protected override void OnStartup(StartupEventArgs e) {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            if (NINA.Properties.Settings.Default.FamilyTypeface == null) {
                NINA.Properties.Settings.Default.FamilyTypeface = NINA.Properties.Settings.Default.ApplicationFontFamily.FamilyTypefaces.First(x => x.AdjustedFaceNames.First().Value == "Regular");
            }

            if (NINA.Properties.Settings.Default.UpdateSettings) {
                NINA.Properties.Settings.Default.Upgrade();
                NINA.Properties.Settings.Default.UpdateSettings = false;
                NINA.Properties.Settings.Default.Save();
            }

            _profileService =
                //TODO: Eliminate Smell by reversing direction of this dependency
                (ProfileService)Current.Resources["ProfileService"];

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

            var startWithProfileId = e
               .Args
               .SkipWhile(x => !x.Equals("/profileid", StringComparison.OrdinalIgnoreCase))
               .Skip(1)
               .FirstOrDefault();
            _profileService = (ProfileService)Current.Resources["ProfileService"];

            if (!_profileService.TryLoad(startWithProfileId)) {
                ProfileService.ActivateInstanceOfNinaReferencingProfile(startWithProfileId);
                Shutdown();
                return;
            }
            this.RefreshJumpList(_profileService);

            _profileService.CreateWatcher();

            Logger.SetLogLevel(_profileService.ActiveProfile.ApplicationSettings.LogLevel);

            _mainWindowViewModel = CompositionRoot.Compose(_profileService);
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.GotFocusEvent,
                new RoutedEventHandler(TextBox_GotFocus));

            var mainWindow = new MainWindow();
            mainWindow.DataContext = _mainWindowViewModel;
            mainWindow.Show();
            ProfileService.ActivateInstanceWatcher(_profileService, mainWindow);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            (sender as TextBox).SelectAll();
        }

        protected override void OnExit(ExitEventArgs e) {
            this.RefreshJumpList(_profileService);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            if (e.Exception.InnerException != null) {
                var message = $"{e.Exception.Message}{Environment.NewLine}{e.Exception.StackTrace}{Environment.NewLine}Inner Exception: {Environment.NewLine}{e.Exception.InnerException}{e.Exception.StackTrace}";
                Logger.Error(message);
            } else {
                Logger.Error(e.Exception);
            }

            if (Current != null) {
                var result = MyMessageBox.Show(
                    Loc.Instance["LblApplicationInBreakMode"],
                    Loc.Instance["LblUnhandledException"],
                    MessageBoxButton.YesNo,
                    MessageBoxResult.No);
                if (result == MessageBoxResult.Yes) {
                    e.Handled = true;
                } else {
                    try {
                        if (_mainWindowViewModel != null) {
                            if (_mainWindowViewModel.ApplicationDeviceConnectionVM != null) {
                                AsyncContext.Run(_mainWindowViewModel.ApplicationDeviceConnectionVM.DisconnectEquipment);
                            }
                        }
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }
                    e.Handled = true;
                    Current.Shutdown();
                }
            }
        }
    }
}