#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using FTD2XX_NET;
using NINA.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.MGEN2.Commands.AppMode {

    public class NoOpCommand : AppModeCommand<MGENResult> {
        public override byte CommandCode { get; } = 0xff;
        public override byte AcknowledgeCode { get; } = 0xff;
        public byte NotAcknowledgeCode { get; } = 0x00;

        public override MGENResult Execute(IFTDI device) {
            Write(device, CommandCode);
            var data = Read(device, 1);
            if (data[0] != AcknowledgeCode && data[0] != NotAcknowledgeCode) {
                return null;
            }
            return new MGENResult(data[0] == AcknowledgeCode);
        }
    }
}