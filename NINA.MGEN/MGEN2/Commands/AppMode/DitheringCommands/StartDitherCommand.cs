#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;
using NINA.Exceptions;

namespace NINA.MGEN2.Commands.AppMode {

    public class StartDitherCommand : DitheringCommand<MGENResult> {
        public override byte SubCommandCode { get; } = 0x01;

        protected override MGENResult ExecuteSubCommand(IFTDI device) {
            Write(device, SubCommandCode);
            var data = Read(device, 2);
            if (data[0] != 0x00) {
                throw new UnexpectedReturnCodeException();
            }
            return new MGENResult(true);
        }
    }
}