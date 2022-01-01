﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyFlatDevice;
using NINA.Equipment.Equipment.MyCamera;
using NINA.WPF.Base.Model;

namespace NINA.WPF.Base.Interfaces.ViewModel {

    public interface IFlatWizardUserPromptVM {
        RelayCommand ResetAndContinueCommand { get; }
        RelayCommand ContinueCommand { get; }
        RelayCommand CancelCommand { get; }
        FlatWizardFilterSettingsWrapper Settings { get; set; }
        string Message { get; set; }
        double CurrentMean { get; set; }
        double CameraBitDepth { get; set; }
        double ExpectedExposureTime { get; set; }
        double ExpectedBrightness { get; set; }
        FlatWizardMode FlatWizardMode { get; set; }
        CameraInfo CameraInfo { get; set; }
        FlatDeviceInfo FlatDeviceInfo { get; set; }
        FlatWizardUserPromptResult Result { get; set; }
    }

    public enum FlatWizardUserPromptResult {
        Continue,
        ResetAndContinue,
        Cancel
    }
}