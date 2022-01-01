﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using NINA.Sequencer.SequenceItem;
using Newtonsoft.Json.Linq;
using NINA.Core.Utility;

namespace NINA.Sequencer.Serialization {

    public class SequenceItemCreationConverter : JsonCreationConverter<ISequenceItem> {
        private ISequencerFactory factory;
        private SequenceContainerCreationConverter sequenceContainerCreationConverter;

        public SequenceItemCreationConverter(ISequencerFactory factory, SequenceContainerCreationConverter sequenceContainerCreationConverter) {
            this.factory = factory;
            this.sequenceContainerCreationConverter = sequenceContainerCreationConverter;
        }

        public override ISequenceItem Create(Type objectType, JObject jObject) {
            if (jObject.SelectToken("Strategy.$type") != null) {
                return sequenceContainerCreationConverter.Create(objectType, jObject);
            }

            if (jObject.TryGetValue("$type", out var token)) {
                var t = GetType(token.ToString());
                try {
                    var method = factory.GetType().GetMethod(nameof(factory.GetItem)).MakeGenericMethod(new Type[] { t });
                    var obj = method.Invoke(factory, null);
                    return (ISequenceItem)obj;
                } catch (Exception e) {
                    Logger.Error($"Encountered unknown sequence item: {token?.ToString()}", e);
                    return new UnknownSequenceItem(token?.ToString());
                }
            } else {
                return new UnknownSequenceItem(token?.ToString());
            }
        }
    }
}