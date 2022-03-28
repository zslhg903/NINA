#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Locale;
using NINA.Profile.Interfaces;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Core.Utility.WindowService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Utility.SerialCommunication;
using NINA.Equipment.SDK.SwitchSDKs.PegasusAstro;
using NINA.Equipment.Interfaces;

namespace NINA.Equipment.Equipment.MySwitch.PegasusAstro {

    public class UltimatePowerBoxV2 : BaseINPC, ISwitchHub, IDisposable {
        private readonly IProfileService _profileService;
        private const string AUTO = "AUTO";
        private IPegasusDevice _sdk;
        private double _firmwareVersion = 1.3;

        public IPegasusDevice Sdk {
            get => _sdk ?? (Sdk = PegasusDevice.Instance);
            set => _sdk = value;
        }

        public UltimatePowerBoxV2(IProfileService profileService) {
            _profileService = profileService;
            PortName = profileService?.ActiveProfile?.SwitchSettings?.Upbv2PortName ?? AUTO;
        }

        public ReadOnlyCollection<string> PortNames => Sdk?.PortNames;

        private void LogAndNotify(ISerialCommand command, InvalidDeviceResponseException ex) {
            Logger.Error($"Invalid response from Ultimate Powerbox V2 on port {PortName}. " +
                         $"Command was: {command} Response was: {ex.Message}.");
            Notification.ShowError(Loc.Instance["LblUPBV2InvalidResponse"]);
        }

        private void HandlePortClosed(ISerialCommand command, SerialPortClosedException ex) {
            Logger.Error($"Serial port was closed. Command was: {command} Exception: {ex.InnerException}.");
            Notification.ShowError(Loc.Instance["LblUPBV2InvalidResponse"]);
            Disconnect();
            RaiseAllPropertiesChanged();
        }

        public string PortName {
            get => _profileService.ActiveProfile.SwitchSettings.Upbv2PortName;
            set {
                _profileService.ActiveProfile.SwitchSettings.Upbv2PortName = value;
                RaisePropertyChanged();
            }
        }

        public bool HasSetupDialog => !Connected;
        public string Id => "21f52eee-842b-42f3-92ab-901952d07718";
        public string Name => "Ultimate Powerbox V2";
        public string Category => "Pegasus Astro";

        private bool _connected;

