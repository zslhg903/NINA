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
using NINA.Core.Model;
using NINA.Core.MyMessageBox;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Sequencer.SequenceItem.Utility {

    [ExportMetadata("Name", "Lbl_SequenceItem_Utility_MessageBox_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Utility_MessageBox_Description")]
    [ExportMetadata("Icon", "MessageBoxSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Utility")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class MessageBox : SequenceItem {

        [ImportingConstructor]
        public MessageBox() { }

        private MessageBox(MessageBox cloneMe) : base(cloneMe) {
        }

        public override object Clone() {
            return new MessageBox(this) {
                Text = Text
            };
        }

        [JsonProperty]
        public string Text { get; set; }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            MyMessageBox.Show(Text);
            return Task.CompletedTask;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(MessageBox)}, Text: {Text}";
        }
    }
}