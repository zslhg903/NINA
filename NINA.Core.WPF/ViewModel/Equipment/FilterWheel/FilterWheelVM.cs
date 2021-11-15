#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Equipment.MyFilterWheel;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Core.Utility.Notification;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NINA.Core.Model.Equipment;
using NINA.Core.Model;
using NINA.Core.Locale;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.Core.MyMessageBox;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Equipment;
using Nito.AsyncEx;

namespace NINA.WPF.Base.ViewModel.Equipment.FilterWheel {

    public class FilterWheelVM : DockableVM, IFilterWheelVM {

        public FilterWheelVM(
            IProfileService profileService,
            IFilterWheelMediator filterWheelMediator,
            IFocuserMediator focuserMediator,
            IGuiderMediator guiderMediator,
            IDeviceChooserVM filterWheelChooserVM,
            IApplicationStatusMediator applicationStatusMediator) : base(profileService) {
            Title = Loc.Instance["LblFilterWheel"];
            ImageGeometry = (System.Windows.Media.GeometryGroup)System.Windows.Application.Current.Resources["FWSVG"];

            FilterWheelChooserVM = filterWheelChooserVM;
            this.filterWheelMediator = filterWheelMediator;
            this.filterWheelMediator.RegisterHandler(this);
            _ = Rescan();

            this.focuserMediator = focuserMediator;
            this.guiderMediator = guiderMediator;
            this.applicationStatusMediator = applicationStatusMediator;

            ChooseFWCommand = new AsyncCommand<bool>(() => ChooseFW());
            CancelChooseFWCommand = new RelayCommand(CancelChooseFW);
            DisconnectCommand = new AsyncCommand<bool>(() => DisconnectFW());
            RefreshFWListCommand = new AsyncCommand<bool>(async o => { await Rescan(); return true; }, o => !(FW?.Connected == true));
            ChangeFilterCommand = new AsyncCommand<bool>(async () => {
                _changeFilterCancellationSource?.Dispose();
                _changeFilterCancellationSource = new CancellationTokenSource();
                await ChangeFilter(TargetFilter, _changeFilterCancellationSource.Token, new Progress<ApplicationStatus>(x => { x.Source = this.Title; applicationStatusMediator.StatusUpdate(x); }));
                return true;
            }, (object o) => FilterWheelInfo.Connected && !FilterWheelInfo.IsMoving);

            profileService.ProfileChanged += async (object sender, EventArgs e) => {
                await Rescan();
            };
        }

        public async Task<IList<string>> Rescan() {
            return await Task.Run(() => {
                FilterWheelChooserVM.GetEquipment();
                return FilterWheelChooserVM.Devices.Select(x => x.Id).ToList();
            });
        }

