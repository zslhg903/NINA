#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Astrometry;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Model;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Equipment.Interfaces;

namespace NINA.WPF.Base.Mediator {

    public class TelescopeMediator : DeviceMediator<ITelescopeVM, ITelescopeConsumer, TelescopeInfo>, ITelescopeMediator {

        public void MoveAxis(TelescopeAxes axis, double rate) {
            handler.MoveAxis(axis, rate);
        }

        public void PulseGuide(GuideDirections direction, int duration) {
            handler.PulseGuide(direction, duration);
        }

        public async Task<bool> Sync(Coordinates coordinates) {
            return await handler.Sync(coordinates);
        }

        public Task<bool> SlewToCoordinatesAsync(Coordinates coords, CancellationToken token) {
            return handler.SlewToCoordinatesAsync(coords, token);
        }

        public Task<bool> SlewToCoordinatesAsync(TopocentricCoordinates coords, CancellationToken token) {
            return handler.SlewToCoordinatesAsync(coords, token);
        }

        public Task<bool> MeridianFlip(Coordinates targetCoordinates, CancellationToken token) {
            return handler.MeridianFlip(targetCoordinates, token);
        }

        public bool SetTrackingEnabled(bool tracking) {
            return handler.SetTrackingEnabled(tracking);
        }

        public bool SendToSnapPort(bool start) {
            return handler.SendToSnapPort(start);
        }

        public Task<bool> ParkTelescope(IProgress<ApplicationStatus> progress, CancellationToken token) {
            return handler.ParkTelescope(progress, token);
        }

        public Task<bool> UnparkTelescope(IProgress<ApplicationStatus> progress, CancellationToken token) {
            return handler.UnparkTelescope(progress, token);
        }

        public Coordinates GetCurrentPosition() {
            return handler.GetCurrentPosition();
        }

        public Task WaitForSlew(CancellationToken cancellationToken) {
            return handler.WaitForSlew(cancellationToken);
        }

        public bool SetTrackingMode(TrackingMode trackingMode) {
            return handler.SetTrackingMode(trackingMode);
        }

        public bool SetCustomTrackingRate(double rightAscensionRate, double declinationRate) {
            return handler.SetCustomTrackingRate(rightAscensionRate, declinationRate);
        }

        public Task<bool> FindHome(IProgress<ApplicationStatus> progress, CancellationToken token) {
            return handler.FindHome(progress, token);
        }
    }
}