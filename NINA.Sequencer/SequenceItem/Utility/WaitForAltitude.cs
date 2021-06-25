﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Utility;
using NINA.Sequencer.Validations;
using NINA.Core.Utility;
using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Locale;

namespace NINA.Sequencer.SequenceItem.Utility {

    [ExportMetadata("Name", "Lbl_SequenceItem_Utility_WaitForAltitude_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Utility_WaitForAltitude_Description")]
    [ExportMetadata("Icon", "WaitForAltitudeSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Utility")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class WaitForAltitude : SequenceItem, IValidatable {
        private IProfileService profileService;
        private string aboveOrBelow;
        private double altitude;
        private bool hasDsoParent;

        [ImportingConstructor]
        public WaitForAltitude(IProfileService profileService) {
            this.profileService = profileService;
            Coordinates = new InputCoordinates();
            AboveOrBelow = ">=";
            Altitude = 30;
        }

        private WaitForAltitude(WaitForAltitude cloneMe) : this(cloneMe.profileService) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new WaitForAltitude(this) {
                Altitude = Altitude,
                AboveOrBelow = AboveOrBelow,
                Coordinates = Coordinates.Clone()
            };
        }

        [JsonProperty]
        public InputCoordinates Coordinates { get; set; }

        [JsonProperty]
        public bool HasDsoParent {
            get => hasDsoParent;
            set {
                hasDsoParent = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public double Altitude {
            get => altitude;
            set {
                altitude = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public string AboveOrBelow {
            get => aboveOrBelow;
            set {
                aboveOrBelow = value;
                RaisePropertyChanged();
            }
        }

        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            do {
                var coordinates = Coordinates.Coordinates;
                var altaz = coordinates.Transform(Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Latitude), Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Longitude));
                progress?.Report(new ApplicationStatus() {
                    Status = string.Format(Loc.Instance["Lbl_SequenceItem_Utility_WaitForAltitude_Progress"], Math.Round(altaz.Altitude.Degree, 2), Altitude)
                });

                if (aboveOrBelow == ">=" && altaz.Altitude.Degree >= Altitude) {
                    break;
                } else if (aboveOrBelow == "<=" && altaz.Altitude.Degree <= Altitude) {
                    break;
                }

                await NINA.Core.Utility.CoreUtil.Delay(TimeSpan.FromSeconds(1), token);
            } while (true);
        }

        public override void AfterParentChanged() {
            var coordinates = ItemUtility.RetrieveContextCoordinates(this.Parent).Item1;
            if (coordinates != null) {
                Coordinates.Coordinates = coordinates;
                HasDsoParent = true;
            } else {
                HasDsoParent = false;
            }
            Validate();
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(WaitForAltitude)}, Altitude: {AboveOrBelow}{Altitude}";
        }

        public bool Validate() {
            var issues = new List<string>();

            var maxAlt = AstroUtil.GetAltitude(0, profileService.ActiveProfile.AstrometrySettings.Latitude, Coordinates.DecDegrees);
            var minAlt = AstroUtil.GetAltitude(180, profileService.ActiveProfile.AstrometrySettings.Latitude, Coordinates.DecDegrees);

            if (aboveOrBelow == ">=") {
                if (maxAlt < Altitude) {
                    issues.Add(Loc.Instance["LblUnreachableAltitude"]);
                }
            } else {
                if (minAlt > Altitude) {
                    issues.Add(Loc.Instance["LblUnreachableAltitude"]);
                }
            }

            Issues = issues;
            return issues.Count == 0;
        }
    }
}