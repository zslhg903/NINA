#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Equipment.MyGuider;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Model;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Astrometry;

namespace NINA.WPF.Base.Mediator {

    public class GuiderMediator : DeviceMediator<IGuiderVM, IGuiderConsumer, GuiderInfo>, IGuiderMediator {

        public Task<bool> Dither(CancellationToken token) {
            return handler.Dither(token);
        }

        public Guid StartRMSRecording() {
            return handler.StartRMSRecording();
        }

        public RMS StopRMSRecording(Guid handle) {
            return handler.StopRMSRecording(handle);
        }

        public Task<bool> StartGuiding(bool forceCalibration, IProgress<ApplicationStatus> progress, CancellationToken token) {
            return handler.StartGuiding(forceCalibration, progress, token);
        }

        public Task<bool> StopGuiding(CancellationToken token) {
            return handler.StopGuiding(token);
        }

        public Task<bool> AutoSelectGuideStar(CancellationToken token) {
            return handler.AutoSelectGuideStar(token);
        }

        public Task<bool> ClearCalibration(CancellationToken token) {
            return handler.ClearCalibration(token);
        }

        public Task<bool> SetShiftRate(SiderealShiftTrackingRate shiftTrackingRate, CancellationToken ct) {
            return handler.SetShiftRate(shiftTrackingRate, ct);
        }

        public Task<bool> StopShifting(CancellationToken ct) {
            return handler.StopShifting(ct);
        }
    }
}