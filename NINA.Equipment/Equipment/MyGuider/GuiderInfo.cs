#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace NINA.Equipment.Equipment.MyGuider {

    public class GuiderInfo : DeviceInfo {
        private bool _canClearCalibration;

        public bool CanClearCalibration {
            get => _canClearCalibration;
            set {
                _canClearCalibration = value;
                RaisePropertyChanged();
            }
        }

        private bool _canSetShiftRate;

        public bool CanSetShiftRate {
            get => _canSetShiftRate;
            set {
                _canSetShiftRate = value;
                RaisePropertyChanged();
            }
        }
    }
}