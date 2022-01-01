#region "copyright"
/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using FluentAssertions;
using FluentAssertions.Specialized;
using FTD2XX_NET;
using Moq;
using NINA.Exceptions;
using NINA.MGEN2.Commands.AppMode;

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NINATest.MGEN.Commands {

    [TestFixture]
    public class GetStarDataCommandTest : CommandTestRunner {
        private Mock<IFTDI> ftdiMock = new Mock<IFTDI>();

        [Test]
        public void ConstructorTest() {
            var sut = new GetStarDataCommand(1);

            sut.CommandCode.Should().Be(0xca);
            sut.AcknowledgeCode.Should().Be(0xca);
            sut.SubCommandCode.Should().Be(0x39);
            sut.RequiredBaudRate.Should().Be(250000);
            sut.Timeout.Should().Be(1000);
        }

        [Test]
        public void Successful_Scenario_Test() {
            byte index = 0x01;
            var posX = new byte[] { 0x33, 0x12 };
            var posY = new byte[] { 0x12, 0x33 };
            var brightness = new byte[] { 0x01, 0x03 };
            byte pixels = 0x65;
            byte peak = 0x78;
            SetupWrite(ftdiMock, new byte[] { 0xca }, new byte[] { 0x39 }, new byte[] { index });
            SetupRead(ftdiMock, new byte[] { 0xca }, new byte[] { 0x00 }, new byte[] { index }, new byte[] { posX[0], posX[1], posY[0], posY[1], brightness[0], brightness[1], pixels, peak });

            var sut = new GetStarDataCommand(index);
            var result = sut.Execute(ftdiMock.Object);

            result.Success.Should().BeTrue();
            result.PositionX.Should().Be((ushort)((posX[1] << 8) + posX[0]));
            result.PositionY.Should().Be((ushort)((posY[1] << 8) + posY[0]));
            result.Brightness.Should().Be((ushort)((brightness[1] << 8) + brightness[0]));
            result.Pixels.Should().Be(pixels);
            result.Peak.Should().Be(peak);
        }

        [Test]
        [TestCase(0x99, typeof(UnexpectedReturnCodeException))]
        [TestCase(0xf0, typeof(UILockedException))]
        [TestCase(0xf1, typeof(AnotherCommandInProgressException))]
        [TestCase(0xf2, typeof(CameraIsOffException))]
        [TestCase(0xf3, typeof(AutoGuidingActiveException))]
        public void Exception_Test(byte errorCode, Type ex) {
            SetupWrite(ftdiMock, new byte[] { 0xca }, new byte[] { 0x39 });
            SetupRead(ftdiMock, new byte[] { 0xca }, new byte[] { errorCode });

            var sut = new GetStarDataCommand(1);
            Action act = () => sut.Execute(ftdiMock.Object);

            TestDelegate test = new TestDelegate(act);

            MethodInfo method = typeof(Assert).GetMethod("Throws", new[] { typeof(TestDelegate) });
            MethodInfo generic = method.MakeGenericMethod(ex);

            generic.Invoke(this, new object[] { test });
        }
    }
}