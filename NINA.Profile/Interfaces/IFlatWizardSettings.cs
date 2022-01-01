#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Core.Model.Equipment;

namespace NINA.Profile.Interfaces {

    public interface IFlatWizardSettings : ISettings {
        int FlatCount { get; set; }
        double HistogramMeanTarget { get; set; }
        double HistogramTolerance { get; set; }
        double StepSize { get; set; }
        BinningMode BinningMode { get; set; }
        int DarkFlatCount { get; set; }
        AltitudeSite AltitudeSite { get; set; }
        FlatWizardMode FlatWizardMode { get; set; }
    }
}