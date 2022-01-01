#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Collections.Generic;
using NINA.Equipment.Equipment.MyCamera;
using NINA.Equipment.Equipment.MyFilterWheel;
using NINA.Core.Utility;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NINA.Core.Utility.WindowService;
using Nito.AsyncEx;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Core.Model.Equipment;
using NINA.WPF.Base.Model;
using NINA.Core.Enum;

namespace NINA.WPF.Base.Interfaces.ViewModel {

    public interface IFlatWizardVM : ICameraConsumer, IFilterWheelConsumer, ITelescopeConsumer, IFlatDeviceConsumer {
        BinningMode BinningMode { get; set; }
        double CalculatedExposureTime { get; set; }
        double CalculatedHistogramMean { get; set; }
        bool CameraConnected { get; set; }
        RelayCommand CancelFlatExposureSequenceCommand { get; }
        ObservableCollection<FilterInfo> FilterInfos { get; }
        ObservableCollection<FlatWizardFilterSettingsWrapper> Filters { get; set; }
        int FlatCount { get; set; }
        int Gain { get; set; }
        BitmapSource Image { get; set; }
        bool IsPaused { get; set; }
        int Mode { get; set; }
        RelayCommand PauseFlatExposureSequenceCommand { get; }
        RelayCommand ResumeFlatExposureSequenceCommand { get; }
        FilterInfo SelectedFilter { get; set; }
        FlatWizardFilterSettingsWrapper SingleFlatWizardFilterSettings { get; set; }
        IAsyncCommand StartFlatSequenceCommand { get; }
        bool PauseBetweenFilters { get; set; }
        FlatWizardMode FlatWizardMode { get; set; }
        IWindowService WindowService { get; set; }

        Task<double> FindFlatExposureTime(PauseToken pt, FlatWizardFilterSettingsWrapper filter);

        Task<int> FindFlatDeviceBrightness(PauseToken pt, FlatWizardFilterSettingsWrapper filter);

        Task<bool> StartFlatMagic(IEnumerable<FlatWizardFilterSettingsWrapper> filterWrappers, PauseToken pt);
    }
}