        private CancellationTokenSource _changeFilterCancellationSource;

        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<FilterInfo> ChangeFilter(FilterInfo inputFilter, CancellationToken token = new CancellationToken(), IProgress<ApplicationStatus> progress = null) {
            //Lock access so only one instance can change the filter
            await semaphoreSlim.WaitAsync(token);
            try {
                if (FW?.Connected == true) {
                    var prevFilter = profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters.Where(x => x.Position == FilterWheelInfo.SelectedFilter?.Position).FirstOrDefault();
                    if (inputFilter == null) {
                        return prevFilter;
                    }

                    var filter = FW.Filters.Where((x) => x.Position == inputFilter.Position).FirstOrDefault();
                    if (filter == null) {
                        Logger.Warning($"Filter not found for position {inputFilter.Position}");
                        Notification.ShowWarning(string.Format(Loc.Instance["LblFilterNotFoundForPosition"], inputFilter.Position));
                        return null;
                    }

                    if (FW?.Position != filter.Position) {
                        Logger.Info($"Moving to Filter {filter.Name} at Position {filter.Position}");
                        FilterWheelInfo.IsMoving = true;
                        Task changeFocus = null;
                        bool activeGuidingStopped = false;
                        if (profileService.ActiveProfile.FocuserSettings.UseFilterWheelOffsets) {
                            if (prevFilter != null) {
                                var newFilter = profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters.Where(x => x.Position == filter.Position).FirstOrDefault();
                                if (newFilter != null) {
                                    int offset = newFilter.FocusOffset - prevFilter.FocusOffset;
                                    if (offset != 0) {
                                        if (profileService.ActiveProfile.FocuserSettings.AutoFocusDisableGuiding) {
                                            // AutoFocusDisableGuiding is typically used with OAGs which are affected when focus changes. We repurpose that setting here for regular
                                            // filter changes that can happen in the middle of a sequence, but only if the focus is actually changing

                                            // This codepath is also hit during auto focus, but guiding should already be disabled by the time it gets here. StopGuiding returns false
                                            // if it isn't currently guiding, so this indicates FilterWheelVM is "responsible" for resuming guiding afterwards
                                            activeGuidingStopped = await this.guiderMediator.StopGuiding(token);
                                            if (activeGuidingStopped) {
                                                Logger.Info($"Disabled guiding during filter change that caused focuser movement");
                                            }
                                        }

                                        changeFocus = focuserMediator.MoveFocuserRelative(offset, token);
                                    }
                                }
                            }
                        }

                        FW.Position = filter.Position;
                        var changeFilter = Task.Run(async () => {
                            while (FW.Position == -1) {
                                await Task.Delay(1000, token);
                                token.ThrowIfCancellationRequested();
                            }
                        });
                        progress?.Report(new ApplicationStatus() { Status = Loc.Instance["LblSwitchingFilter"] });

                        if (changeFocus != null) {
                            await changeFocus;
                        }

                        await changeFilter;
                        if (activeGuidingStopped) {
                            var resumedGuiding = await this.guiderMediator.StartGuiding(false, progress, token);
                            if (resumedGuiding) {
                                Logger.Info($"Resumed guiding after filter change");
                            } else {
                                Logger.Error($"Failed to resume guiding after filter change");
                                Notification.ShowError(Loc.Instance["LblFilterChangeResumeGuidingFailed"]);
                            }
                        }
                    }
                    FilterWheelInfo.SelectedFilter = filter;
                } else {
                    await Disconnect();
                }
            } finally {
                progress?.Report(new ApplicationStatus() { Status = string.Empty });

                BroadcastFilterWheelInfo();
                //unlock access
                FilterWheelInfo.IsMoving = false;
                semaphoreSlim.Release();
            }
            return FilterWheelInfo.SelectedFilter;
        }

        private IFilterWheel _fW;

        public IFilterWheel FW {
            get {
                return _fW;
            }
            private set {
                _fW = value;
                RaisePropertyChanged();
            }
        }

        private FilterInfo targetFilter;

        public FilterInfo TargetFilter {
            get {
                return targetFilter;
            }
            set {
                targetFilter = value;
                RaisePropertyChanged();
            }
        }

        private readonly SemaphoreSlim ss = new SemaphoreSlim(1, 1);

