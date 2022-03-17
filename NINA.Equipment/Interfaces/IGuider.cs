#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Core.Interfaces;
using NINA.Core.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Interfaces {

    public interface IGuider : IDevice {
        double PixelScale { get; set; }
        string State { get; }
        bool CanClearCalibration { get; }
        bool CanSetShiftRate { get; }
        bool ShiftEnabled { get; }

        SiderealShiftTrackingRate ShiftRate { get; }

        event EventHandler<IGuideStep> GuideEvent;

        Task<bool> AutoSelectGuideStar();

        //Task<bool> Pause(bool pause, CancellationToken ct);

        Task<bool> StartGuiding(bool forceCalibration, IProgress<ApplicationStatus> progress, CancellationToken ct);

        Task<bool> StopGuiding(CancellationToken ct);

        Task<bool> Dither(IProgress<ApplicationStatus> progress, CancellationToken ct);

        Task<bool> ClearCalibration(CancellationToken ct);

        Task<bool> SetShiftRate(SiderealShiftTrackingRate shiftTrackingRate, CancellationToken ct);

        Task<bool> StopShifting(CancellationToken ct);
    }

    public interface IGuiderAppState {
        string State { get; }
    }
}