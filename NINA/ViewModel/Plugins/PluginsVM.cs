﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Locale;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Interfaces;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Plugin.ManifestDefinition;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NINA.ViewModel.Plugins {

    public class PluginsVM : BaseVM, IPluginsVM {
        private IPluginLoader pluginProvider;

        public PluginsVM(IPluginLoader pluginProvider, IProfileService profileService) : base(profileService) {
            this.pluginProvider = pluginProvider;

            this.repositories = new List<string>();
            this.repositories.Add("https://nighttime-imaging.eu/wp-json/nina/v1");
            FetchPluginsCommand = new AsyncCommand<bool>((object o) => {
                if (o?.ToString() == "Initial") {
                    if (firstTime) {
                        firstTime = false;
                        return FetchPlugins();
                    } else {
                        return Task.FromResult(false);
                    }
                } else {
                    return FetchPlugins();
                }
            });
            UpdatePluginCommand = new AsyncCommand<bool>(() => InstallPlugin(true));
            InstallPluginCommand = new AsyncCommand<bool>(() => InstallPlugin(false));
            CancelInstallPluginCommand = new RelayCommand((object o) => { try { installCts?.Cancel(); } catch (Exception) { } });
            CancelFetchPluginsCommand = new RelayCommand((object o) => { try { fetchCts?.Cancel(); } catch (Exception) { } });
            UninstallPluginCommand = new AsyncCommand<bool>(UninstallPlugin);
            RestartCommand = new RelayCommand(Restart);

            _ = Task.Run(async () => {
                await pluginProvider.Load();
                InstalledPlugins = pluginProvider.Plugins;
                SelectedInstalledPlugin = InstalledPlugins.FirstOrDefault();
                RaisePropertyChanged(nameof(AvailablePlugins));
                RaisePropertyChanged(nameof(SelectedAvailablePlugin));

                Initialized = true;

                if (InstalledPlugins?.Count > 0) {
                    await FetchPluginsCommand.ExecuteAsync("Initial");
                }
            });
        }

        private void Restart(object obj) {
            profileService.Release();
            Application.Current.Shutdown();
            System.Windows.Forms.Application.Restart();
        }

        private object lockObj = new object();
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

        private Task<bool> UninstallPlugin() {
            return Task.Run(() => {
                var manifest = SelectedInstalledPlugin.Key;
                var installer = new PluginInstaller();

                try {
                    installer.Uninstall(manifest);
                    InstalledPlugins.Remove(manifest);
                    InstalledPlugins = new Dictionary<IPluginManifest, bool>(InstalledPlugins);
                    SelectedInstalledPlugin = InstalledPlugins.FirstOrDefault();

                    var available = AvailablePlugins?.FirstOrDefault(x => x.Identifier == manifest.Identifier);
                    if (available != null) {
                        available.State = PluginState.NotInstalled;
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                    Notification.ShowError(string.Format(Loc.Instance["LblPluginInstallationFailed"], manifest.Name));
                }
                return true;
            });
        }

        private CancellationTokenSource installCts;

        private Task<bool> InstallPlugin(bool update) {
            return Task.Run(async () => {
                using (installCts = new CancellationTokenSource()) {
                    var installer = new PluginInstaller();
                    var manifest = SelectedAvailablePlugin;

                    try {
                        await installer.Install(manifest, update, installCts.Token);
                        manifest.State = PluginState.InstalledAndRequiresRestart;
                    } catch (OperationCanceledException) {
                    } catch (Exception ex) {
                        Logger.Error(ex);
                        Notification.ShowError(string.Format(Loc.Instance["LblPluginInstallationFailed"], manifest.Name));
                    }
                    return true;
                }
            });
        }

        private bool firstTime = true;

        private KeyValuePair<IPluginManifest, bool> selectedInstalledPlugin;

        public KeyValuePair<IPluginManifest, bool> SelectedInstalledPlugin {
            get => selectedInstalledPlugin;
            set {
                selectedInstalledPlugin = value;
                RaisePropertyChanged();
            }
        }

        private IDictionary<IPluginManifest, bool> installedPlugins;

        public IDictionary<IPluginManifest, bool> InstalledPlugins {
            get => installedPlugins;
            set {
                installedPlugins = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand FetchPluginsCommand { get; }
        public IAsyncCommand InstallPluginCommand { get; }
        public ICommand CancelFetchPluginsCommand { get; }
        public ICommand CancelInstallPluginCommand { get; }
        public ICommand RestartCommand { get; }

        public IAsyncCommand UpdatePluginCommand { get; }
        public IAsyncCommand UninstallPluginCommand { get; }

        private ExtendedPluginManifest selectedAvailablePlugin;

        public ExtendedPluginManifest SelectedAvailablePlugin {
            get => selectedAvailablePlugin;
            set {
                selectedAvailablePlugin = value;
                RaisePropertyChanged();
            }
        }

        private int availablePluginUpdateCount;

        public int AvailablePluginUpdateCount {
            get => availablePluginUpdateCount;
            private set {
                availablePluginUpdateCount = value;
                RaisePropertyChanged();
            }
        }

        private IList<ExtendedPluginManifest> availablePlugins;

        public IList<ExtendedPluginManifest> AvailablePlugins {
            get => availablePlugins;
            set {
                availablePlugins = value;
                RaisePropertyChanged();
            }
        }

        private IList<string> repositories;

        private IList<string> Repositories {
            get => repositories;
            set {
                repositories = value;
                RaisePropertyChanged();
            }
        }

        private CancellationTokenSource fetchCts;

        public async Task<bool> FetchPlugins() {
            try {
                //todo progress
                var progress = new Progress<ApplicationStatus>(p => { });
                var onlinePlugins = new List<IPluginManifest>();
                using (fetchCts = new CancellationTokenSource()) {
                    foreach (var repo in repositories) {
                        try {
                            var fetcher = new PluginFetcher(repo);
                            onlinePlugins.AddRange(await fetcher.RequestAll(new PluginVersion(CoreUtil.Version), progress, fetchCts.Token));
                        } catch (Exception ex) {
                            var host = new Uri(repo).Host;
                            Logger.Error($"Failed to fetch plugins from {host}", ex);
                        }
                    }
                }

                //now grab the latest applicaple version of the plugins per id
                Dictionary<string, ExtendedPluginManifest> onlinePluginDict = onlinePlugins
                    .GroupBy(x => x.Identifier)
                    .Select(grp => new ExtendedPluginManifest(grp
                        .Aggregate((max, cur) => (max == null ||
                            (cur.Version.Major > max.Version.Major)
                            && (cur.Version.Minor > max.Version.Minor)
                            && (cur.Version.Patch > max.Version.Patch)
                            && (cur.Version.Build > max.Version.Build)) ? max : cur))).ToDictionary(x => x.Identifier);

                //var extended = new List<ExtendedPluginManifest>();
                foreach (var kv in InstalledPlugins) {
                    var p = kv.Key;

                    if (onlinePluginDict.TryGetValue(p.Identifier, out var common)) {
                        if (PluginVersionCompare(common.Version, p.Version) > 0) {
                            onlinePluginDict[p.Identifier].State = PluginState.UpdateAvailable;
                        } else {
                            onlinePluginDict[p.Identifier].State = PluginState.Installed;
                        }
                    }
                }

                var list = onlinePluginDict.Values.OrderBy(x => (int)x.State).ThenBy(x => x.Name).ToList();
                ExtendedPluginManifest m = null;
                if (list.Count > 0) {
                    if (SelectedAvailablePlugin != null) {
                        m = list.FirstOrDefault(x => x.Identifier == SelectedAvailablePlugin.Identifier);
                    }
                    if (m == null) { m = list.FirstOrDefault(); }
                }
                AvailablePlugins = list;
                SelectedAvailablePlugin = m;

                AvailablePluginUpdateCount = AvailablePlugins.Where(x => x.State == PluginState.UpdateAvailable).Count();
            } catch (OperationCanceledException) {
            } catch (Exception ex) {
                Logger.Error(ex);
            }
            return true;
        }

        public int PluginVersionCompare(IPluginVersion v1, IPluginVersion v2) {
            if (v2 == null)
                return 1;

            if (v1.Major != v2.Major)
                if (v1.Major > v2.Major)
                    return 1;
                else
                    return -1;

            if (v1.Minor != v2.Minor)
                if (v1.Minor > v2.Minor)
                    return 1;
                else
                    return -1;

            if (v1.Patch != v2.Patch)
                if (v1.Patch > v2.Patch)
                    return 1;
                else
                    return -1;

            if (v1.Build != v2.Build)
                if (v1.Build > v2.Build)
                    return 1;
                else
                    return -1;

            return 0;
        }
    }
}