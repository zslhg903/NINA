#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Equipment.Utility;
using NINA.Profile.Interfaces;
using System.Collections.Generic;

namespace NINA.WPF.Base.ViewModel.Equipment.FlatDevice {

    public class FlatDeviceChooserVM : DeviceChooserVM {
        private readonly IDeviceFactory deviceFactory;

        public FlatDeviceChooserVM(IProfileService profileService, IDeviceFactory deviceFactory) : base(profileService) {
            this.deviceFactory = deviceFactory;
        }

        public override void GetEquipment() {
            lock (lockObj) {
                var devices = new List<IDevice>();

                foreach (var device in deviceFactory.GetDevices()) {
                    devices.Add(device);
                }

                DetermineSelectedDevice(devices, profileService.ActiveProfile.FlatDeviceSettings.Id);
            }
        }
    }
}