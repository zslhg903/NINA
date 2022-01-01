#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace NINA.Equipment.Equipment.MyRotator {

    public class RotatorInfo : DeviceInfo {
        private bool canReverse;

        public bool CanReverse {
            get { return canReverse; }
            set { canReverse = value; RaisePropertyChanged(); }
        }

        private bool reverse;

        public bool Reverse {
            get { return reverse; }
            set { reverse = value; RaisePropertyChanged(); }
        }

        private float mechanicalPosition;

        public float MechanicalPosition {
            get { return mechanicalPosition; }
            set { mechanicalPosition = value; RaisePropertyChanged(); }
        }

        private float position;

        public float Position {
            get { return position; }
            set { position = value; RaisePropertyChanged(); }
        }

        private float stepsize;

        public float StepSize {
            get { return stepsize; }
            set { stepsize = value; RaisePropertyChanged(); }
        }

        private bool isMoving;

        public bool IsMoving {
            get { return isMoving; }
            set { isMoving = value; RaisePropertyChanged(); }
        }

        private bool synced;

        public bool Synced {
            get { return synced; }
            set { synced = value; RaisePropertyChanged(); }
        }
    }
}