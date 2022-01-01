﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NINA.Core.Interfaces;

namespace NINA.Equipment.Equipment.MyGuider.PHD2.PhdEvents {

    [DataContract]
    public class PhdEventGuideStep : PhdEvent, IGuideStep {

        [DataMember]
        [JsonProperty]
        private double frame;

        [DataMember]
        [JsonProperty]
        private double time;

        [DataMember]
        [JsonProperty]
        private string mount;

        [DataMember]
        [JsonProperty]
        private double dx;

        [DataMember]
        [JsonProperty]
        private double dy;

        [DataMember]
        [JsonProperty]
        private double rADistanceRaw;

        [DataMember]
        [JsonProperty]
        private double decDistanceRaw;

        [DataMember]
        [JsonProperty]
        private double raDistanceDisplay;

        [DataMember]
        [JsonProperty]
        private double decDistanceDisplay;

        [DataMember]
        [JsonProperty]
        private double rADistanceGuide;

        [DataMember]
        [JsonProperty]
        private double decDistanceGuide;

        [DataMember]
        [JsonProperty]
        private double raDistanceGuideDisplay;

        [DataMember]
        [JsonProperty]
        private double decDistanceGuideDisplay;

        [DataMember]
        [JsonProperty]
        private double rADuration;

        [DataMember]
        [JsonProperty]
        private string rADirection;

        [DataMember]
        [JsonProperty]
        private double dECDuration;

        [DataMember]
        [JsonProperty]
        private string decDirection;

        [DataMember]
        [JsonProperty]
        private double starMass;

        [DataMember]
        [JsonProperty]
        private double sNR;

        [DataMember]
        [JsonProperty]
        private double avgDist;

        [DataMember]
        [JsonProperty]
        private bool rALimited;

        [DataMember]
        [JsonProperty]
        private bool decLimited;

        [DataMember]
        [JsonProperty]
        private double errorCode;

        public PhdEventGuideStep() {
        }

        [DataMember]
        [JsonProperty]
        public double RADistanceGuideDisplay {
            get {
                return raDistanceGuideDisplay;
            }
            set {
                raDistanceGuideDisplay = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double DecDistanceGuideDisplay {
            get {
                return decDistanceGuideDisplay;
            }
            set {
                decDistanceGuideDisplay = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double Frame {
            get {
                return frame;
            }

            set {
                frame = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double Time {
            get {
                return time;
            }

            set {
                time = DateTime.UtcNow
               .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
               .TotalSeconds;
            }
        }

        [DataMember]
        [JsonProperty]
        public string Mount {
            get {
                return mount;
            }

            set {
                mount = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double Dx {
            get {
                return dx;
            }

            set {
                dx = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double Dy {
            get {
                return dy;
            }

            set {
                dy = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double RADistanceRaw {
            get {
                return -rADistanceRaw;
            }

            set {
                rADistanceRaw = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double DECDistanceRaw {
            get {
                return decDistanceRaw;
            }

            set {
                decDistanceRaw = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double RADistanceGuide {
            get {
                return rADistanceGuide;
            }

            set {
                rADistanceGuide = value;
                RADistanceGuideDisplay = RADistanceGuide;
            }
        }

        [DataMember]
        [JsonProperty]
        public double DECDistanceGuide {
            get {
                return decDistanceGuide;
            }

            set {
                decDistanceGuide = value;
                DecDistanceGuideDisplay = DECDistanceRaw;
            }
        }

        [DataMember]
        [JsonProperty]
        public double RADuration {
            get {
                if (RADirection == "East") {
                    return -rADuration;
                } else {
                    return rADuration;
                }
            }

            set {
                rADuration = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public string RADirection {
            get {
                return rADirection;
            }

            set {
                rADirection = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double DECDuration {
            get {
                if (DECDirection == "South") {
                    return -dECDuration;
                } else {
                    return dECDuration;
                }
            }

            set {
                dECDuration = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public string DECDirection {
            get {
                return decDirection;
            }

            set {
                decDirection = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double StarMass {
            get {
                return starMass;
            }

            set {
                starMass = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double SNR {
            get {
                return sNR;
            }

            set {
                sNR = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double AvgDist {
            get {
                return avgDist;
            }

            set {
                avgDist = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public bool RALimited {
            get {
                return rALimited;
            }

            set {
                rALimited = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public bool DecLimited {
            get {
                return decLimited;
            }

            set {
                decLimited = value;
            }
        }

        [DataMember]
        [JsonProperty]
        public double ErrorCode {
            get {
                return errorCode;
            }

            set {
                errorCode = value;
            }
        }

        public IGuideStep Clone() {
            return (PhdEventGuideStep)this.MemberwiseClone();
        }
    }
}