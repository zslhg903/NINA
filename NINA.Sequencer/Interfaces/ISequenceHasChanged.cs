﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace NINA.Sequencer {

    public interface ISequenceHasChanged {

        /// <summary>
        /// Indicator that item or child items have been modified
        /// </summary>
        ///
        bool HasChanged { get; set; }

        /// <summary>
        /// Clear HasChanged in item and child items
        /// </summary>
        ///
        void ClearHasChanged();

        /// <summary>
        /// If HasChanged ask user to proceed
        /// Returns true if HasChanged and user says not to proceed
        /// </summary>
        ///
        bool AskHasChanged(string name);
    }
}