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
using Moq;
using NINA.Image.ImageData;
using NINA.Equipment.Equipment.MyFilterWheel;
using NINA.PlateSolving;
using NINA.Equipment.Interfaces.Mediator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Utility;
using NINA.Image.Interfaces;
using NINA.Equipment.Model;
using NINA.Core.Model;
using NINA.Core.Model.Equipment;
using NINA.PlateSolving.Interfaces;

namespace NINATest.PlateSolving {

    [TestFixture]
    public class CaptureSolverTest {
        private Mock<IPlateSolver> plateSolverMock;
        private Mock<IPlateSolver> blindSolverMock;
        private Mock<IImagingMediator> imagingMediatorMock;
        private Mock<IImageSolver> imageSolverMock;
        private Mock<IFilterWheelMediator> filterMediatorMock;

        [SetUp]
        public void Setup() {
            plateSolverMock = new Mock<IPlateSolver>();
            blindSolverMock = new Mock<IPlateSolver>();
            imagingMediatorMock = new Mock<IImagingMediator>();
            imageSolverMock = new Mock<IImageSolver>();
            filterMediatorMock = new Mock<IFilterWheelMediator>();
        }

        [Test]
        public async Task Successful_CaptureAndSolving_Test() {
            var imageDataMock = new Mock<IImageData>();
            var renderedImageMock = new Mock<IRenderedImage>();
            renderedImageMock.SetupGet(x => x.RawImageData).Returns(imageDataMock.Object);
            var testResult = new PlateSolveResult() {
                Success = true
            };
            var seq = new CaptureSequence();
            var parameter = new CaptureSolverParameter() { FocalLength = 700 };

            imagingMediatorMock.Setup(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>())).ReturnsAsync(renderedImageMock.Object);
            imageSolverMock.Setup(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>())).ReturnsAsync(testResult);

            var sut = new CaptureSolver(plateSolverMock.Object, blindSolverMock.Object, imagingMediatorMock.Object, filterMediatorMock.Object);
            sut.ImageSolver = imageSolverMock.Object;

            var result = await sut.Solve(seq, parameter, default, default, default);

