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
    public class InputCoordinates : BaseINPC {

        public InputCoordinates() {
            Coordinates = new Coordinates(Angle.Zero, Angle.Zero, Epoch.J2000);
        }

        public InputCoordinates(Coordinates coordinates) {
            Coordinates = coordinates;
        }

        private Coordinates coordinates;

        public Coordinates Coordinates {
            get => coordinates;
            set {
                coordinates = value;
                RaiseCoordinatesChanged();
            }
        }

        [JsonProperty]
        public int RAHours {
            get {
                return (int)Math.Truncate(coordinates.RA);
            }
            set {
                if (value >= 0) {
                    coordinates.RA = coordinates.RA - RAHours + value;
                    RaiseCoordinatesChanged();
                }
            }
        }

        [JsonProperty]
        public int RAMinutes {
            get {
                return (int)(Math.Floor(coordinates.RA * 60.0d) % 60);
            }
            set {
                if (value >= 0) {
                    coordinates.RA = coordinates.RA - RAMinutes / 60.0d + value / 60.0d;
                    RaiseCoordinatesChanged();
                }
            }
        }

        [JsonProperty]
        public int RASeconds {
            get {
                return (int)(Math.Floor(coordinates.RA * 60.0d * 60.0d) % 60);
            }
            set {
                if (value >= 0) {
                    coordinates.RA = coordinates.RA - RASeconds / (60.0d * 60.0d) + value / (60.0d * 60.0d);
                    RaiseCoordinatesChanged();
                }
            }
        }

        private bool negativeDec;

        public bool NegativeDec {
            get => negativeDec;
            set {
                negativeDec = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public int DecDegrees {
            get {
                return (int)Math.Truncate(coordinates.Dec);
            }
            set {
                if (NegativeDec) {
                    coordinates.Dec = value - DecMinutes / 60.0d - DecSeconds / (60.0d * 60.0d);
                } else {
                    coordinates.Dec = value + DecMinutes / 60.0d + DecSeconds / (60.0d * 60.0d);
                }
                RaiseCoordinatesChanged();
            }
        }

        [JsonProperty]
        public int DecMinutes {
            get {
                return (int)Math.Floor((Math.Abs(coordinates.Dec * 60.0d) % 60));
            }
            set {
                if (coordinates.Dec < 0) {
                    coordinates.Dec = coordinates.Dec + DecMinutes / 60.0d - value / 60.0d;
                } else {
                    coordinates.Dec = coordinates.Dec - DecMinutes / 60.0d + value / 60.0d;
                }

                RaiseCoordinatesChanged();
            }
        }

        [JsonProperty]
        public int DecSeconds {
            get {
                return (int)Math.Round((Math.Abs(coordinates.Dec * 60.0d * 60.0d) % 60));
            }
            set {
                if (coordinates.Dec < 0) {
                    coordinates.Dec = coordinates.Dec + DecSeconds / (60.0d * 60.0d) - value / (60.0d * 60.0d);
                } else {
                    coordinates.Dec = coordinates.Dec - DecSeconds / (60.0d * 60.0d) + value / (60.0d * 60.0d);
                }

                RaiseCoordinatesChanged();
            }
        }

        private void RaiseCoordinatesChanged() {
            RaisePropertyChanged(nameof(Coordinates));
            RaisePropertyChanged(nameof(RAHours));
            RaisePropertyChanged(nameof(RAMinutes));
            RaisePropertyChanged(nameof(RASeconds));
            RaisePropertyChanged(nameof(DecDegrees));
            RaisePropertyChanged(nameof(DecMinutes));
            RaisePropertyChanged(nameof(DecSeconds));
            NegativeDec = Coordinates?.Dec < 0;

            this.CoordinatesChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler CoordinatesChanged;

        public InputCoordinates Clone() =>
            new InputCoordinates(coordinates.Clone());

        public override string ToString() {
            return Coordinates.ToString();
        }
    }
}