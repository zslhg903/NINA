﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Locale;
using NINA.Sequencer.SequenceItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.Sequencer.Conditions {

    internal class UnknownSequenceCondition : SequenceCondition {

        public new string Name {
            get => $"<{Loc.Instance["LblUnknownInstruction"]} - {base.Name}> ";
            private set {
                base.Name = value;
            }
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            return false;
        }

        public override object Clone() {
            return new UnknownSequenceCondition() {
                Name = base.Name
            };
        }
    }
}