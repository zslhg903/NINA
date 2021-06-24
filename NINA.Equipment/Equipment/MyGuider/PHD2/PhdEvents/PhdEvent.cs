﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility;
using System.Runtime.Serialization;
using NINA.Core.Interfaces;

namespace NINA.Equipment.Equipment.MyGuider.PHD2.PhdEvents {

    [DataContract]
    public class PhdEvent : BaseINPC, IGuideEvent {

        [DataMember]
        public string Event { get; set; }

        [DataMember]
        public string TimeStamp { get; set; }

        [DataMember]
        public string Host { get; set; }

        [DataMember]
        public int Inst { get; set; }
    }
}