        public bool Connected {
            get => _connected;
            private set {
                _connected = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasSetupDialog));
            }
        }

        private string _description;

        public string Description {
            get => _description;
            set {
                _description = value;
                RaisePropertyChanged();
            }
        }

        public string DriverInfo => "Serial driver for devices with firmware >= v1.3 (July 2019)";
        public string DriverVersion => "1.0";

        public async Task<bool> Connect(CancellationToken token) {
            if (!Sdk.InitializeSerialPort(PortName, this)) return false;
            if (Connected) return true;
            return await Task.Run(async () => {
                var statusCommand = new StatusCommand();
                try {
                    _ = await Sdk.SendCommand<StatusResponse>(statusCommand);
                    Connected = true;
                    Description = $"Ultimate Powerbox V2 on port {PortName}. Firmware version: ";
                } catch (InvalidDeviceResponseException ex) {
                    LogAndNotify(statusCommand, ex);
                    Sdk.Dispose(this);
                    Connected = false;
                } catch (SerialPortClosedException ex) {
                    HandlePortClosed(statusCommand, ex);
                    Connected = false;
                }
                if (!Connected) return false;

                var fwCommand = new FirmwareVersionCommand();
                try {
                    var response = await Sdk.SendCommand<FirmwareVersionResponse>(fwCommand);
                    Description += $"{response.FirmwareVersion}";
                    _firmwareVersion = response.FirmwareVersion;
                } catch (InvalidDeviceResponseException ex) {
                    LogAndNotify(fwCommand, ex);
                    Description += Loc.Instance["LblNoValidFirmwareVersion"];
                } catch (SerialPortClosedException ex) {
                    HandlePortClosed(fwCommand, ex);
                }

                var powerSwitch = new PegasusAstroPowerSwitch(0) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2PowerName1,
                    FirmwareVersion = _firmwareVersion
                };
                PowerSwitches.Add(powerSwitch);
                Switches.Add(powerSwitch);
                powerSwitch = new PegasusAstroPowerSwitch(1) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2PowerName2,
                    FirmwareVersion = _firmwareVersion
                };
                PowerSwitches.Add(powerSwitch);
                Switches.Add(powerSwitch);
                powerSwitch = new PegasusAstroPowerSwitch(2) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2PowerName3,
                    FirmwareVersion = _firmwareVersion
                };
                PowerSwitches.Add(powerSwitch);
                Switches.Add(powerSwitch);
                powerSwitch = new PegasusAstroPowerSwitch(3) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2PowerName4,
                    FirmwareVersion = _firmwareVersion
                };
                PowerSwitches.Add(powerSwitch);
                Switches.Add(powerSwitch);

                var variablePowerSwitch = new VariablePowerSwitch {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2PowerName5,
                    FirmwareVersion = _firmwareVersion
                };
                PowerSwitches.Add(variablePowerSwitch);
                Switches.Add(variablePowerSwitch);

                var usbSwitch = new PegasusAstroUsbSwitch(0) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2UsbName1,
                    FirmwareVersion = _firmwareVersion
                };
                UsbSwitches.Add(usbSwitch);
                Switches.Add(usbSwitch);
                usbSwitch = new PegasusAstroUsbSwitch(1) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2UsbName2,
                    FirmwareVersion = _firmwareVersion
                };
                UsbSwitches.Add(usbSwitch);
                Switches.Add(usbSwitch);
                usbSwitch = new PegasusAstroUsbSwitch(2) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2UsbName3,
                    FirmwareVersion = _firmwareVersion
                };
                UsbSwitches.Add(usbSwitch);
                Switches.Add(usbSwitch);
                usbSwitch = new PegasusAstroUsbSwitch(3) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2UsbName4,
                    FirmwareVersion = _firmwareVersion
                };
                UsbSwitches.Add(usbSwitch);
                Switches.Add(usbSwitch);
                usbSwitch = new PegasusAstroUsbSwitch(4) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2UsbName5,
                    FirmwareVersion = _firmwareVersion
                };
                UsbSwitches.Add(usbSwitch);
                Switches.Add(usbSwitch);
                usbSwitch = new PegasusAstroUsbSwitch(5) {
                    Name = _profileService.ActiveProfile.SwitchSettings.Upbv2UsbName6,
                    FirmwareVersion = _firmwareVersion
                };
                UsbSwitches.Add(usbSwitch);
                Switches.Add(usbSwitch);

                for (short i = 0; i < 3; i++) {
                    var dewHeater = new DewHeater(i) { FirmwareVersion = _firmwareVersion };
                    DewHeaters.Add(dewHeater);
                    Switches.Add(dewHeater);
                }

                DataProvider = new DataProviderSwitch() { FirmwareVersion = _firmwareVersion };
                Switches.Add(DataProvider);

                Connected = true;
                RaiseAllPropertiesChanged();
                return Connected;
            }, token);
        }

        public void Disconnect() {
            if (!Connected) return;
            Connected = false;
            PowerSwitches.Clear();
            UsbSwitches.Clear();
            DewHeaters.Clear();
            DataProvider = null;
            Switches.Clear();
            Sdk.Dispose(this);
        }

        public IWindowService WindowService { get; set; } = new WindowService();

        public void SetupDialog() {
            WindowService.ShowDialog(this, "Ultimate Powerbox V2 Setup", System.Windows.ResizeMode.NoResize, System.Windows.WindowStyle.SingleBorderWindow);
        }

        public ICollection<ISwitch> PowerSwitches { get; } = new AsyncObservableCollection<ISwitch>();
        public ICollection<ISwitch> UsbSwitches { get; } = new AsyncObservableCollection<ISwitch>();
        public ICollection<ISwitch> DewHeaters { get; } = new AsyncObservableCollection<ISwitch>();
        public ICollection<ISwitch> Switches { get; } = new AsyncObservableCollection<ISwitch>();
        public ISwitch DataProvider { get; protected set; }

        public void Dispose() {
            if (Connected) Disconnect();
        }

        public IList<string> SupportedActions => new List<string>();

        public string Action(string actionName, string actionParameters) {
            throw new NotImplementedException();
        }

        public string SendCommandString(string command, bool raw) {
            throw new NotImplementedException();
        }

        public bool SendCommandBool(string command, bool raw) {
            throw new NotImplementedException();
        }

        public void SendCommandBlind(string command, bool raw) {
            throw new NotImplementedException();
        }
    }
}