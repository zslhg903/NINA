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
using NINA.Sequencer.SequenceItem;
using NINA.Core.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NINA.Sequencer.Conditions {

    [ExportMetadata("Name", "Lbl_SequenceCondition_LoopCondition_Name")]
    [ExportMetadata("Description", "Lbl_SequenceCondition_LoopCondition_Description")]
    [ExportMetadata("Icon", "LoopSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Condition")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class LoopCondition : SequenceCondition {

        [ImportingConstructor]
        public LoopCondition() {
            Iterations = 2;
        }

        private LoopCondition(LoopCondition cloneMe) : this() {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new LoopCondition(this) {
                Iterations = Iterations
            };
        }

        private int completedIterations;
        private int iterations;

        [JsonProperty]
        public int CompletedIterations {
            get => completedIterations;
            set {
                completedIterations = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public int Iterations {
            get => iterations;
            set {
                iterations = value;
                RaisePropertyChanged();
            }
        }

        public override bool Check(ISequenceItem nextItem) {
            return CompletedIterations < Iterations;
        }

        public override void ResetProgress() {
            CompletedIterations = 0;
        }

        public override void SequenceBlockFinished() {
            CompletedIterations++;
        }

        public override string ToString() {
            return $"Condition: {nameof(LoopCondition)}, Iterations: {CompletedIterations}/{Iterations}";
        }
    }
}