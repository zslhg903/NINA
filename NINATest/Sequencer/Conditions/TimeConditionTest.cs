﻿#region "copyright"

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
using NINA.Sequencer;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Utility.DateTimeProvider;
using NINA.Astrometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NINA.Core.Utility;
using NINA.Sequencer.Container;
using NINA.Sequencer.Interfaces;

namespace NINATest.Sequencer.Conditions {

    [TestFixture]
    public class TimeConditionTest {

        [Test]
        public void TimeCondition_Clone_GoodClone() {
            var l = new List<IDateTimeProvider>();
            var sut = new TimeCondition(l);
            sut.Icon = new System.Windows.Media.GeometryGroup();
            var item2 = (TimeCondition)sut.Clone();

            item2.Should().NotBeSameAs(sut);
            item2.Icon.Should().BeSameAs(sut.Icon);
            item2.DateTimeProviders.Should().BeSameAs(l);
            item2.Hours.Should().Be(sut.Hours);
            item2.Minutes.Should().Be(sut.Minutes);
            item2.Seconds.Should().Be(sut.Seconds);
        }

        [Test]
        public void TimeCondition_Clone_GoodClone_TimeProviderDoesntOverwrite() {
            var hours = (int)AstroUtil.EuclidianModulus(DateTime.Now.Hour + 1, 24);

            var l = new List<IDateTimeProvider>() { new TimeProvider() };
            var sut = new TimeCondition(l);
            sut.Icon = new System.Windows.Media.GeometryGroup();
            sut.SelectedProvider = l.First();
            sut.Hours = hours;
            sut.Minutes = 20;
            sut.Seconds = 30;
            var item2 = (TimeCondition)sut.Clone();
            item2.AfterParentChanged();

            item2.Should().NotBeSameAs(sut);
            item2.Icon.Should().BeSameAs(sut.Icon);
            item2.DateTimeProviders.Should().BeSameAs(l);
            item2.Hours.Should().Be(hours);
            item2.Minutes.Should().Be(20);
            item2.Seconds.Should().Be(30);
        }

        [Test]
        public void TimeCondition_NoProviderInConstructor_NoCrash() {
            var sut = new TimeCondition(null);

            sut.Hours.Should().Be(0);
            sut.Minutes.Should().Be(0);
            sut.Seconds.Should().Be(0);
        }

