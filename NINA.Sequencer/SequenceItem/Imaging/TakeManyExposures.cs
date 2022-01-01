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
using NINA.Profile.Interfaces;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Validations;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.Sequencer.Utility;

namespace NINA.Sequencer.SequenceItem.Imaging {

    [ExportMetadata("Name", "Lbl_SequenceItem_Imaging_TakeManyExposures_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Imaging_TakeManyExposures_Description")]
    [ExportMetadata("Icon", "CameraSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Camera")]
    [Export(typeof(ISequenceItem))]
    [Export(typeof(ISequenceContainer))]
    [JsonObject(MemberSerialization.OptIn)]
    public class TakeManyExposures : SequentialContainer, IImmutableContainer {

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context) {
            this.Items.Clear();
            this.Conditions.Clear();
            this.Triggers.Clear();
        }

        [ImportingConstructor]
        public TakeManyExposures(IProfileService profileService, ICameraMediator cameraMediator, IImagingMediator imagingMediator, IImageSaveMediator imageSaveMediator, IImageHistoryVM imageHistoryVM) :
                this(
                    null,
                    new TakeExposure(profileService, cameraMediator, imagingMediator, imageSaveMediator, imageHistoryVM),
                    new LoopCondition() { Iterations = 1 }) {
        }

        private InstructionErrorBehavior errorBehavior = InstructionErrorBehavior.ContinueOnError;

        [JsonProperty]
        public override InstructionErrorBehavior ErrorBehavior {
            get => errorBehavior;
            set {
                errorBehavior = value;
                foreach (var item in Items) {
                    item.ErrorBehavior = errorBehavior;
                }
                RaisePropertyChanged();
            }
        }

        private int attempts = 1;

        [JsonProperty]
        public override int Attempts {
            get => attempts;
            set {
                if (value > 0) {
                    attempts = value;
                    foreach (var item in Items) {
                        item.Attempts = attempts;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Clone Constructor
        /// </summary>
        private TakeManyExposures(
                TakeManyExposures cloneMe, TakeExposure takeExposure, LoopCondition loopCondition) {
            this.Add(takeExposure);
            this.Add(loopCondition);

            IsExpanded = false;

            if (cloneMe != null) {
                CopyMetaData(cloneMe);
            }
        }

        public TakeExposure GetTakeExposure() {
            return Items[0] as TakeExposure;
        }

        public LoopCondition GetLoopCondition() {
            return Conditions[0] as LoopCondition;
        }

        public override bool Validate() {
            var item = GetTakeExposure();
            var valid = item.Validate();
            Issues = item.Issues;
            RaisePropertyChanged(nameof(Issues));
            return valid;
        }

        public override object Clone() {
            var clone = new TakeManyExposures(
                    this,
                    (TakeExposure)this.GetTakeExposure().Clone(),
                    (LoopCondition)this.GetLoopCondition().Clone());
            return clone;
        }

        public override TimeSpan GetEstimatedDuration() {
            return GetTakeExposure().GetEstimatedDuration();
        }

        /// <summary>
        /// When an inner instruction interrupts this set, it should reroute the interrupt to the real parent set
        /// </summary>
        /// <returns></returns>
        public override Task Interrupt() {
            return this.Parent?.Interrupt();
        }
    }
}