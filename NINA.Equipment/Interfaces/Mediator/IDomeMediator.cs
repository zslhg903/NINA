﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Equipment.Equipment.MyDome;
using NINA.Equipment.Interfaces.ViewModel;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Interfaces.Mediator {

    public interface IDomeMediator : IDeviceMediator<IDomeVM, IDomeConsumer, DomeInfo> {

        bool IsFollowingScope { get; }

        Task WaitForDomeSynchronization(CancellationToken cancellationToken);

        Task<bool> SyncToScopeCoordinates(Coordinates coordinates, PierSide sideOfPier, CancellationToken cancellationToken);

        Task<bool> OpenShutter(CancellationToken cancellationToken);

        Task<bool> CloseShutter(CancellationToken cancellationToken);

        Task<bool> EnableFollowing(CancellationToken cancellationToken);

        Task<bool> DisableFollowing(CancellationToken cancellationToken);

        Task<bool> Park(CancellationToken cancellationToken);

        Task<bool> SlewToAzimuth(double degrees, CancellationToken cancellationToken);
    }
}