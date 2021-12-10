#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using ASCOM.DeviceInterface;
using ASCOM.DriverAccess;
using NINA.Core.Locale;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NINA.Equipment.Equipment.MySwitch.Ascom {

    internal class AscomSwitchHub : AscomDevice<ISwitchV2, Switch>, ISwitchHub, IDisposable {
        public AscomSwitchHub(string id, string name, IDeviceDispatcher deviceDispatcher) : base(id, name, deviceDispatcher, DeviceDispatcherType.Switch) {
        }

        public ICollection<ISwitch> Switches { get; private set; } = new AsyncObservableCollection<ISwitch>();

        protected override string ConnectionLostMessage => Loc.Instance["LblSwitchConnectionLost"];

        private async Task ScanForSwitches() {
            Logger.Trace("Scanning for Ascom Switches");
            var numberOfSwitches = device.MaxSwitch;
            for (short i = 0; i < numberOfSwitches; i++) {
                try {
                    var canWrite = device.CanWrite(i);

                    if (canWrite) {
                        Logger.Trace($"Writable Switch found for index {i}");
                        var s = new AscomWritableSwitch(device, i);
                        Switches.Add(s);
                    } else {
                        Logger.Trace($"Readable Switch found for index {i}");
                        var s = new AscomSwitch(device, i);
                        Switches.Add(s);
                    }
                } catch (ASCOM.MethodNotImplementedException) {
                    //ISwitchV1 Fallbacks
                    try {
                        var s = new AscomWritableV1Switch(device, i);
                        s.TargetValue = s.Value;
                        await s.SetValue();
                        Switches.Add(s);
                    } catch (Exception) {
                        var s = new AscomV1Switch(device, i);
                        Switches.Add(s);
                    }
                }
            }
        }

        protected override async Task PostConnect() {
            await ScanForSwitches();
        }

        protected override void PostDisconnect() {
            Switches.Clear();
        }

        protected override Switch GetInstance(string id) {
            return DeviceDispatcher.Invoke(DeviceDispatcherType, () => new Switch(id));
        }
    }
}