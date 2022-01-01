#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility;
using System.ComponentModel;

namespace NINA.Core.Enum {

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum BlindSolverEnum {

        [Description("LblAstrometryNet")]
        ASTROMETRY_NET,

        [Description("LblLocalPlatesolver")]
        LOCAL,

        [Description("LblASPS")]
        ASPS,

        [Description("LblASTAPShort")]
        ASTAP
    }
}