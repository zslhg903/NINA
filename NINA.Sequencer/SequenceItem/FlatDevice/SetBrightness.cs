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
using NINA.Sequencer.Validations;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Locale;

namespace NINA.Sequencer.SequenceItem.FlatDevice {

    [ExportMetadata("Name", "Lbl_SequenceItem_FlatDevice_SetBrightness_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_FlatDevice_SetBrightness_Description")]
    [ExportMetadata("Icon", "BrightnessSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_FlatDevice")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SetBrightness : SequenceItem, IValidatable {

        [ImportingConstructor]
        public SetBrightness(IFlatDeviceMediator flatDeviceMediator) {
            this.flatDeviceMediator = flatDeviceMediator;
        }

        private SetBrightness(SetBrightness cloneMe) : this(cloneMe.flatDeviceMediator) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new SetBrightness(this) {
                Brightness = Brightness
            };
        }

        private IFlatDeviceMediator flatDeviceMediator;
        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = value;
                RaisePropertyChanged();
            }
        }

        private int brightness;

        [JsonProperty]
        public int Brightness {
            get => brightness;
            set {
                brightness = value;
                RaisePropertyChanged();
            }
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            return flatDeviceMediator.SetBrightness(Brightness, token);
        }

        public bool Validate() {
            var i = new List<string>();
            var info = flatDeviceMediator.GetInfo();
            if (!info.Connected) {
                i.Add(Loc.Instance["LblFlatDeviceNotConnected"]);
            }
            Issues = i;
            return i.Count == 0;
        }

        public override void AfterParentChanged() {
            Validate();
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(SetBrightness)}";
        }
    }
}