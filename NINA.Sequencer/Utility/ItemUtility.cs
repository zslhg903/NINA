﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Sequencer.Container;
using NINA.Astrometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.Sequencer.Utility {

    public class ItemUtility {

        public static (Coordinates, double) RetrieveContextCoordinates(ISequenceContainer parent) {
            if (parent != null) {
                var container = parent as IDeepSkyObjectContainer;
                if (container != null) {
                    return (container.Target.InputCoordinates.Coordinates.Transform(container.Target.InputCoordinates.Coordinates.Epoch), container.Target.Rotation);
                } else {
                    return RetrieveContextCoordinates(parent.Parent);
                }
            } else {
                return (null, 0);
            }
        }

        public static bool IsInRootContainer(ISequenceContainer parent) {
            return GetRootContainer(parent) != null;
        }

        public static ISequenceRootContainer GetRootContainer(ISequenceContainer parent) {
            if (parent != null) {
                if (parent is ISequenceRootContainer rootContainer) {
                    return rootContainer;
                }
                return GetRootContainer(parent.Parent);
            } else {
                return null;
            }
        }
    }
}