#region "copyright"
/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NINA.Equipment.Equipment.MyFlatDevice;
using NINA.Profile.Interfaces;
using NUnit.Framework;

namespace NINATest.FlatDevice {

    [TestFixture]
    public class AlnitakFlipFlatSimulatorTest {
        private AlnitakFlipFlatSimulator _sut;
        private Mock<IProfileService> _mockProfileService;

        [SetUp]
        public void Init() {
            _mockProfileService = new Mock<IProfileService>();
            _sut = new AlnitakFlipFlatSimulator(_mockProfileService.Object);
        }

        [TearDown]
        public void Dispose() {
            _sut.Disconnect();
            Assert.That(_sut.Connected, Is.False);
        }

        [Test]
        public async Task TestConnect() {
            Assert.That(await _sut.Connect(new CancellationToken()), Is.True);
        }

        [Test]
        public async Task TestOpen() {
            Assert.That(await _sut.Open(new CancellationToken()), Is.True);
        }

        [Test]
        public async Task TestClose() {
            Assert.That(await _sut.Close(new CancellationToken()), Is.True);
        }

        [Test]
        public void TestLightOnNotConnected() {
            _sut.LightOn = true;
            Assert.That(_sut.LightOn, Is.False);
        }

        [Test]
        public async Task TestLightOnFreshInstance() {
            await _sut.Connect(new CancellationToken());
            _sut.LightOn = true;
            Assert.That(_sut.LightOn, Is.False);
        }

        [Test]
        public async Task TestLightOnCoverOpen() {
            await _sut.Connect(new CancellationToken());
            await _sut.Open(new CancellationToken());
            _sut.LightOn = true;
            Assert.That(_sut.LightOn, Is.False);
        }

        [Test]
        public async Task TestLightOnCoverClosed() {
            await _sut.Connect(new CancellationToken());
            await _sut.Close(new CancellationToken());
            _sut.LightOn = true;
            Assert.That(_sut.LightOn, Is.True);
        }

        [Test]
        [TestCase(-3, 0)]
        [TestCase(0, 0)]
        [TestCase(5, 5)]
        [TestCase(255, 255)]
        [TestCase(1000, 255)]
        public async Task TestBrightness(int setValue, int expectedValue) {
            await _sut.Connect(new CancellationToken());
            _sut.Brightness = setValue;
            Assert.That(_sut.Brightness, Is.EqualTo(expectedValue));
        }
    }
}