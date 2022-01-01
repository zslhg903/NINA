﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Core.Utility;
using NINA.Astrometry;
using System;

namespace NINA.Astrometry {

    [JsonObject(MemberSerialization.OptIn)]
    public class InputTopocentricCoordinates : BaseINPC {

        public InputTopocentricCoordinates(Angle latitude, Angle longitude) {
            Coordinates = new TopocentricCoordinates(Angle.Zero, Angle.Zero, latitude, longitude);
        }

        public InputTopocentricCoordinates(TopocentricCoordinates coordinates) {
            Coordinates = coordinates;
        }

        private TopocentricCoordinates coordinates;

        public TopocentricCoordinates Coordinates {
            get => coordinates;
            set {
                coordinates = value;
                RaiseCoordinatesChanged();
            }
        }

        [JsonProperty]
        public int AzDegrees {
            get {
                return (int)Math.Truncate(coordinates.Azimuth.Degree);
            }
            set {
                if (value >= 0) {
                    coordinates.Azimuth = Angle.ByDegree(coordinates.Azimuth.Degree - AzDegrees + value);
                    RaiseCoordinatesChanged();
                }
            }
        }

        [JsonProperty]
        public int AzMinutes {
            get {
                return (int)(Math.Floor(coordinates.Azimuth.Degree * 60.0d) % 60);
            }
            set {
                if (value >= 0) {
                    coordinates.Azimuth = Angle.ByDegree(coordinates.Azimuth.Degree - AzMinutes / 60.0d + value / 60.0d);
                    RaiseCoordinatesChanged();
                }
            }
        }

        [JsonProperty]
        public int AzSeconds {
            get {
                return (int)(Math.Floor(coordinates.Azimuth.Degree * 60.0d * 60.0d) % 60);
            }
            set {
                if (value >= 0) {
                    coordinates.Azimuth = Angle.ByDegree(coordinates.Azimuth.Degree - AzSeconds / (60.0d * 60.0d) + value / (60.0d * 60.0d));
                    RaiseCoordinatesChanged();
                }
            }
        }

        private bool negativeAlt;

        public bool NegativeAlt {
            get => negativeAlt;
            set {
                negativeAlt = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public int AltDegrees {
            get {
                return (int)Math.Truncate(coordinates.Altitude.Degree);
            }
            set {
                if (NegativeAlt) {
                    coordinates.Altitude = Angle.ByDegree(value - AltMinutes / 60.0d - AltSeconds / (60.0d * 60.0d));
                } else {
                    coordinates.Altitude = Angle.ByDegree(value + AltMinutes / 60.0d + AltSeconds / (60.0d * 60.0d));
                }
                RaiseCoordinatesChanged();
            }
        }

        [JsonProperty]
        public int AltMinutes {
            get {
                return (int)Math.Floor((Math.Abs(coordinates.Altitude.Degree * 60.0d) % 60));
            }
            set {
                if (coordinates.Altitude.Degree < 0) {
                    coordinates.Altitude = Angle.ByDegree(coordinates.Altitude.Degree + AltMinutes / 60.0d - value / 60.0d);
                } else {
                    coordinates.Altitude = Angle.ByDegree(coordinates.Altitude.Degree - AltMinutes / 60.0d + value / 60.0d);
                }

                RaiseCoordinatesChanged();
            }
        }

        [JsonProperty]
        public int AltSeconds {
            get {
                return (int)Math.Round((Math.Abs(coordinates.Altitude.Degree * 60.0d * 60.0d) % 60));
            }
            set {
                if (coordinates.Altitude.Degree < 0) {
                    coordinates.Altitude = Angle.ByDegree(coordinates.Altitude.Degree + AltSeconds / (60.0d * 60.0d) - value / (60.0d * 60.0d));
                } else {
                    coordinates.Altitude = Angle.ByDegree(coordinates.Altitude.Degree - AltSeconds / (60.0d * 60.0d) + value / (60.0d * 60.0d));
                }

                RaiseCoordinatesChanged();
            }
        }

        private void RaiseCoordinatesChanged() {
            RaisePropertyChanged(nameof(Coordinates));
            RaisePropertyChanged(nameof(AzDegrees));
            RaisePropertyChanged(nameof(AzMinutes));
            RaisePropertyChanged(nameof(AzSeconds));
            RaisePropertyChanged(nameof(AltDegrees));
            RaisePropertyChanged(nameof(AltMinutes));
            RaisePropertyChanged(nameof(AltSeconds));
            NegativeAlt = Coordinates?.Altitude.Degree < 0;
        }
    }
}