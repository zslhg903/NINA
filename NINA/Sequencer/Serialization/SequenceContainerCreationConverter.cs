﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Sequencer.Container;
using System;
using Newtonsoft.Json.Linq;

namespace NINA.Sequencer.Serialization {

    public class SequenceContainerCreationConverter : JsonCreationConverter<ISequenceContainer> {
        private SequencerFactory factory;

        public SequenceContainerCreationConverter(SequencerFactory factory) {
            this.factory = factory;
        }

        public override ISequenceContainer Create(Type objectType, JObject jObject) {
            var t = Type.GetType(jObject.GetValue("$type").ToString());
            var method = factory.GetType().GetMethod(nameof(factory.GetContainer)).MakeGenericMethod(new Type[] { t });
            var obj = method.Invoke(factory, null);
            return (ISequenceContainer)obj;
        }
    }
}