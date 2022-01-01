#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Utility;
using System;
using System.Runtime.Serialization;

namespace NINA.Core.Model.Equipment {

    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    [DataContract]
    public class FlatWizardFilterSettings : BaseINPC {
        private FlatWizardMode flatWizardMode;
        private double histogramMeanTarget;

        private double histogramTolerance;

        private double maxFlatExposureTime;

        private double minFlatExposureTime;

        private double stepSize;

        private int maxAbsoluteFlatDeviceBrightness;

        private int minAbsoluteFlatDeviceBrightness;

        private int flatDeviceAbsoluteStepSize;

        public FlatWizardFilterSettings() {
            flatWizardMode = FlatWizardMode.DYNAMICEXPOSURE;
            HistogramMeanTarget = 0.5;
            HistogramTolerance = 0.1;
            StepSize = 0.1;
            MinFlatExposureTime = 0.01;
            MaxFlatExposureTime = 30;
            MinAbsoluteFlatDeviceBrightness = 0;
            MaxAbsoluteFlatDeviceBrightness = 1;
            FlatDeviceAbsoluteStepSize = 1;
        }

        [DataMember]
        [JsonProperty]
        public FlatWizardMode FlatWizardMode {
            get => flatWizardMode;
            set {
                flatWizardMode = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public double HistogramMeanTarget {
            get => histogramMeanTarget;
            set {
                histogramMeanTarget = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public double HistogramTolerance {
            get => histogramTolerance;
            set {
                histogramTolerance = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public double MaxFlatExposureTime {
            get => maxFlatExposureTime;
            set {
                maxFlatExposureTime = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public double MinFlatExposureTime {
            get => minFlatExposureTime;
            set {
                minFlatExposureTime = value;
                if (MaxFlatExposureTime < minFlatExposureTime) {
                    MaxFlatExposureTime = minFlatExposureTime;
                }

                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public double StepSize {
            get => stepSize;
            set {
                stepSize = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public int MaxAbsoluteFlatDeviceBrightness {
            get => maxAbsoluteFlatDeviceBrightness;
            set {
                maxAbsoluteFlatDeviceBrightness = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public int MinAbsoluteFlatDeviceBrightness {
            get => minAbsoluteFlatDeviceBrightness;
            set {
                minAbsoluteFlatDeviceBrightness = value;
                if (MaxAbsoluteFlatDeviceBrightness < minAbsoluteFlatDeviceBrightness) {
                    MaxAbsoluteFlatDeviceBrightness = minAbsoluteFlatDeviceBrightness;
                }

                RaisePropertyChanged();
            }
        }

        [DataMember]
        [JsonProperty]
        public int FlatDeviceAbsoluteStepSize {
            get => flatDeviceAbsoluteStepSize;
            set {
                flatDeviceAbsoluteStepSize = value;
                RaisePropertyChanged();
            }
        }
    }
}