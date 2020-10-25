﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Utility;
using NINA.Utility.Astrometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.Model {

    [JsonObject(MemberSerialization.OptIn)]
    public class InputTarget : BaseINPC {

        public InputTarget(Angle latitude, Angle longitude) {
            this.latitude = latitude;
            this.longitude = longitude;
            DeepSkyObject = new DeepSkyObject(string.Empty, string.Empty);
            DeepSkyObject.SetDateAndPosition(NighttimeCalculator.GetReferenceDate(DateTime.Now), latitude.Degree, longitude.Degree);
            InputCoordinates = new InputCoordinates();
            InputCoordinates.PropertyChanged += InputCoordinates_PropertyChanged;
        }

        private void InputCoordinates_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            RaiseCoordinatesChanged();
        }

        private bool expanded = true;

        [JsonProperty]
        public bool Expanded {
            get => expanded;
            set {
                expanded = value;
                RaisePropertyChanged();
            }
        }

        private DeepSkyObject deepSkyObject;

        public DeepSkyObject DeepSkyObject {
            get => deepSkyObject;
            set {
                deepSkyObject = value;
                RaisePropertyChanged();
            }
        }

        private string targetName;

        [JsonProperty]
        public string TargetName {
            get {
                return targetName;
            }
            set {
                targetName = value;
                RaisePropertyChanged();
                RaiseCoordinatesChanged();
            }
        }

        private double rotation;

        [JsonProperty]
        public double Rotation {
            get {
                return rotation;
            }
            set {
                rotation = value;
                RaiseCoordinatesChanged();
            }
        }

        private InputCoordinates inputCoordinates;
        private Angle latitude;
        private Angle longitude;

        [JsonProperty]
        public InputCoordinates InputCoordinates {
            get => inputCoordinates;
            set {
                if (inputCoordinates != null) {
                    InputCoordinates.PropertyChanged -= InputCoordinates_PropertyChanged;
                }
                inputCoordinates = value;
                if (inputCoordinates != null) {
                    InputCoordinates.PropertyChanged += InputCoordinates_PropertyChanged;
                }
                RaiseCoordinatesChanged();
            }
        }

        private void RaiseCoordinatesChanged() {
            RaisePropertyChanged(nameof(Rotation));
            RaisePropertyChanged(nameof(InputCoordinates));

            DeepSkyObject.Name = TargetName;
            DeepSkyObject.Coordinates = InputCoordinates.Coordinates;
            DeepSkyObject.Rotation = Rotation;
        }
    }
}