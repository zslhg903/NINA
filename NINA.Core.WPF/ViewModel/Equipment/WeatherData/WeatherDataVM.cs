#region "copyright"

/*
    Copyright ? 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Equipment.MyWeatherData;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Core.Utility.Notification;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.Core.Locale;
using NINA.Core.Model;
using NINA.Core.MyMessageBox;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Equipment;
using Nito.AsyncEx;
using System.Linq;

namespace NINA.WPF.Base.ViewModel.Equipment.WeatherData {

    public class WeatherDataVM : DockableVM, IWeatherDataVM {

        public WeatherDataVM(IProfileService profileService, IWeatherDataMediator weatherDataMediator, IApplicationStatusMediator applicationStatusMediator, IDeviceDispatcher deviceDispatcher) : base(profileService) {
            Title = Loc.Instance["LblWeather"];
            ImageGeometry = (System.Windows.Media.GeometryGroup)System.Windows.Application.Current.Resources["CloudSVG"];

            this.weatherDataMediator = weatherDataMediator;
            this.weatherDataMediator.RegisterHandler(this);
            this.applicationStatusMediator = applicationStatusMediator;
            this.deviceDispatcher = deviceDispatcher;
            _ = Rescan();

            ChooseWeatherDataCommand = new AsyncCommand<bool>(() => Task.Run(ChooseWeatherData));
            CancelChooseWeatherDataCommand = new RelayCommand(CancelChooseWeatherData);
            DisconnectCommand = new AsyncCommand<bool>(() => Task.Run(DisconnectDiag));
            RefreshWeatherDataListCommand = new AsyncCommand<bool>(async o => { await Rescan(); return true; }, o => !WeatherDataInfo.Connected);

            updateTimer = new DeviceUpdateTimer(
                GetWeatherDataValues,
                UpdateWeatherDataValues,
                profileService.ActiveProfile.ApplicationSettings.DevicePollingInterval
            );

            profileService.ProfileChanged += async (object sender, EventArgs e) => {
                await Rescan();
            };
        }

        public async Task<IList<string>> Rescan() {
            return await Task.Run(() => {
                WeatherDataChooserVM.GetEquipment();
                return WeatherDataChooserVM.Devices.Select(x => x.Id).ToList();
            });
        }

        private CancellationTokenSource _cancelChooseWeatherDataSource;

        private readonly SemaphoreSlim ss = new SemaphoreSlim(1, 1);

        private async Task<bool> ChooseWeatherData() {
            await ss.WaitAsync();
            try {
                await Disconnect();
                if (updateTimer != null) {
                    await updateTimer.Stop();
                }

                if (WeatherDataChooserVM.SelectedDevice.Id == "No_Device") {
                    profileService.ActiveProfile.WeatherDataSettings.Id = WeatherDataChooserVM.SelectedDevice.Id;
                    return false;
                }

                applicationStatusMediator.StatusUpdate(
                    new ApplicationStatus() {
                        Source = Title,
                        Status = Loc.Instance["LblConnecting"]
                    }
                );

                var weatherdev = (IWeatherData)WeatherDataChooserVM.SelectedDevice;
                _cancelChooseWeatherDataSource?.Dispose();
                _cancelChooseWeatherDataSource = new CancellationTokenSource();
                if (weatherdev != null) {
                    try {
                        var connected = await weatherdev?.Connect(_cancelChooseWeatherDataSource.Token);
                        _cancelChooseWeatherDataSource.Token.ThrowIfCancellationRequested();
                        if (connected) {
                            WeatherData = weatherdev;

                            WeatherDataInfo = new WeatherDataInfo {
                                Connected = true,
                                Name = WeatherData.Name,
                                AveragePeriod = WeatherData.AveragePeriod,
                                CloudCover = WeatherData.CloudCover,
                                DewPoint = WeatherData.DewPoint,
                                Humidity = WeatherData.Humidity,
                                Pressure = WeatherData.Pressure,
                                RainRate = WeatherData.RainRate,
                                SkyBrightness = WeatherData.SkyBrightness,
                                SkyQuality = WeatherData.SkyQuality,
                                SkyTemperature = WeatherData.SkyTemperature,
                                StarFWHM = WeatherData.StarFWHM,
                                Temperature = WeatherData.Temperature,
                                WindDirection = WeatherData.WindDirection,
                                WindGust = WeatherData.WindGust,
                                WindSpeed = WeatherData.WindSpeed,
                                Description = WeatherData.Description,
                                DriverInfo = WeatherData.DriverInfo,
                                DriverVersion = WeatherData.DriverVersion,
                                DeviceId = WeatherData.Id
                            };

                            Notification.ShowSuccess(Loc.Instance["LblWeatherConnected"]);

                            updateTimer.Start();

                            profileService.ActiveProfile.WeatherDataSettings.Id = WeatherData.Id;

                            Logger.Info($"Successfully connected Weather Device. Id: {weatherdev.Id} Name: {weatherdev.Name} Driver Version: {weatherdev.DriverVersion}");

                            return true;
                        } else {
                            WeatherDataInfo.Connected = false;
                            WeatherData = null;
                            return false;
                        }
                    } catch (OperationCanceledException) {
                        if (WeatherDataInfo.Connected) { await Disconnect(); }
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

        private void CancelChooseWeatherData(object o) {
            _cancelChooseWeatherDataSource?.Cancel();
        }

        private Dictionary<string, object> GetWeatherDataValues() {
            Dictionary<string, object> weatherDataValues = new Dictionary<string, object> {
                { nameof(WeatherDataInfo.Connected), _weatherdev?.Connected ?? false },
                { nameof(WeatherDataInfo.AveragePeriod), _weatherdev?.AveragePeriod ?? double.NaN },
                { nameof(WeatherDataInfo.CloudCover), _weatherdev?.CloudCover ?? double.NaN },
                { nameof(WeatherDataInfo.DewPoint), _weatherdev?.DewPoint ?? double.NaN },
                { nameof(WeatherDataInfo.Humidity), _weatherdev?.Humidity ?? double.NaN },
                { nameof(WeatherDataInfo.Pressure), _weatherdev?.Pressure ?? double.NaN },
                { nameof(WeatherDataInfo.RainRate), _weatherdev?.RainRate ?? double.NaN },
                { nameof(WeatherDataInfo.SkyBrightness), _weatherdev?.SkyBrightness ?? double.NaN },
                { nameof(WeatherDataInfo.SkyQuality), _weatherdev?.SkyQuality ?? double.NaN },
                { nameof(WeatherDataInfo.SkyTemperature), _weatherdev?.SkyTemperature ?? double.NaN },
                { nameof(WeatherDataInfo.StarFWHM), _weatherdev?.StarFWHM ?? double.NaN },
                { nameof(WeatherDataInfo.Temperature), _weatherdev?.Temperature ?? double.NaN },
                { nameof(WeatherDataInfo.WindDirection), _weatherdev?.WindDirection ?? double.NaN },
                { nameof(WeatherDataInfo.WindGust), _weatherdev?.WindGust ?? double.NaN },
                { nameof(WeatherDataInfo.WindSpeed), _weatherdev?.WindSpeed ?? double.NaN }
            };

            return weatherDataValues;
        }

        private void UpdateWeatherDataValues(Dictionary<string, object> weatherDataValues) {
            object o;

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.Connected), out o);
            WeatherDataInfo.Connected = (bool)(o ?? false);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.AveragePeriod), out o);
            WeatherDataInfo.AveragePeriod = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.CloudCover), out o);
            WeatherDataInfo.CloudCover = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.DewPoint), out o);
            WeatherDataInfo.DewPoint = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.Humidity), out o);
            WeatherDataInfo.Humidity = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.Pressure), out o);
            WeatherDataInfo.Pressure = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.RainRate), out o);
            WeatherDataInfo.RainRate = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.SkyBrightness), out o);
            WeatherDataInfo.SkyBrightness = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.SkyQuality), out o);
            WeatherDataInfo.SkyQuality = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.SkyTemperature), out o);
            WeatherDataInfo.SkyTemperature = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.StarFWHM), out o);
            WeatherDataInfo.StarFWHM = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.Temperature), out o);
            WeatherDataInfo.Temperature = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.WindDirection), out o);
            WeatherDataInfo.WindDirection = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.WindGust), out o);
            WeatherDataInfo.WindGust = (double)(o ?? double.NaN);

            weatherDataValues.TryGetValue(nameof(WeatherDataInfo.WindSpeed), out o);
            WeatherDataInfo.WindSpeed = (double)(o ?? double.NaN);

            BroadcastWeatherDataInfo();
        }

        private WeatherDataInfo weatherDataInfo;

        public WeatherDataInfo WeatherDataInfo {
            get {
                if (weatherDataInfo == null) {
                    weatherDataInfo = DeviceInfo.CreateDefaultInstance<WeatherDataInfo>();
                }
                return weatherDataInfo;
            }
            set {
                weatherDataInfo = value;
                RaisePropertyChanged();
            }
        }

        public WeatherDataInfo GetDeviceInfo() {
            return WeatherDataInfo;
        }

        private void BroadcastWeatherDataInfo() {
            weatherDataMediator.Broadcast(WeatherDataInfo);
        }

        public Task<bool> Connect() {
            return ChooseWeatherData();
        }

        private async Task<bool> DisconnectDiag() {
            var diag = MyMessageBox.Show(Loc.Instance["LblWeatherDisconnect"], "", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxResult.Cancel);
            if (diag == System.Windows.MessageBoxResult.OK) {
                await Disconnect();
            }
            return true;
        }

        public async Task Disconnect() {
            if (WeatherData != null) { Logger.Info("Disconnected Weather Device"); }
            if (updateTimer != null) {
                await updateTimer.Stop();
            }
            WeatherData?.Disconnect();
            WeatherData = null;
            WeatherDataInfo = DeviceInfo.CreateDefaultInstance<WeatherDataInfo>();
            BroadcastWeatherDataInfo();
            RaisePropertyChanged(nameof(WeatherData));
        }

        private IWeatherData _weatherdev;

        public IWeatherData WeatherData {
            get {
                return _weatherdev;
            }
            private set {
                _weatherdev = value;
                RaisePropertyChanged();
            }
        }

        private WeatherDataChooserVM _weatherDataChooserVM;

        public WeatherDataChooserVM WeatherDataChooserVM {
            get {
                if (_weatherDataChooserVM == null) {
                    _weatherDataChooserVM = new WeatherDataChooserVM(profileService, deviceDispatcher);
                }
                return _weatherDataChooserVM;
            }
            set => _weatherDataChooserVM = value;
        }

        private DeviceUpdateTimer updateTimer;
        private IWeatherDataMediator weatherDataMediator;
        private IApplicationStatusMediator applicationStatusMediator;
        private IDeviceDispatcher deviceDispatcher;
        public IAsyncCommand ChooseWeatherDataCommand { get; private set; }
        public ICommand RefreshWeatherDataListCommand { get; private set; }
        public ICommand CancelChooseWeatherDataCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }
    }
}