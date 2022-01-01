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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.MGEN2.Commands.AppMode {

    public class ReadDisplayCommand : IOCommand<DisplayData> {
        private ushort address;
        private byte chunkSize;

        public ReadDisplayCommand(ushort address, byte chunkSize) {
            this.address = address;
            this.chunkSize = chunkSize;
        }

        public override byte SubCommandCode { get; } = 0x0d;

        protected override DisplayData ExecuteSubCommand(IFTDI device) {
            Write(device, SubCommandCode);
            Write(device, new byte[] { (byte)address, (byte)(address >> 8), chunkSize });
            var displayData = Read(device, chunkSize);
            return new DisplayData(displayData);
        }
    }

    public class DisplayData : MGENResult {

        public DisplayData(byte[] data) : base(true) {
            Data = data;
        }

        public byte[] Data { get; private set; }
    }
}