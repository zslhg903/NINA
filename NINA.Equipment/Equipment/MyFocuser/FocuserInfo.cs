#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Collections.Generic;

namespace NINA.Equipment.Equipment.MyFocuser {

    public class FocuserInfo : DeviceInfo {
        private int position;

        public int Position {
            get { return position; }
            set { position = value; RaisePropertyChanged(); }
        }

        private double stepsize;

        public double StepSize {
            get { return stepsize; }
            set { stepsize = value; RaisePropertyChanged(); }
        }

        private double temperature;

        public double Temperature {
            get { return temperature; }
            set { temperature = value; RaisePropertyChanged(); }
        }

        private bool isMoving;

        public bool IsMoving {
            get { return isMoving; }
            set { isMoving = value; RaisePropertyChanged(); }
        }

        private bool isSettling;

        public bool IsSettling {
            get { return isSettling; }
            set { isSettling = value; RaisePropertyChanged(); }
        }

        private bool tempComp;

        public bool TempComp {
            get { return tempComp; }
            set { tempComp = value; RaisePropertyChanged(); }
        }

        private bool tempCompAvailable;

        public bool TempCompAvailable {
            get { return tempCompAvailable; }
            set { tempCompAvailable = value; RaisePropertyChanged(); }
        }

        private IList<string> supportedActions;

        public IList<string> SupportedActions {
            get => supportedActions;
            set {
                supportedActions = value;
                RaisePropertyChanged();
            }
        }
    }
}