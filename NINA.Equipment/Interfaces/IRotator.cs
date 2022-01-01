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

namespace NINA.Equipment.Interfaces {

    public interface IRotator : IDevice {
        bool CanReverse { get; }
        bool Reverse { get; set; }
        bool IsMoving { get; }
        bool Synced { get; }

        float Position { get; }
        float MechanicalPosition { get; }

        float StepSize { get; }

        void Sync(float skyAngle);

        Task<bool> Move(float position, CancellationToken ct);

        Task<bool> MoveAbsolute(float position, CancellationToken ct);

        Task<bool> MoveAbsoluteMechanical(float position, CancellationToken ct);

        void Halt();
    }
}