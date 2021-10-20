﻿using FluentAssertions;
using Moq;
using NINA.Equipment.Equipment.MyFilterWheel;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Trigger.Autofocus;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Core.Utility.WindowService;
using NINA.ViewModel;
using NINA.ViewModel.ImageHistory;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NINA.Core.Model.Equipment;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.Equipment.Equipment.MyFocuser;
using NINA.Equipment.Equipment.MyCamera;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.Utility.AutoFocus;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Interfaces;
using NINA.WPF.Base.Interfaces;

namespace NINATest.Sequencer.Trigger.Autofocus {

    [TestFixture]
    public class AutofocusAfterFilterChangeTest {
        private ImageDataFactoryTestUtility dataFactoryUtility;
        private Mock<IProfileService> profileServiceMock;
        private Mock<IImageHistoryVM> historyMock;
        private Mock<ICameraMediator> cameraMediatorMock;
        private Mock<IFilterWheelMediator> filterWheelMediatorMock;
        private Mock<IFocuserMediator> focuserMediatorMock;
        private Mock<IAutoFocusVMFactory> autoFocusVMFactoryMock;
        private AutofocusAfterFilterChange sut;

        [SetUp]
        public void Setup() {
            dataFactoryUtility = new ImageDataFactoryTestUtility();
            profileServiceMock = dataFactoryUtility.ProfileServiceMock;
            historyMock = new Mock<IImageHistoryVM>();
            cameraMediatorMock = new Mock<ICameraMediator>();
            filterWheelMediatorMock = new Mock<IFilterWheelMediator>();
            focuserMediatorMock = new Mock<IFocuserMediator>();
            autoFocusVMFactoryMock = new Mock<IAutoFocusVMFactory>();

            profileServiceMock.SetupGet(x => x.ActiveProfile.FocuserSettings.AutoFocusExposureTime).Returns(2);
            profileServiceMock.SetupGet(x => x.ActiveProfile.FocuserSettings.AutoFocusInitialOffsetSteps).Returns(4);
            profileServiceMock.SetupGet(x => x.ActiveProfile.FocuserSettings.AutoFocusNumberOfFramesPerPoint).Returns(2);
            profileServiceMock.SetupGet(x => x.ActiveProfile.FocuserSettings.FocuserSettleTime).Returns(1);

            cameraMediatorMock.Setup(x => x.GetInfo()).Returns(new CameraInfo { Connected = true });
            focuserMediatorMock.Setup(x => x.GetInfo()).Returns(new FocuserInfo { Connected = true });
            sut = new AutofocusAfterFilterChange(profileServiceMock.Object, historyMock.Object, cameraMediatorMock.Object, filterWheelMediatorMock.Object, focuserMediatorMock.Object, autoFocusVMFactoryMock.Object);
        }

        [Test]
        public void Test_Clone() {
            var result = (AutofocusAfterFilterChange)sut.Clone();

            result.Should().NotBeSameAs(sut);
            result.Icon.Should().BeSameAs(sut.Icon);
            result.Name.Should().BeSameAs(sut.Name);
        }

        [Test]
        public void Test_Initialize() {
            var filterWheelInfo = new FilterWheelInfo();
            var filter = new FilterInfo();
            filterWheelInfo.SelectedFilter = filter;
            filterWheelMediatorMock.Setup(m => m.GetInfo())
                .Returns(filterWheelInfo);

            sut.Initialize();

            sut.LastAutoFocusFilter.Should().Be(filter);
        }

        [Test]
        public void Test_ShouldTrigger_WhenNoChangeNoTrigger() {
            var filterWheelInfo = new FilterWheelInfo();
            var filter = new FilterInfo();
            filterWheelInfo.SelectedFilter = filter;
            filterWheelMediatorMock.Setup(m => m.GetInfo())
                .Returns(filterWheelInfo);

            sut.SequenceBlockStarted();

            var result = sut.ShouldTrigger(null, new Mock<IExposureItem>().Object);

            result.Should().BeFalse();
        }

        [Test]
        public void Test_ShouldTrigger_WhenChangeThenTrigger() {
            var filterWheelInfo = new FilterWheelInfo();
            var filterInfo = new FilterInfo() {
                Name = "Test1"
            };
            filterWheelInfo.SelectedFilter = filterInfo;

            var nextFilterWheelInfo = new FilterWheelInfo();
            var nextFilterInfo = new FilterInfo() {
                Name = "Test2"
            };
            nextFilterWheelInfo.SelectedFilter = nextFilterInfo;

            filterWheelMediatorMock.SetupSequence(m => m.GetInfo())
                .Returns(filterWheelInfo)
                .Returns(nextFilterWheelInfo);

            sut.Initialize();

            var result = sut.ShouldTrigger(null, new Mock<IExposureItem>().Object);

            result.Should().BeTrue();
        }

        [Test]
        public void Test_ShouldTrigger_WhenAlwaysNullThenNoTrigger() {
            sut.SequenceBlockStarted();

            var result = sut.ShouldTrigger(null, new Mock<IExposureItem>().Object);

            result.Should().BeFalse();
        }

        [Test]
        public async Task Execute_Successfully_WithAllParametersPassedCorrectly() {
            var report = new AutoFocusReport();

            var filter = new FilterInfo() { Position = 0 };
            filterWheelMediatorMock.Setup(x => x.GetInfo()).Returns(new FilterWheelInfo() { SelectedFilter = filter });

            await sut.Execute(default, default, default);

            // Todo proper assertion
            //historyMock.Verify(h => h.AppendAutoFocusPoint(It.Is<AutoFocusReport>(r => r == report)), Times.Once);
        }

        [Test]
        public void ToString_FilledProperly() {
            var tostring = sut.ToString();
            tostring.Should().Be("Trigger: AutofocusAfterFilterChange");
        }
    }
}