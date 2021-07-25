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
using NINA.Equipment.Interfaces.Mediator;
using NINA.Core.Model;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.SequenceItem.Guider;
using NINA.Sequencer.Validations;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Locale;

namespace NINA.Sequencer.Trigger.Guider {

    [ExportMetadata("Name", "Lbl_SequenceTrigger_Guider_DitherAfterExposures_Name")]
    [ExportMetadata("Description", "Lbl_SequenceTrigger_Guider_DitherAfterExposures_Description")]
    [ExportMetadata("Icon", "DitherSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Guider")]
    [Export(typeof(ISequenceTrigger))]
    [JsonObject(MemberSerialization.OptIn)]
    public class DitherAfterExposures : SequenceTrigger, IValidatable {
        private IGuiderMediator guiderMediator;
        private IImageHistoryVM history;

        [ImportingConstructor]
        public DitherAfterExposures(IGuiderMediator guiderMediator, IImageHistoryVM history) : base() {
            this.guiderMediator = guiderMediator;
            this.history = history;
            AfterExposures = 1;
            TriggerRunner.Add(new Dither(guiderMediator));
        }

        private DitherAfterExposures(DitherAfterExposures cloneMe) : this(cloneMe.guiderMediator, cloneMe.history) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new DitherAfterExposures(this) {
                AfterExposures = AfterExposures,
                TriggerRunner = (SequentialContainer)TriggerRunner.Clone()
            };
        }

        private int lastTriggerId = 0;
        private int afterExposures;

        [JsonProperty]
        public int AfterExposures {
            get => afterExposures;
            set {
                afterExposures = value;
                RaisePropertyChanged();
            }
        }

        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = ImmutableList.CreateRange(value);
                RaisePropertyChanged();
            }
        }

        public int ProgressExposures {
            get => AfterExposures > 0 ? history.ImageHistory.Count % AfterExposures : 0;
        }

        public override async Task Execute(ISequenceContainer context, IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (AfterExposures > 0) {
                lastTriggerId = history.ImageHistory.Count;
                await TriggerRunner.Run(progress, token);
            } else {
                return;
            }
        }

        public override bool ShouldTrigger(ISequenceItem previousItem, ISequenceItem nextItem) {
            if (nextItem == null) { return false; }
            RaisePropertyChanged(nameof(ProgressExposures));
            return lastTriggerId < history.ImageHistory.Count && history.ImageHistory.Count > 0 && ProgressExposures == 0;
        }

        public override string ToString() {
            return $"Trigger: {nameof(DitherAfterExposures)}, After Exposures: {AfterExposures}";
        }

        public bool Validate() {
            var i = new List<string>();
            var info = guiderMediator.GetInfo();

            if (!info.Connected) {
                i.Add(Loc.Instance["LblGuiderNotConnected"]);
            }

            Issues = i;
            return i.Count == 0;
        }
    }
}