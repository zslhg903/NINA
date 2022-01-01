#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility;
using NINA.Astrometry;

namespace NINA.Astrometry {

    public class FramingRectangle : ObservableRotatingRectangle {

        public FramingRectangle(double rotationOffset) : base(rotationOffset) {
        }

        public FramingRectangle(double x, double y, double width, double height) : base(x, y, width, height) {
        }

        private int id;

        public int Id {
            get {
                return id;
            }
            set {
                id = value;
                RaisePropertyChanged();
            }
        }

        private Coordinates coordinates;

        public Coordinates Coordinates {
            get {
                return coordinates;
            }
            set {
                coordinates = value;
                RaisePropertyChanged();
            }
        }
    }
}