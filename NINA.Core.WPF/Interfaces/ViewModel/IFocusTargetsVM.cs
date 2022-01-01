﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Collections.ObjectModel;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Core.Utility;
using NINA.Astrometry;
using NINA.Equipment.Interfaces.ViewModel;

namespace NINA.WPF.Base.Interfaces.ViewModel {

    public interface IFocusTargetsVM : IDockableVM {
        ObservableCollection<FocusTarget> FocusTargets { get; set; }
        FocusTarget SelectedFocusTarget { get; set; }
        IAsyncCommand SlewToCoordinatesCommand { get; }
        bool TelescopeConnected { get; set; }

        void Dispose();

        void UpdateDeviceInfo(TelescopeInfo deviceInfo);
    }
}