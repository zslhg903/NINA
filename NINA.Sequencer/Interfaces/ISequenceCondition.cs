﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Sequencer.SequenceItem;
using System.Windows.Input;

namespace NINA.Sequencer.Conditions {

    public interface ISequenceCondition : ISequenceEntity, ISequenceHasChanged {

        /// <summary>
        /// Determine if the condition is satisfied or not. Will be called after each processed sequence item.
        /// </summary>
        /// <param name="nextItem">The next item that is scheduled to be processed</param>
        /// <returns></returns>
        bool Check(ISequenceItem nextItem);

        /// <summary>
        /// When the sequencer is started this method is called
        /// </summary>
        void Initialize();

        /// <summary>
        /// When the sequencer is stopped or canceled this method is called
        /// </summary>
        void Teardown();

        /// <summary>
        /// When a sequence container is entered this method is called
        /// </summary>
        void SequenceBlockInitialize();

        /// <summary>
        /// When a sequence container is finished this method is called
        /// </summary>
        void SequenceBlockTeardown();

        /// <summary>
        /// Each time the sequence container starts a loop this is called
        /// </summary>
        void SequenceBlockStarted();

        /// <summary>
        /// Each time the sequence container finishes a loop this is called
        /// </summary>
        void SequenceBlockFinished();

        /// <summary>
        /// Resets the progress of the condition. For example when nested in a container that restarts itself the condition needs to be reset
        /// </summary>
        void ResetProgress();

        ICommand ResetProgressCommand { get; }
    }
}