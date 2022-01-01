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
using NINA.Profile.Interfaces;
using NINA.Sequencer.SequenceItem.Utility;
using NUnit.Framework;

namespace NINATest.Sequencer.SequenceItem.Utility {

    [TestFixture]
    public class WaitForSunAltitudeTest {
        private Mock<IProfileService> profileServiceMock;

        [SetUp]
        public void Setup() {
            profileServiceMock = new Mock<IProfileService>();
            profileServiceMock.SetupGet(x => x.ActiveProfile.AstrometrySettings.Latitude).Returns(0);
            profileServiceMock.SetupGet(x => x.ActiveProfile.AstrometrySettings.Longitude).Returns(0);
        }

        [Test]
        public void WaitForMoonAltitude_Clone_GoodClone() {
            var sut = new WaitForSunAltitude(profileServiceMock.Object);
            sut.Icon = new System.Windows.Media.GeometryGroup();
            var item2 = (WaitForSunAltitude)sut.Clone();

            item2.Should().NotBeSameAs(sut);
            item2.Name.Should().BeSameAs(sut.Name);
            item2.Description.Should().BeSameAs(sut.Description);
            item2.Icon.Should().BeSameAs(sut.Icon);
            item2.UserSunAltitude.Should().Be(sut.UserSunAltitude);
            item2.Comparator.Should().Be(sut.Comparator);
        }
    }
}