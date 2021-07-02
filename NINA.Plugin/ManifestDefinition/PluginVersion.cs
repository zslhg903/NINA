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
using NINA.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.Plugin.ManifestDefinition {

    [Serializable]
    public class PluginVersion : IPluginVersion {

        public PluginVersion() {
        }

        public PluginVersion(string version) {
            var v = new Version(version);
            Major = v.Major;
            Minor = v.Minor;
            Patch = v.Build;
            Build = v.Revision;
        }

        [JsonProperty(Required = Required.Always)]
        public int Major { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Minor { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Patch { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Build { get; set; }

        public override string ToString() {
            return $"{Major}.{Minor}.{Patch}.{Build}";
        }
    }
}