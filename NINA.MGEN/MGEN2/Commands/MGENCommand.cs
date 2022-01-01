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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.MGEN2.Commands {

    public abstract class MGENCommand<TResult> : IMGENCommand<TResult> where TResult : IMGENResult {
        public abstract uint RequiredBaudRate { get; }
        public virtual uint Timeout { get; } = 1000;
        public abstract byte CommandCode { get; }
        public abstract byte AcknowledgeCode { get; }

        public abstract TResult Execute(IFTDI device);

        protected void Write(IFTDI device, byte[] data) {
            ValidateDeviceParameter(device);
            var status = device.Write(data, data.Length, out var writtenBytes);
            if (status != FT_STATUS.FT_OK) {
                throw new FTDIWriteException();
            }
        }

        protected void Write(IFTDI device, byte data) {
            ValidateDeviceParameter(device);
            var command = new byte[] { data };
            var status = device.Write(command, command.Length, out var writtenBytes);
            if (status != FT_STATUS.FT_OK) {
                throw new FTDIWriteException();
            }
        }

        protected byte[] Read(IFTDI device, int length) {
            ValidateDeviceParameter(device);

            byte[] buffer = new byte[length];
            var status = device.Read(buffer, buffer.Length, out var readBytes);
            if (status != FT_STATUS.FT_OK) {
                throw new FTDIReadException();
            }
            return buffer;
        }

        private void ValidateDeviceParameter(IFTDI device) {
            device.SetBaudRate(RequiredBaudRate);
            device.SetTimeouts(Timeout, Timeout);
        }

        protected short ToShort(byte first, byte second) {
            var prepared = new byte[] { first, second };
            if (!BitConverter.IsLittleEndian) { Array.Reverse(prepared); }
            return BitConverter.ToInt16(prepared, 0);
        }

        protected ushort ToUShort(byte first, byte second) {
            var prepared = new byte[] { first, second };
            if (!BitConverter.IsLittleEndian) { Array.Reverse(prepared); }
            return BitConverter.ToUInt16(prepared, 0);
        }

        protected int ThreeBytesToInt(byte first, byte second, byte third) {
            var isNegative = (third >> 7) == 1;
            var prepared = new byte[] { first, second, third, isNegative ? (byte)0xff : (byte)0x00 };
            if (!BitConverter.IsLittleEndian) { Array.Reverse(prepared); }
            return BitConverter.ToInt32(prepared, 0);
        }

        protected byte[] GetBytes(ushort value) {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) { Array.Reverse(bytes); }
            return bytes;
        }
    }
}