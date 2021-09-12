#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using FluentAssertions;
using Moq;
using NINA.Core.Model;
using NINA.Equipment.Equipment.MyGuider;
using NINA.Equipment.Equipment.MyGuider.PHD2;
using NINA.Equipment.Interfaces;
using NUnit.Framework;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace NINATest {

    [TestFixture]
    public class SynchronizedPHD2GuiderServiceTest {
        private SynchronizedPHD2GuiderService sut = new SynchronizedPHD2GuiderService();
        private Mock<IGuider> guider;

        [SetUp]
        public void Init() {
            guider = new Mock<IGuider>(MockBehavior.Strict);
            guider.Setup(m => m.Connect(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        [Test]
        public async Task Initialize_WhenInitializing_ConnectToPhd2() {
            // setup
            CancellationTokenSource cts = new CancellationTokenSource();
            var localguider = new Mock<IGuider>(MockBehavior.Strict);
            guider.Setup(m => m.Connect(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // act
            var result = await sut.Initialize(guider.Object, cts.Token);

            // assert
            result.Should().Be(false);
            guider.Verify(m => m.Connect(cts.Token), Times.Once);
        }

        [Test]
        public async Task ConnectAndGetPixelScale_WhenNotExistingId_AddToClientInfo() {
            // setup
            Guid id = Guid.NewGuid();
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            guider.Setup(m => m.PixelScale).Returns(1);

            // act
            var result = await sut.ConnectAndGetPixelScale(id);

            // assert
            result.Should().Be(1);
            sut.ConnectedClients.Count.Should().Be(1);
            sut.ConnectedClients[0].InstanceID.Should().Be(id);
        }

        [Test]
        public async Task ConnectAndGetPixelScale_WhenAlreadyConnectedClient_AddToClientInfo() {
            // setup
            Guid id = Guid.NewGuid();
            Guid prevId = Guid.NewGuid();
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            guider.Setup(m => m.PixelScale).Returns(1);
            sut.ConnectedClients.Add(new SynchronizedClientInfo { InstanceID = prevId });

            // act
            var result = await sut.ConnectAndGetPixelScale(id);

            // assert
            result.Should().Be(1);
            sut.ConnectedClients.Count.Should().Be(2);
            sut.ConnectedClients[0].InstanceID.Should().Be(prevId);
            sut.ConnectedClients[1].InstanceID.Should().Be(id);
        }

        [Test]
        public async Task GetGuideInfo_WhenPhd2NotConnected_ThrowPhd2Fault() {
            // setup
            Guid id = Guid.NewGuid();
            CancellationTokenSource cts = new CancellationTokenSource();
            guider.Setup(m => m.Connect(cts.Token)).ReturnsAsync(false);
            await sut.Initialize(guider.Object, cts.Token);

            // act
            Func<Task> act = async () => {
                await sut.GetGuideInfo(id);
            };

            // assert
            await act.Should().ThrowAsync<FaultException<PHD2Fault>>();
        }

        [Test]
        public async Task StartGuiding_WhenCalled_ShouldStartGuidingOnGuideInstance() {
            // setup
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            guider.Setup(m => m.StartGuiding(It.IsAny<bool>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // act
            var result = await sut.StartGuiding(It.IsAny<bool>());

            // assert
            result.Should().BeTrue();
            guider.Verify(m => m.StartGuiding(It.IsAny<bool>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task StopGuiding_WhenCalled_ShouldStopGuidingOnGuideInstance() {
            // setup
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            guider.Setup(m => m.StopGuiding(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // act
            var result = await sut.StopGuiding();

            // assert
            result.Should().BeTrue();
            guider.Verify(m => m.StopGuiding(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SynchronizedDither_WhenClientNextExposureTimeIsNegative_ReturnImmediately() {
            // setup
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            Guid id = Guid.NewGuid();
            sut.ConnectedClients.Add(new SynchronizedClientInfo { InstanceID = id, NextExposureTime = -1 });

            // act
            var result = await sut.SynchronizedDither(id);

            // assert
            result.Should().BeTrue();
            guider.Verify(m => m.Dither(default, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SynchronizedDither_WhenNoOtherClientsExist_DitherImmediately() {
            // setup
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            Guid id = Guid.NewGuid();
            sut.ConnectedClients.Add(new SynchronizedClientInfo { InstanceID = id, NextExposureTime = 10 });
            guider.Setup(m => m.Dither(default, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // act
            var result = await sut.SynchronizedDither(id);

            // assert
            result.Should().BeTrue();
            guider.Verify(m => m.Dither(default, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SynchronizedDither_WhenNoOtherAliveClientsExist_DitherImmediately() {
            // setup
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            Guid id = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            sut.ConnectedClients.Add(new SynchronizedClientInfo { InstanceID = id, NextExposureTime = 10 });
            sut.ConnectedClients.Add(new SynchronizedClientInfo { InstanceID = id2, NextExposureTime = 10, LastPing = DateTime.Now.Subtract(TimeSpan.FromSeconds(5)) });
            guider.Setup(m => m.Dither(default, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // act
            var result = await sut.SynchronizedDither(id);

            // assert
            result.Should().BeTrue();
            guider.Verify(m => m.Dither(default, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SynchronizedDither_WhenAllOtherClientsHaveExposureEndTimeLessThanNow_DitherWhenClientsAreReady() {
            // setup
            CancellationTokenSource cts = new CancellationTokenSource();
            await sut.Initialize(guider.Object, cts.Token);
            Guid id = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            sut.ConnectedClients.Add(new SynchronizedClientInfo { InstanceID = id, NextExposureTime = 10 });
            sut.ConnectedClients.Add(new SynchronizedClientInfo { InstanceID = id2, NextExposureTime = 10, LastPing = DateTime.Now });
            guider.Setup(m => m.Dither(default, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // act
            var result = await sut.SynchronizedDither(id);

            // assert
            result.Should().BeTrue();
            guider.Verify(m => m.Dither(default, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}