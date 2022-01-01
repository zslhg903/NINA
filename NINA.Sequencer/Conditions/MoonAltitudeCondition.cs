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
using NINA.Core.Enum;
using NINA.Profile.Interfaces;
using NINA.Sequencer.SequenceItem;
using NINA.Core.Utility;
using NINA.Astrometry;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NINA.Sequencer.Utility;
using System.Runtime.Serialization;

namespace NINA.Sequencer.Conditions {

    [ExportMetadata("Name", "Lbl_SequenceCondition_MoonAltitudeCondition_Name")]
    [ExportMetadata("Description", "Lbl_SequenceCondition_MoonAltitudeCondition_Description")]
    [ExportMetadata("Icon", "WaningGibbousMoonSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Condition")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class MoonAltitudeCondition : SequenceCondition {
        private readonly IProfileService profileService;
        private double userMoonAltitude;
        private double currentMoonAltitude;
        private ComparisonOperatorEnum comparator;

        [ImportingConstructor]
        public MoonAltitudeCondition(IProfileService profileService) {
            this.profileService = profileService;
            UserMoonAltitude = 0d;
            Comparator = ComparisonOperatorEnum.GREATER_THAN;

            CalculateCurrentMoonState();
            ConditionWatchdog = new ConditionWatchdog(InterruptWhenMoonOutsideOfBounds, TimeSpan.FromSeconds(5));
        }

        private async Task InterruptWhenMoonOutsideOfBounds() {
            CalculateCurrentMoonState();
            if (!Check(null, null)) {
                if (this.Parent != null) {
                    if (ItemUtility.IsInRootContainer(Parent) && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                        Logger.Info("Moon is outside of the specified altitude range - Interrupting current Instruction Set");
                        await this.Parent.Interrupt();
                    }
                }
            }
        }

        private MoonAltitudeCondition(MoonAltitudeCondition cloneMe) : this(cloneMe.profileService) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new MoonAltitudeCondition(this) {
                UserMoonAltitude = UserMoonAltitude,
                Comparator = Comparator
            };
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            RunWatchdogIfInsideSequenceRoot();
        }

        [JsonProperty]
        public double UserMoonAltitude {
            get => userMoonAltitude;
            set {
                userMoonAltitude = value;
                RaisePropertyChanged();
                CalculateCurrentMoonState();
            }
        }

        [JsonProperty]
        public ComparisonOperatorEnum Comparator {
            get => comparator;
            set {
                comparator = value;
                RaisePropertyChanged();
            }
        }

        public double CurrentMoonAltitude {
            get => currentMoonAltitude;
            set {
                currentMoonAltitude = value;
                RaisePropertyChanged();
            }
        }

        public ComparisonOperatorEnum[] ComparisonOperators => Enum.GetValues(typeof(ComparisonOperatorEnum))
            .Cast<ComparisonOperatorEnum>()
            .Where(p => p != ComparisonOperatorEnum.EQUALS)
            .Where(p => p != ComparisonOperatorEnum.NOT_EQUAL)
            .ToArray();

        public override void AfterParentChanged() {
            RunWatchdogIfInsideSequenceRoot();
        }

        public override string ToString() {
            return $"Condition: {nameof(MoonAltitudeCondition)}, " +
                $"CurrentMoonAltitude: {CurrentMoonAltitude}, Comparator: {Comparator}, UserMoonAltitude: {UserMoonAltitude}";
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            // See if the moon's altitude is outside of the user's wishes
            switch (Comparator) {
                case ComparisonOperatorEnum.LESS_THAN:
                    if (CurrentMoonAltitude < UserMoonAltitude) { return false; }
                    break;

                case ComparisonOperatorEnum.LESS_THAN_OR_EQUAL:
                    if (CurrentMoonAltitude <= UserMoonAltitude) { return false; }
                    break;

                case ComparisonOperatorEnum.GREATER_THAN:
                    if (CurrentMoonAltitude > UserMoonAltitude) { return false; }
                    break;

                case ComparisonOperatorEnum.GREATER_THAN_OR_EQUAL:
                    if (CurrentMoonAltitude >= UserMoonAltitude) { return false; }
                    break;
            }

            // Everything is fine
            return true;
        }

        private void CalculateCurrentMoonState() {
            var latlong = profileService.ActiveProfile.AstrometrySettings;
            var now = DateTime.UtcNow;

            CurrentMoonAltitude = AstroUtil.GetMoonAltitude(now, latlong.Latitude, latlong.Longitude);
        }
    }
}