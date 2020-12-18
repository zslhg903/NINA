#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Utility.Astrometry;

namespace NINA.Profile {

    public interface IAstrometrySettings : ISettings {
        Hemisphere HemisphereType { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }
        string HorizonFilePath { get; set; }
        CustomHorizon Horizon { get; set; }
    }
}