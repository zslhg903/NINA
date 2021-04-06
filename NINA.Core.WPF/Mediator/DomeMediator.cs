﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Model.MyDome;
using NINA.Utility.Mediator.Interfaces;
using NINA.ViewModel.Equipment.Dome;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Utility.Mediator {

    public class DomeMediator : DeviceMediator<IDomeVM, IDomeConsumer, DomeInfo>, IDomeMediator {

        public Task<bool> OpenShutter(CancellationToken cancellationToken) {
            return handler.OpenShutter(cancellationToken);
        }

        public Task<bool> EnableFollowing(CancellationToken cancellationToken) {
            return handler.EnableFollowing(cancellationToken);
        }

        public Task WaitForDomeSynchronization(CancellationToken cancellationToken) {
            return handler.WaitForDomeSynchronization(cancellationToken);
        }

        public Task<bool> CloseShutter(CancellationToken cancellationToken) {
            return handler.CloseShutter(cancellationToken);
        }

        public Task<bool> Park(CancellationToken cancellationToken) {
            return handler.Park(cancellationToken);
        }

        public Task<bool> DisableFollowing(CancellationToken cancellationToken) {
            return handler.DisableFollowing(cancellationToken);
        }

        public Task<bool> SlewToAzimuth(double degrees, CancellationToken cancellationToken) {
            return handler.SlewToAzimuth(degrees, cancellationToken);
        }
    }
}