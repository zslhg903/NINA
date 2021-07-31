#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Profile.Interfaces;
using System;
using System.Runtime.Serialization;

namespace NINA.Profile {

    [Serializable()]
    [DataContract]
    internal class RotatorSettings : Settings, IRotatorSettings {

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context) {
            SetDefaultValues();
        }

        protected override void SetDefaultValues() {
            id = "No_Device";
            reverse = false;
            rangeType = RotatorRangeTypeEnum.FULL;
            rangeStartMechanicalPosition = 0.0f;
        }

        private string id;

        [DataMember]
        public string Id {
            get {
                return id;
            }
            set {
                if (id != value) {
                    id = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool reverse;

        [DataMember]
        public bool Reverse {
            get {
                return reverse;
            }
            set {
                if (reverse != value) {
                    reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RotatorRangeTypeEnum rangeType;

        [DataMember]
        public RotatorRangeTypeEnum RangeType {
            get {
                return rangeType;
            }
            set {
                if (rangeType != value) {
                    rangeType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private float rangeStartMechanicalPosition;

        [DataMember]
        public float RangeStartMechanicalPosition {
            get {
                return rangeStartMechanicalPosition;
            }
            set {
                if (rangeStartMechanicalPosition != value) {
                    rangeStartMechanicalPosition = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}