            result.Success.Should().BeTrue();
            imagingMediatorMock.Verify(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()), Times.Once());
            imageSolverMock.Verify(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task Successful_CaptureAndSolving_WithReattempt_Test() {
            var imageDataMock = new Mock<IImageData>();
            var renderedImageMock = new Mock<IRenderedImage>();
            renderedImageMock.SetupGet(x => x.RawImageData).Returns(imageDataMock.Object);
            var initialFilter = new FilterInfo() { Name = "L", Position = 1 };
            filterMediatorMock.Setup(x => x.GetInfo()).Returns(new FilterWheelInfo() { SelectedFilter = initialFilter });
            var failedResult = new PlateSolveResult() {
                Success = false
            };
            var testResult = new PlateSolveResult() {
                Success = true
            };
            var seq = new CaptureSequence();
            var parameter = new CaptureSolverParameter() { FocalLength = 700, Attempts = 5, ReattemptDelay = TimeSpan.FromMilliseconds(5) };

            imagingMediatorMock.Setup(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>())).ReturnsAsync(renderedImageMock.Object);
            imageSolverMock
                .SetupSequence(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult)
                .ReturnsAsync(failedResult)
                .ReturnsAsync(testResult);

            var sut = new CaptureSolver(plateSolverMock.Object, blindSolverMock.Object, imagingMediatorMock.Object, filterMediatorMock.Object);
            sut.ImageSolver = imageSolverMock.Object;

            var result = await sut.Solve(seq, parameter, default, default, default);

            result.Success.Should().BeTrue();
            imagingMediatorMock.Verify(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()), Times.Exactly(3));
            imageSolverMock.Verify(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            filterMediatorMock.Verify(x => x.ChangeFilter(It.Is<FilterInfo>(f => f == initialFilter), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()), Times.AtLeastOnce());
        }

        [Test]
        public async Task Unsuccessful_Solving_WithReattempt_Test() {
            var imageDataMock = new Mock<IImageData>();
            var renderedImageMock = new Mock<IRenderedImage>();
            renderedImageMock.SetupGet(x => x.RawImageData).Returns(imageDataMock.Object);
            var failedResult = new PlateSolveResult() {
                Success = false
            };
            var testResult = new PlateSolveResult() {
                Success = true
            };
            var seq = new CaptureSequence();
            var parameter = new CaptureSolverParameter() { FocalLength = 700, Attempts = 3, ReattemptDelay = TimeSpan.FromMilliseconds(5) };

            imagingMediatorMock.Setup(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>())).ReturnsAsync(renderedImageMock.Object);
            imageSolverMock
                .SetupSequence(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult)
                .ReturnsAsync(failedResult)
                .ReturnsAsync(failedResult);

            var sut = new CaptureSolver(plateSolverMock.Object, blindSolverMock.Object, imagingMediatorMock.Object, filterMediatorMock.Object);
            sut.ImageSolver = imageSolverMock.Object;

            var result = await sut.Solve(seq, parameter, default, default, default);

            result.Success.Should().BeFalse();
            imagingMediatorMock.Verify(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()), Times.Exactly(3));
            imageSolverMock.Verify(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Test]
        public async Task Unsuccessful_Capture_WithReattempt_Test() {
            var imageDataMock = new Mock<IImageData>();
            var renderedImageMock = new Mock<IRenderedImage>();
            renderedImageMock.SetupGet(x => x.RawImageData).Returns(imageDataMock.Object);
            var testResult = new PlateSolveResult() {
                Success = true
            };
            var seq = new CaptureSequence();
            var parameter = new CaptureSolverParameter() { FocalLength = 700, Attempts = 3, ReattemptDelay = TimeSpan.FromMilliseconds(5) };

            imagingMediatorMock
                .Setup(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()))
                .ReturnsAsync((IRenderedImage)null);
            imageSolverMock
                .Setup(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testResult);

            var sut = new CaptureSolver(plateSolverMock.Object, blindSolverMock.Object, imagingMediatorMock.Object, filterMediatorMock.Object);
            sut.ImageSolver = imageSolverMock.Object;

            var result = await sut.Solve(seq, parameter, default, default, default);

            result.Success.Should().BeFalse();
            imagingMediatorMock.Verify(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()), Times.Exactly(3));
            imageSolverMock.Verify(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Exactly(0));
        }

        [Test]
        public async Task Successful_AfterTwoFailedCaptures_WithReattempt_Test() {
            var imageDataMock = new Mock<IImageData>();
            var renderedImageMock = new Mock<IRenderedImage>();
            renderedImageMock.SetupGet(x => x.RawImageData).Returns(imageDataMock.Object);
            var testResult = new PlateSolveResult() {
                Success = true
            };
            var seq = new CaptureSequence();
            var parameter = new CaptureSolverParameter() { FocalLength = 700, Attempts = 5, ReattemptDelay = TimeSpan.FromMilliseconds(5) };

            imagingMediatorMock
                .SetupSequence(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()))
                .ReturnsAsync((IRenderedImage)null)
                .ReturnsAsync((IRenderedImage)null)
                .ReturnsAsync(renderedImageMock.Object);
            imageSolverMock
                .Setup(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testResult);

            var sut = new CaptureSolver(plateSolverMock.Object, blindSolverMock.Object, imagingMediatorMock.Object, filterMediatorMock.Object);
            sut.ImageSolver = imageSolverMock.Object;

            var result = await sut.Solve(seq, parameter, default, default, default);

            result.Success.Should().BeTrue();
            imagingMediatorMock.Verify(x => x.CaptureAndPrepareImage(seq, It.IsAny<PrepareImageParameters>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<ApplicationStatus>>()), Times.Exactly(3));
            imageSolverMock.Verify(x => x.Solve(imageDataMock.Object, It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }
    }
}