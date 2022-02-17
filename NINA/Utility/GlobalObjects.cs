﻿#region "copyright"
/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using NINA.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.Utility {
    /// <summary>
    /// This class is a container for objects that have global scope and need to be instantiated at application startup. Bindings must be resolvable from IoCBindings
    /// </summary>
    public class GlobalObjects {
        private readonly IPluggableBehaviorManager pluggableBehaviorManager;

        public GlobalObjects(IPluggableBehaviorManager pluggableBehaviorManager) {
            this.pluggableBehaviorManager = pluggableBehaviorManager;

            this.pluggableBehaviorManager.Initialize();
        }
    }
}
