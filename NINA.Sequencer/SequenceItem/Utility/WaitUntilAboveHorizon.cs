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
using NINA.Model;
using NINA.Profile;
using NINA.Sequencer.Utility;
using NINA.Sequencer.Validations;
using NINA.Astrometry;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Sequencer.SequenceItem.Utility {

    [ExportMetadata("Name", "Lbl_SequenceItem_Utility_WaitUntilAboveHorizon_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Utility_WaitUntilAboveHorizon_Description")]
    [ExportMetadata("Icon", "WaitForAltitudeSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Utility")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class WaitUntilAboveHorizon : SequenceItem, IValidatable {
        private IProfileService profileService;
        private bool hasDsoParent;

        [ImportingConstructor]
        public WaitUntilAboveHorizon(IProfileService profileService) {
            this.profileService = profileService;
            Coordinates = new InputCoordinates();
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

        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = value;
                RaisePropertyChanged();
            }
        }

        public int UpdateInterval { get; set; } = 1;

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            do {
                var coordinates = Coordinates.Coordinates;
                var altaz = coordinates.Transform(Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Latitude), Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Longitude));
                var horizon = profileService.ActiveProfile.AstrometrySettings.Horizon;

                var horizonAltitude = 0d;
                if (horizon != null) {
                    horizonAltitude = horizon.GetAltitude(altaz.Azimuth.Degree);
                }

                progress?.Report(new ApplicationStatus() {
                    Status = string.Format(Locale.Loc.Instance["Lbl_SequenceItem_Utility_WaitUntilAboveHorizon_Progress"], Math.Round(altaz.Altitude.Degree, 2), Math.Round(horizonAltitude, 2))
                });

                if (altaz.Altitude.Degree > horizonAltitude) {
                    break;
                } else {
                    await NINA.Utility.Utility.Delay(TimeSpan.FromSeconds(UpdateInterval), token);
                }
            } while (true);
        }

        public override object Clone() {
            return new WaitUntilAboveHorizon(profileService) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
                Coordinates = Coordinates.Clone()
            };
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
            return $"Category: {Category}, Item: {nameof(WaitUntilAboveHorizon)}";
        }

        public bool Validate() {
            var issues = new List<string>();

            var maxAlt = AstroUtil.GetAltitude(0, profileService.ActiveProfile.AstrometrySettings.Latitude, Coordinates.DecDegrees);

            var horizon = profileService.ActiveProfile.AstrometrySettings.Horizon;
            var minHorizonAlt = horizon?.GetMinAltitude() ?? 0;

            if (maxAlt < minHorizonAlt) {
                issues.Add(Locale.Loc.Instance["LblUnreachableAltitudeForHorizon"]);
            }

            Issues = issues;
            return issues.Count == 0;
        }
    }
}