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
using NINA.Equipment.Equipment.MyFlatDevice;
using System.Threading.Tasks;

namespace NINA.Equipment.Interfaces.ViewModel {

    public interface IFlatDeviceVM : IDeviceVM<FlatDeviceInfo>, IDockableVM {

        Task<bool> OpenCover(CancellationToken token);

        Task<bool> CloseCover(CancellationToken token);

        int Brightness { get; set; }
        bool LightOn { get; set; }
        FlatDeviceInfo FlatDeviceInfo { get; set; }

        Task<bool> ToggleLight(object o, CancellationToken token);

        Task<bool> SetBrightness(int value, CancellationToken token);

        Task<bool> SetBrightness(object o, CancellationToken token);
    }
}