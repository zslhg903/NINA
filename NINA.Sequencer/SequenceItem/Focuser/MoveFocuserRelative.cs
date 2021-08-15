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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Locale;

namespace NINA.Sequencer.SequenceItem.Focuser {

    [ExportMetadata("Name", "Lbl_SequenceItem_Focuser_MoveFocuserRelative_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Focuser_MoveFocuserRelative_Description")]
    [ExportMetadata("Icon", "MoveFocuserRelativeSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Focuser")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class MoveFocuserRelative : SequenceItem, IValidatable {

        [ImportingConstructor]
        public MoveFocuserRelative(IFocuserMediator focuserMediator) {
            this.focuserMediator = focuserMediator;
        }

        private MoveFocuserRelative(MoveFocuserRelative cloneMe) : this(cloneMe.focuserMediator) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new MoveFocuserRelative(this) {
                RelativePosition = RelativePosition
            };
        }

        private IFocuserMediator focuserMediator;

        private int relativePosition = 0;

        [JsonProperty]
        public int RelativePosition {
            get => relativePosition;
            set {
                relativePosition = value;
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

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            return focuserMediator.MoveFocuserRelative(RelativePosition, token);
        }

        public bool Validate() {
            var i = new List<string>();
            if (!focuserMediator.GetInfo().Connected) {
                i.Add(Loc.Instance["LblFocuserNotConnected"]);
            }
            Issues = i;
            return i.Count == 0;
        }

        public override void AfterParentChanged() {
            Validate();
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(MoveFocuserRelative)}, Relative Position: {RelativePosition}";
        }
    }
}