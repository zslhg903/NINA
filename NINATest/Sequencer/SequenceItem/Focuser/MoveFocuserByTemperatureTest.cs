﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using FluentAssertions;
using Moq;
using NINA.Model.MyFocuser;
using NINA.Sequencer.Exceptions;
using NINA.Sequencer.SequenceItem.Focuser;
using NINA.Utility.Mediator.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINATest.Sequencer.SequenceItem.Focuser {

    internal class MoveFocuserByTemperatureTest {
        public Mock<IFocuserMediator> focuserMediatorMock;

        [SetUp]
        public void Setup() {
            focuserMediatorMock = new Mock<IFocuserMediator>();
        }

        [Test]
        public void Clone_ItemClonedProperly() {
            var sut = new MoveFocuserByTemperature(focuserMediatorMock.Object);
            sut.Name = "SomeName";
            sut.Description = "SomeDescription";
            sut.Icon = new System.Windows.Media.GeometryGroup();
            sut.Slope = 12234;
            sut.Intercept = 99802;
            var item2 = (MoveFocuserByTemperature)sut.Clone();

            item2.Should().NotBeSameAs(sut);
            item2.Name.Should().BeSameAs(sut.Name);
            item2.Description.Should().BeSameAs(sut.Description);
            item2.Icon.Should().BeSameAs(sut.Icon);
            item2.Slope.Should().Be(sut.Slope);
            item2.Intercept.Should().Be(sut.Intercept);
        }

        [Test]
        public void Validate_NoIssues() {
            focuserMediatorMock.Setup(x => x.GetInfo()).Returns(new FocuserInfo() { Connected = true, Temperature = 10 });

            var sut = new MoveFocuserByTemperature(focuserMediatorMock.Object);
            var valid = sut.Validate();

            valid.Should().BeTrue();

            sut.Issues.Should().BeEmpty();
        }

        [Test]
        public void Validate_NotConnected_OneIssue() {
            focuserMediatorMock.Setup(x => x.GetInfo()).Returns(new FocuserInfo() { Connected = false });

            var sut = new MoveFocuserByTemperature(focuserMediatorMock.Object);
            var valid = sut.Validate();

            valid.Should().BeFalse();

            sut.Issues.Should().HaveCount(1);
        }

        [Test]
        public void Validate_NoTempProbe_OneIssue() {
            focuserMediatorMock.Setup(x => x.GetInfo()).Returns(new FocuserInfo() { Connected = true, Temperature = double.NaN });

            var sut = new MoveFocuserByTemperature(focuserMediatorMock.Object);
            var valid = sut.Validate();

            valid.Should().BeFalse();

            sut.Issues.Should().HaveCount(1);
        }

        [Test]
        [TestCase(10, 2000, 10, 2100)]
        [TestCase(10, 2000, 0, 2000)]
        [TestCase(10, 2000, -10, 1900)]
        public async Task Execute_NoIssues_LogicCalled(double slope, double intercept, double temperature, int position) {
            focuserMediatorMock.Setup(x => x.GetInfo()).Returns(new FocuserInfo() { Connected = true, Temperature = temperature });

            var sut = new MoveFocuserByTemperature(focuserMediatorMock.Object);
            sut.Slope = slope;
            sut.Intercept = intercept;
            await sut.Execute(default, default);

            focuserMediatorMock.Verify(x => x.MoveFocuser(It.Is<int>(p => p == position), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public Task Execute_HasIssues_LogicNotCalled() {
            focuserMediatorMock.Setup(x => x.GetInfo()).Returns(new FocuserInfo() { Connected = false });

            var sut = new MoveFocuserByTemperature(focuserMediatorMock.Object);
            Func<Task> act = () => { return sut.Execute(default, default); };

            focuserMediatorMock.Verify(x => x.MoveFocuser(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            return act.Should().ThrowAsync<SequenceItemSkippedException>(string.Join(",", sut.Issues));
        }

        [Test]
        public void GetEstimatedDuration_BasedOnParameters_ReturnsCorrectEstimate() {
            var sut = new MoveFocuserByTemperature(focuserMediatorMock.Object);

            var duration = sut.GetEstimatedDuration();

            duration.Should().Be(TimeSpan.Zero);
        }
    }
}