        private async Task<bool> ChooseFW() {
            await ss.WaitAsync();
            try {
                await Disconnect();

                if (FilterWheelChooserVM.SelectedDevice.Id == "No_Device") {
                    profileService.ActiveProfile.FilterWheelSettings.Id = FilterWheelChooserVM.SelectedDevice.Id;
                    return false;
                }

                applicationStatusMediator.StatusUpdate(
                    new ApplicationStatus() {
                        Source = Title,
                        Status = Loc.Instance["LblConnecting"]
                    }
                );

                var fW = (IFilterWheel)FilterWheelChooserVM.SelectedDevice;
                _cancelChooseFilterWheelSource?.Dispose();
                _cancelChooseFilterWheelSource = new CancellationTokenSource();
                if (fW != null) {
                    try {
                        var connected = await fW?.Connect(_cancelChooseFilterWheelSource.Token);
                        _cancelChooseFilterWheelSource.Token.ThrowIfCancellationRequested();
                        if (connected) {
                            this.FW = fW;

                            FilterWheelInfo = new FilterWheelInfo {
                                Connected = true,
                                IsMoving = false,
                                Name = FW.Name,
                                Description = FW.Description,
                                DriverInfo = FW.DriverInfo,
                                DriverVersion = FW.DriverVersion,
                                DeviceId = FW.Id
                            };

                            Notification.ShowSuccess(Loc.Instance["LblFilterwheelConnected"]);
                            profileService.ActiveProfile.FilterWheelSettings.Id = FW.Id;
                            if (FW.Position > -1) {
                                FilterWheelInfo.SelectedFilter = FW.Filters[FW.Position];
                            }

                            // Auto import filters to profile, when profile does not have any filters set yet.
                            if (FW.Filters.Count > 0 && profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters.Count == 0) {
                                var l = FW.Filters.OrderBy(x => x.Position);
                                foreach (var filter in l) {
                                    profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters.Add(filter);
                                }
                            }

                            TargetFilter = FilterWheelInfo.SelectedFilter;

                            BroadcastFilterWheelInfo();

                            Logger.Info($"Successfully connected Filter Wheel. Id: {FW.Id} Name: {FW.Name} Driver Version: {FW.DriverVersion}");

                            return true;
                        } else {
                            this.FW = null;
                            return false;
                        }
                    } catch (OperationCanceledException) {
                        if (fW?.Connected == true) { await Disconnect(); }
                        return false;
                    } catch (Exception e) {
                        Logger.Error($"Failed to connect to filter wheel: {e}");
                        Notification.ShowError(e.Message);
                        return false;
                    }
                } else {
                    return false;
                }
            } finally {
                ss.Release();
                applicationStatusMediator.StatusUpdate(
                    new ApplicationStatus() {
                        Source = Title,
                        Status = string.Empty
                    }
                );
            }
        }

        private void CancelChooseFW(object o) {
            _cancelChooseFilterWheelSource?.Cancel();
        }

        private CancellationTokenSource _cancelChooseFilterWheelSource;

        private async Task<bool> DisconnectFW() {
            var diag = MyMessageBox.Show(Loc.Instance["LblDisconnectFilterWheel"], "", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxResult.Cancel);
            if (diag == System.Windows.MessageBoxResult.OK) {
                await Disconnect();
            }
            return true;
        }

        public Task Disconnect() {
            if (FW != null) {
                _changeFilterCancellationSource?.Cancel();
                FW.Disconnect();
                FW = null;
                FilterWheelInfo = DeviceInfo.CreateDefaultInstance<FilterWheelInfo>();
                RaisePropertyChanged(nameof(FW));
                BroadcastFilterWheelInfo();
                Logger.Info("Disconnected Filter Wheel");
            }
            return Task.CompletedTask;
        }

        private readonly FilterWheelChooserVM _filterWheelChooserVM;
        private readonly IFilterWheelMediator filterWheelMediator;
        private readonly IFocuserMediator focuserMediator;
        private readonly IGuiderMediator guiderMediator;
        private readonly IApplicationStatusMediator applicationStatusMediator;
        private FilterWheelInfo filterWheelInfo;

        public FilterWheelInfo FilterWheelInfo {
            get {
                if (filterWheelInfo == null) {
                    filterWheelInfo = DeviceInfo.CreateDefaultInstance<FilterWheelInfo>();
                }
                return filterWheelInfo;
            }
            set {
                filterWheelInfo = value;
                RaisePropertyChanged();
            }
        }

        private void BroadcastFilterWheelInfo() {
            this.filterWheelMediator.Broadcast(FilterWheelInfo);
        }

        public ICollection<FilterInfo> GetAllFilters() {
            if (FilterWheelInfo.Connected) {
                return FW?.Filters;
            } else {
                return null;
            }
        }

        public Task<bool> Connect() {
            return ChooseFW();
        }

        public FilterWheelInfo GetDeviceInfo() {
            return FilterWheelInfo;
        }

        public IDeviceChooserVM FilterWheelChooserVM { get; set; }
        public IAsyncCommand ChooseFWCommand { get; private set; }
        public ICommand CancelChooseFWCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand RefreshFWListCommand { get; private set; }
        public IAsyncCommand ChangeFilterCommand { get; private set; }
    }
}