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
using NINA.PlateSolving;
using NINA.Astrometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Locale;
using NINA.Image.Interfaces;
using NINA.Core.Model;
using NINA.PlateSolving.Interfaces;

namespace NINATest.PlateSolving {

    [TestFixture]
    public class ImageSolverTest {
        private Mock<IPlateSolver> plateSolverMock;
        private Mock<IPlateSolver> blindSolverMock;

        [SetUp]
        public void Setup() {
            plateSolverMock = new Mock<IPlateSolver>();
            blindSolverMock = new Mock<IPlateSolver>();
        }

        [Test]
        public Task Prerequisites_PlateSolveParameterNull_Validation_Test() {
            var sut = new ImageSolver(plateSolverMock.Object, blindSolverMock.Object);

            Func<Task> f = () => sut.Solve(default, default, default, default);

            return f.Should().ThrowAsync<ArgumentNullException>(nameof(PlateSolveParameter));
        }

        [Test]
        public Task Prerequisites_PlateSolveParameter_FocalLengthMissing_Validation_Test() {
            var sut = new ImageSolver(plateSolverMock.Object, blindSolverMock.Object);

            var parameter = new PlateSolveParameter() { };
            Func<Task> f = () => sut.Solve(default, parameter, default, default);

            return f.Should().ThrowAsync<Exception>(Loc.Instance["LblPlateSolveNoFocalLength"]);
        }

        [Test]
        public async Task Successful_PlateSolve_Test() {
            var testResult = new PlateSolveResult() {
                Success = true
            };
            plateSolverMock
                .Setup(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testResult);

            var sut = new ImageSolver(plateSolverMock.Object, blindSolverMock.Object);
            var parameter = new PlateSolveParameter() {
                FocalLength = 700,
                Coordinates = new Coordinates(Angle.ByDegree(0), Angle.ByDegree(0), Epoch.J2000)
            };

            var result = await sut.Solve(default, parameter, default, default);

            result.Success.Should().BeTrue();
            plateSolverMock.Verify(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Once());
            blindSolverMock.Verify(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async Task Successful_BlindSolve_Test() {
            var testResult = new PlateSolveResult() {
                Success = true
            };
            blindSolverMock
                .Setup(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testResult);

            var sut = new ImageSolver(plateSolverMock.Object, blindSolverMock.Object);
            var parameter = new PlateSolveParameter() {
                FocalLength = 700
            };

            var result = await sut.Solve(default, parameter, default, default);

            result.Success.Should().BeTrue();
            plateSolverMock.Verify(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Never());
            blindSolverMock.Verify(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task Rollover_Solve_Test() {
            var testResult1 = new PlateSolveResult() {
                Success = false
            };
            var testResult2 = new PlateSolveResult() {
                Success = true
            };
            plateSolverMock
                .Setup(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testResult1);
            blindSolverMock
                .Setup(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testResult2);

            var sut = new ImageSolver(plateSolverMock.Object, blindSolverMock.Object);
            var parameter = new PlateSolveParameter() {
                FocalLength = 700,
                Coordinates = new Coordinates(Angle.ByDegree(0), Angle.ByDegree(0), Epoch.J2000)
            };

            var result = await sut.Solve(default, parameter, default, default);

            result.Success.Should().BeTrue();
            plateSolverMock.Verify(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Once());
            blindSolverMock.Verify(x => x.SolveAsync(It.IsAny<IImageData>(), It.IsAny<PlateSolveParameter>(), It.IsAny<IProgress<ApplicationStatus>>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}