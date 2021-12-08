﻿using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Interfaces;
using NINA.Utility;
using NINA.ViewModel.Interfaces;
using NINA.ViewModel.Sequencer;
using NINA.WPF.Base.Interfaces.ViewModel;

namespace NINA.ViewModel {

    internal class MainWindowVM : IMainWindowVM {
        public IImagingVM ImagingVM { get; set; }
        public IApplicationVM AppVM { get; set; }
        public IEquipmentVM EquipmentVM { get; set; }
        public ISkyAtlasVM SkyAtlasVM { get; set; }
        public IFramingAssistantVM FramingAssistantVM { get; set; }
        public IFlatWizardVM FlatWizardVM { get; set; }
        public IDockManagerVM DockManagerVM { get; set; }
        public ISequenceNavigationVM SequenceNavigationVM { get; set; }
        public IOptionsVM OptionsVM { get; set; }
        public IVersionCheckVM VersionCheckVM { get; set; }
        public IApplicationStatusVM ApplicationStatusVM { get; set; }
        public IApplicationDeviceConnectionVM ApplicationDeviceConnectionVM { get; set; }
        public IImageSaveController ImageSaveController { get; set; }
        public IImageHistoryVM ImageHistoryVM { get; set; }
        public IPluginsVM PluginsVM { get; set; }
        public GlobalObjects GlobalObjects { get; set; }
        public IDeviceDispatcher DeviceDispatcher { get; set; }
    }
}