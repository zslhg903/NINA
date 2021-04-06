#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace NINA.Model.MyRotator {

    public interface IRotator : IDevice {
        bool CanReverse { get; }
        bool Reverse { get; set; }
        bool IsMoving { get; }
        bool Synced { get; }

        float Position { get; }
        float MechanicalPosition { get; }

        float StepSize { get; }

        void Sync(float skyAngle);

        void Move(float position);

        void MoveAbsolute(float position);

        void MoveAbsoluteMechanical(float position);

        void Halt();
    }
}