        [Test]
        public void TimeCondition_SelectProviderInConstructor_TimeExtracted() {
            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 2, 3, 4, 5, 6));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object });

            sut.Hours.Should().Be(4);
            sut.Minutes.Should().Be(5);
            sut.Seconds.Should().Be(6);
        }

        [Test]
        public void TimeCondition_SelectProvider_TimeExtracted() {
            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(1, 2, 3, 4, 5, 6));
            var provider2Mock = new Mock<IDateTimeProvider>();
            provider2Mock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 10, 30, 10, 20, 30));
            provider2Mock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 10, 30, 10, 20, 30));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object, provider2Mock.Object });
            sut.SelectedProvider = sut.DateTimeProviders.Last();

            sut.Hours.Should().Be(10);
            sut.Minutes.Should().Be(20);
            sut.Seconds.Should().Be(30);
        }

        [Test]
        public void TimeCondition_SelectProvider_OffsetApplied() {
            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(1, 2, 3, 4, 5, 6));
            var provider2Mock = new Mock<IDateTimeProvider>();
            provider2Mock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 10, 30, 10, 20, 30));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object, provider2Mock.Object });
            sut.SelectedProvider = sut.DateTimeProviders.Last();
            sut.MinutesOffset = -5;

            sut.Hours.Should().Be(10);
            sut.Minutes.Should().Be(15);
            sut.Seconds.Should().Be(30);
        }

        [Test]
        [TestCase(0, 10, 10)]
        [TestCase(5, 10, 5)]
        [TestCase(15, 10, 0)]
        public void TimeCondition_RemainingTime_CalculatedCorrectly(int nowSeconds, int providerSeconds, int remainingSeconds) {
            var dateMock = new Mock<ICustomDateTime>();
            dateMock.SetupGet(x => x.Now).Returns(new DateTime(2000, 1, 1, 0, 0, nowSeconds));

            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, 0, 0, providerSeconds));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);
            sut.DateTime = dateMock.Object;

            sut.RemainingTime.Should().Be(TimeSpan.FromSeconds(remainingSeconds));
        }

        [Test]
        [TestCase(0, 2, 2)]
        [TestCase(2, 6, 4)]
        [TestCase(6, 11, 5)]
        [TestCase(12, 12, 0)]
        [TestCase(13, 11, 22)]
        [TestCase(2, 22, 0)]
        [TestCase(23, 22, 0)]
        [TestCase(0, 22, 0)]
        [TestCase(21, 22, 1)]
        [TestCase(22, 2, 4)]
        public void TimeCondition_RemainingTime_CalculatedCorrectly_WithDaySwitches(int nowHours, int conditionHours, int expectedTime) {
            var dateMock = new Mock<ICustomDateTime>();
            dateMock.SetupGet(x => x.Now).Returns(new DateTime(2000, 1, 1, nowHours, 0, 0));

            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, conditionHours, 0, 0));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);
            sut.DateTime = dateMock.Object;

            sut.RemainingTime.Should().Be(TimeSpan.FromHours(expectedTime));
        }

        [Test]
        /* RemainingTime Without Next Item taking any time */
        [TestCase(0, 10, 0, true)]
        [TestCase(5, 10, 0, true)]
        [TestCase(15, 10, 0, false)]
        /* Next Item exceeds remaining time */
        [TestCase(0, 10, 11, false)]
        [TestCase(5, 10, 6, false)]
        [TestCase(15, 10, 10, false)]
        /* Next Item fits into remaining time */
        [TestCase(0, 10, 10, true)]
        [TestCase(5, 10, 5, true)]
        [TestCase(50, 55, 2, true)]
        public void TimeCondition_Check_ReturnsFalseOrTrueAccordingToRemainingTime(int nowSeconds, int providerSeconds, int nextItemSeconds, bool expected) {
            var dateMock = new Mock<ICustomDateTime>();
            dateMock.SetupGet(x => x.Now).Returns(new DateTime(2000, 1, 1, 0, 0, nowSeconds));

            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, 0, 0, providerSeconds));

            var nextItemMock = new Mock<ISequenceItem>();
            nextItemMock.Setup(x => x.GetEstimatedDuration()).Returns(TimeSpan.FromSeconds(nextItemSeconds));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);
            sut.DateTime = dateMock.Object;

            sut.Check(null, nextItemMock.Object).Should().Be(expected);
        }

        [Test]
        public void TimeCondition_AfterParentChanged_HasNoParent_WatchdogNotTouched() {
            var watchdogMock = new Mock<IConditionWatchdog>();

            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, 0, 0, 10));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);
            sut.ConditionWatchdog = watchdogMock.Object;

            sut.AfterParentChanged();

            watchdogMock.Verify(x => x.Start(), Times.Never);
        }

        [Test]
        public void TimeCondition_AfterParentChanged_HasNoRootContainerParent_WatchdogNotTouched() {
            var watchdogMock = new Mock<IConditionWatchdog>();

            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, 0, 0, 10));

            var parentMock = new Mock<ISequenceContainer>();

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);
            sut.ConditionWatchdog = watchdogMock.Object;
            sut.Parent = parentMock.Object;

            sut.AfterParentChanged();

            watchdogMock.Verify(x => x.Start(), Times.Never);
        }

        [Test]
        public void TimeCondition_AfterParentChanged_IsInRootContainer_WatchdogStarted() {
            var watchdogMock = new Mock<IConditionWatchdog>();

            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, 0, 0, 10));

            var parentMock = new Mock<ISequenceRootContainer>();

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);
            sut.ConditionWatchdog = watchdogMock.Object;
            sut.Parent = parentMock.Object;

            sut.AfterParentChanged();

            watchdogMock.Verify(x => x.Start(), Times.Once);
        }

        [Test]
        public void TimeCondition_AfterParentChanged_IsRemovedFromRootContainer_WatchdogStopped() {
            var watchdogMock = new Mock<IConditionWatchdog>();

            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, 0, 0, 10));

            var parentMock = new Mock<ISequenceRootContainer>();

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);
            sut.ConditionWatchdog = watchdogMock.Object;
            sut.Parent = parentMock.Object;

            sut.AfterParentChanged();
            sut.Parent = null;
            sut.AfterParentChanged();

            watchdogMock.Verify(x => x.Start(), Times.Once);
            watchdogMock.Verify(x => x.Cancel(), Times.Once);
        }

        [Test]
        public void TimeCondition_ToString_IncludesTimeCorrectly() {
            var providerMock = new Mock<IDateTimeProvider>();
            providerMock.Setup(x => x.GetDateTime(It.IsAny<ISequenceEntity>())).Returns(new DateTime(2000, 1, 1, 10, 20, 30));

            var sut = new TimeCondition(new List<IDateTimeProvider>() { providerMock.Object }, providerMock.Object);

            sut.ToString().Should().Be("Condition: TimeCondition, Time: 10:20:30h");
        }
    }
}