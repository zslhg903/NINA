﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Sequencer.Container;
using NINA.ViewModel.Sequencer;
using System.Collections.Generic;

namespace NINA.Sequencer.Interfaces.Mediator {

    public interface ISequenceMediator {

        IList<IDeepSkyObjectContainer> GetDeepSkyObjectContainerTemplates();

        void SetAdvancedSequence(ISequenceRootContainer container);

        void AddAdvancedTarget(IDeepSkyObjectContainer container);

        void AddSimpleTarget(DeepSkyObject deepSkyObject);

        void RegisterSequenceNavigation(ISequenceNavigationVM sequenceNavigation);

        void SwitchToAdvancedView();

        void SwitchToOverview();

        void AddTargetToTargetList(IDeepSkyObjectContainer container);

        bool Initialized { get; }
    }
}