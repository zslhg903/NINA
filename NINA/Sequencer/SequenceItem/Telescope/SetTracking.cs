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
using NINA.Model;
using NINA.Model.MyTelescope;
using NINA.Sequencer.Exceptions;
using NINA.Sequencer.Validations;
using NINA.Utility.Mediator.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Sequencer.SequenceItem.Telescope {

    [ExportMetadata("Name", "Lbl_SequenceItem_Telescope_SetTracking_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Telescope_SetTracking_Description")]
    [ExportMetadata("Icon", "SpeedometerSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Telescope")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    internal class SetTracking : SequenceItem, IValidatable {
        private static readonly IList<TrackingMode> trackingModeChoices;

        static SetTracking() {
            var trackingModeChoicesBuilder = ImmutableList.CreateBuilder<TrackingMode>();
            trackingModeChoicesBuilder.Add(TrackingMode.Sidereal);
            trackingModeChoicesBuilder.Add(TrackingMode.King);
            trackingModeChoicesBuilder.Add(TrackingMode.Solar);
            trackingModeChoicesBuilder.Add(TrackingMode.Lunar);
            trackingModeChoices = trackingModeChoicesBuilder.ToImmutable();
        }

        [ImportingConstructor]
        public SetTracking(ITelescopeMediator telescopeMediator) {
            this.telescopeMediator = telescopeMediator;
        }

        private ITelescopeMediator telescopeMediator;
        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = value;
                RaisePropertyChanged();
            }
        }

        private TrackingMode trackingMode = TrackingMode.Sidereal;

        [JsonProperty]
        public TrackingMode TrackingMode {
            get => trackingMode;
            set {
                trackingMode = value;
                RaisePropertyChanged();
            }
        }

        public IList<TrackingMode> TrackingModeChoices {
            get {
                return trackingModeChoices;
            }
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (Validate()) {
                telescopeMediator.SetTrackingMode(TrackingMode);
                return Task.CompletedTask;
            } else {
                throw new SequenceItemSkippedException(string.Join(",", Issues));
            }
        }

        public override object Clone() {
            return new SetTracking(telescopeMediator) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
                TrackingMode = TrackingMode
            };
        }

        public bool Validate() {
            var i = new List<string>();
            if (!telescopeMediator.GetInfo().Connected) {
                i.Add(Locale.Loc.Instance["LblTelescopeNotConnected"]);
            } else if (!(telescopeMediator.GetInfo().TrackingModes?.Contains(TrackingMode) == true)) {
                i.Add(Locale.Loc.Instance["LblTrackingModeNotSupported"]);
            }
            Issues = i;
            return i.Count == 0;
        }

        public override void AfterParentChanged() {
            Validate();
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(SetTracking)}";
        }
    }
}