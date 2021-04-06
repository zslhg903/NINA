﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Sequencer.DragDrop;
using System;
using System.Windows.Media;

namespace NINA.Sequencer {

    public interface ISequenceEntity : ICloneable, IDroppable {
        string Name { get; set; }

        /// <summary>
        /// A brief description of what the sequence item does
        /// </summary>
        string Description { get; set; }

        GeometryGroup Icon { get; set; }
        string Category { get; set; }

        /// <summary>
        /// Indicator that the item is currently active and running
        /// </summary>
        SequenceEntityStatus Status { get; }
    }
}