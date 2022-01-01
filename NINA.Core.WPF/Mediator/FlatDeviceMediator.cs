#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Threading;
using System.Threading.Tasks;
using NINA.Equipment.Equipment.MyFlatDevice;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;

namespace NINA.WPF.Base.Mediator {

    public class FlatDeviceMediator : DeviceMediator<IFlatDeviceVM, IFlatDeviceConsumer, FlatDeviceInfo>, IFlatDeviceMediator {

        public Task SetBrightness(int brightness, CancellationToken token) {
            return handler.SetBrightness(brightness, token);
        }

        public Task CloseCover(CancellationToken token) {
            return handler.CloseCover(token);
        }

        public Task ToggleLight(object o, CancellationToken token) {
            return handler.ToggleLight(o, token);
        }

        public Task OpenCover(CancellationToken token) {
            return handler.OpenCover(token);
        }
    }
}