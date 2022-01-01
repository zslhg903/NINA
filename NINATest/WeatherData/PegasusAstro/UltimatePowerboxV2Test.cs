#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Moq;
using NINA.Core.Locale;
using NINA.Core.Utility.SerialCommunication;
using NINA.Equipment.SDK.SwitchSDKs.PegasusAstro;
using NINA.Equipment.Equipment.MyWeatherData;
using NINA.Profile.Interfaces;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace NINATest.WeatherData.PegasusAstro {

    [TestFixture]
    public class UltimatePowerboxV2Test {
        private Mock<IProfileService> _mockProfileService;
        private Mock<IPegasusDevice> _mockSdk;
        private UltimatePowerboxV2 _sut;

        [SetUp]
        public async Task Init() {
            _mockProfileService = new Mock<IProfileService>();
            _mockProfileService.SetupProperty(m => m.ActiveProfile.SwitchSettings.Upbv2PortName, "COM3");
            _mockSdk = new Mock<IPegasusDevice>();
            _sut = new UltimatePowerboxV2(_mockProfileService.Object) { Sdk = _mockSdk.Object };
            _mockSdk.Setup(m => m.InitializeSerialPort(It.IsAny<string>(), It.IsAny<object>())).Returns(true);
            _mockSdk.Setup(m => m.SendCommand<FirmwareVersionResponse>(It.IsAny<FirmwareVersionCommand>()))
                .Returns(Task.FromResult(new FirmwareVersionResponse { DeviceResponse = "1.3" }));
            await _sut.Connect(new CancellationToken());
        }

        [Test]
        public void TestConstructor() {
            _mockProfileService.SetupProperty(m => m.ActiveProfile.SwitchSettings.Upbv2PortName, null);
            var sut = new UltimatePowerboxV2(_mockProfileService.Object) { Sdk = _mockSdk.Object };
            Assert.That(sut.PortName, Is.EqualTo("AUTO"));
        }

        [Test]
        [TestCase(true, "1.3", true, "Ultimate Powerbox V2 on port COM3. Firmware version: ")]
        [TestCase(false, "1.3", false)]
        public async Task TestConnectAsync(bool expected, string commandString = "1.3", bool portAvailable = true, string description = null) {
            _mockSdk.Setup(m => m.InitializeSerialPort(It.IsAny<string>(), It.IsAny<object>())).Returns(portAvailable);
            _mockSdk.Setup(m => m.SendCommand<FirmwareVersionResponse>(It.IsAny<FirmwareVersionCommand>()))
                .Returns(Task.FromResult(new FirmwareVersionResponse { DeviceResponse = commandString }));
            var sut = new UltimatePowerboxV2(_mockProfileService.Object) { Sdk = _mockSdk.Object };
            var result = await sut.Connect(new CancellationToken());
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(sut.Connected, Is.EqualTo(expected));
            if (!expected) return;
            Assert.That(double.TryParse(commandString, NumberStyles.Float, CultureInfo.InvariantCulture, out var version), Is.True);
            Assert.That(sut.Description, Is.EqualTo($"{description}{version}"));
        }

        [Test]
        public async Task TestConnectAsyncInvalidFirmwareResponse() {
            _mockSdk.Setup(m => m.InitializeSerialPort(It.IsAny<string>(), It.IsAny<object>())).Returns(true);
            _mockSdk.Setup(m => m.SendCommand<FirmwareVersionResponse>(It.IsAny<FirmwareVersionCommand>()))
                .Throws(new InvalidDeviceResponseException());
            var sut = new UltimatePowerboxV2(_mockProfileService.Object) { Sdk = _mockSdk.Object };
            var result = await sut.Connect(new CancellationToken());
            Assert.That(result, Is.True);
            Assert.That(sut.Connected, Is.True);
            Assert.That(sut.Description, Is.EqualTo("Ultimate Powerbox V2 on port COM3. Firmware version: " + Loc.Instance["LblNoValidFirmwareVersion"]));
        }

        [Test]
        public async Task TestConnectSerialPortClosed() {
            _mockSdk.Setup(m => m.SendCommand<FirmwareVersionResponse>(It.IsAny<FirmwareVersionCommand>()))
                .Throws(new SerialPortClosedException());
            var sut = new UltimatePowerboxV2(_mockProfileService.Object) { Sdk = _mockSdk.Object };
            var result = await sut.Connect(new CancellationToken());
            Assert.That(result, Is.False);
            Assert.That(sut.Connected, Is.False);
        }

        [Test]
        public void TestDisconnect() {
            _sut.Disconnect();
            _mockSdk.Verify(m => m.Dispose(It.IsAny<object>()), Times.Once);
            Assert.That(_sut.Connected, Is.False);
        }

        [Test]
        public void TestDewPoint() {
            _mockSdk.Setup(m => m.SendCommand<StatusResponse>(It.IsAny<StatusCommand>()))
                .Returns(Task.FromResult(new StatusResponse { DeviceResponse = "UPB:12.2:0.2:2:23.2:59:14.7:1111:111111:0:0:0:0:0:0:0:0:0:0:0000000:0" }));
            var result = _sut.DewPoint;
            Assert.That(result, Is.EqualTo(14.7));
        }

        [Test]
        public void TestDewPointInvalidResponse() {
            _mockSdk.Setup(m => m.SendCommand<StatusResponse>(It.IsAny<StatusCommand>()))
                .Throws(new InvalidDeviceResponseException());
            var result = _sut.DewPoint;
            Assert.That(result, Is.EqualTo(double.NaN));
        }

        [Test]
        public void TestDewTemperature() {
            _mockSdk.Setup(m => m.SendCommand<StatusResponse>(It.IsAny<StatusCommand>()))
                .Returns(Task.FromResult(new StatusResponse { DeviceResponse = "UPB:12.2:0.2:2:23.2:59:14.7:1111:111111:0:0:0:0:0:0:0:0:0:0:0000000:0" }));
            var result = _sut.Temperature;
            Assert.That(result, Is.EqualTo(23.2));
        }

        [Test]
        public void TestDewTemperatureInvalidResponse() {
            _mockSdk.Setup(m => m.SendCommand<StatusResponse>(It.IsAny<StatusCommand>()))
                .Throws(new InvalidDeviceResponseException());
            var result = _sut.Temperature;
            Assert.That(result, Is.EqualTo(double.NaN));
        }

        [Test]
        public void TestHumidity() {
            _mockSdk.Setup(m => m.SendCommand<StatusResponse>(It.IsAny<StatusCommand>()))
                .Returns(Task.FromResult(new StatusResponse { DeviceResponse = "UPB:12.2:0.2:2:23.2:59:14.7:1111:111111:0:0:0:0:0:0:0:0:0:0:0000000:0" }));
            var result = _sut.Humidity;
            Assert.That(result, Is.EqualTo(59d));
        }

        [Test]
        public void TestHumidityInvalidResponse() {
            _mockSdk.Setup(m => m.SendCommand<StatusResponse>(It.IsAny<StatusCommand>()))
                .Throws(new InvalidDeviceResponseException());
            var result = _sut.Humidity;
            Assert.That(result, Is.EqualTo(double.NaN));
        }
    }
}