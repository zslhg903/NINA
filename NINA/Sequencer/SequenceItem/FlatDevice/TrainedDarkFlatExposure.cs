﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Model;
using NINA.Profile;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container;
using NINA.Sequencer.Container.ExecutionStrategy;
using NINA.Sequencer.SequenceItem.FilterWheel;
using NINA.Sequencer.SequenceItem.Imaging;
using NINA.Sequencer.Trigger;
using NINA.Utility.Mediator.Interfaces;
using NINA.ViewModel.ImageHistory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Sequencer.SequenceItem.FlatDevice {

    [ExportMetadata("Name", "Lbl_SequenceItem_FlatDevice_TrainedDarkFlatExposure_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_FlatDevice_TrainedDarkFlatExposure_Description")]
    [ExportMetadata("Icon", "BrainBulbSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_FlatDevice")]
    [Export(typeof(ISequenceItem))]
    [Export(typeof(ISequenceContainer))]
    [JsonObject(MemberSerialization.OptIn)]
    public class TrainedDarkFlatExposure : SequentialContainer, IImmutableContainer {

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context) {
            this.Items.Clear();
            this.Conditions.Clear();
            this.Triggers.Clear();
        }

        private IProfileService profileService;

        [ImportingConstructor]
        public TrainedDarkFlatExposure(IProfileService profileService, ICameraMediator cameraMediator, IImagingMediator imagingMediator, IImageSaveMediator imageSaveMediator, IImageHistoryVM imageHistoryVM, IFilterWheelMediator filterWheelMediator, IFlatDeviceMediator flatDeviceMediator) :
            this(
                profileService,
                new CloseCover(flatDeviceMediator),
                new ToggleLight(flatDeviceMediator) { OnOff = false },
                new SwitchFilter(profileService, filterWheelMediator),
                new SetBrightness(flatDeviceMediator),
                new TakeExposure(profileService, cameraMediator, imagingMediator, imageSaveMediator, imageHistoryVM) { ImageType = CaptureSequence.ImageTypes.DARKFLAT },
                new LoopCondition() { Iterations = 1 },
                new OpenCover(flatDeviceMediator)

            ) {
        }

        public TrainedDarkFlatExposure(
            IProfileService profileService,
            CloseCover closeCover,
            ToggleLight toggleLightOff,
            SwitchFilter switchFilter,
            SetBrightness setBrightness,
            TakeExposure takeExposure,
            LoopCondition loopCondition,
            OpenCover openCover
            ) {
            this.profileService = profileService;

            this.Add(closeCover);
            this.Add(toggleLightOff);
            this.Add(switchFilter);
            this.Add(setBrightness);

            var container = new SequentialContainer();
            container.Add(loopCondition);
            container.Add(takeExposure);
            this.Add(container);
            this.Add(openCover);

            IsExpanded = false;
        }

        public CloseCover GetCloseCoverItem() {
            return (Items[0] as CloseCover);
        }

        public ToggleLight GetToggleLightOffItem() {
            return (Items[1] as ToggleLight);
        }

        public SwitchFilter GetSwitchFilterItem() {
            return (Items[2] as SwitchFilter);
        }

        public SetBrightness GetSetBrightnessItem() {
            return (Items[3] as SetBrightness);
        }

        public SequentialContainer GetImagingContainer() {
            return (Items[4] as SequentialContainer);
        }

        public TakeExposure GetExposureItem() {
            return ((Items[4] as SequentialContainer).Items[0] as TakeExposure);
        }

        public LoopCondition GetIterations() {
            return ((Items[4] as IConditionable).Conditions[0] as LoopCondition);
        }

        public OpenCover GetOpenCoverItem() {
            return (Items[5] as OpenCover);
        }

        public override object Clone() {
            var clone = new TrainedDarkFlatExposure(
                profileService,
                (CloseCover)this.GetCloseCoverItem().Clone(),
                (ToggleLight)this.GetToggleLightOffItem().Clone(),
                (SwitchFilter)this.GetSwitchFilterItem().Clone(),
                (SetBrightness)this.GetSetBrightnessItem().Clone(),
                (TakeExposure)this.GetExposureItem().Clone(),
                (LoopCondition)this.GetIterations().Clone(),
                (OpenCover)this.GetOpenCoverItem().Clone()
            ) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
            };
            return clone;
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            /* Lookup trained values and set brightness and exposure time accordingly */
            var filter = GetSwitchFilterItem()?.Filter;
            var takeExposure = GetExposureItem();
            var binning = takeExposure.Binning;
            var gain = takeExposure.Gain == -1 ? profileService.ActiveProfile.CameraSettings.Gain ?? -1 : takeExposure.Gain;
            var info = profileService.ActiveProfile.FlatDeviceSettings.GetBrightnessInfo(new FlatDeviceFilterSettingsKey(filter?.Position, binning, gain));

            (Items[3] as SetBrightness).Brightness = 0;
            takeExposure.ExposureTime = info.Time;

            return base.Execute(progress, token);
        }

        public override bool Validate() {
            var switchFilter = GetSwitchFilterItem();
            var takeExposure = GetExposureItem();
            var setBrightness = GetSetBrightnessItem();

            var valid = takeExposure.Validate() && switchFilter.Validate() && setBrightness.Validate();

            var issues = new List<string>();

            if (valid) {
                var filter = switchFilter?.Filter;
                var binning = takeExposure.Binning;
                var gain = takeExposure.Gain == -1 ? profileService.ActiveProfile.CameraSettings.Gain ?? -1 : takeExposure.Gain;
                if (profileService.ActiveProfile.FlatDeviceSettings.GetBrightnessInfo(new FlatDeviceFilterSettingsKey(filter?.Position, binning, gain)) == null) {
                    issues.Add(string.Format(Locale.Loc.Instance["Lbl_SequenceItem_Validation_FlatDeviceTrainedExposureNotFound"], filter?.Name, gain, binning?.Name));
                    valid = false;
                }
            }

            Issues = issues.Concat(takeExposure.Issues).Concat(switchFilter.Issues).Concat(setBrightness.Issues).Distinct().ToList();
            RaisePropertyChanged(nameof(Issues));

            return valid;
        }
    }
}