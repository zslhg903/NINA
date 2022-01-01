#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using ASCOM.DriverAccess;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Equipment.ASCOMFacades;
using NINA.Equipment.Interfaces;

namespace NINA.Equipment.Equipment.MySwitch.Ascom {

    internal class AscomWritableSwitch : AscomSwitch, IWritableSwitch {

        public AscomWritableSwitch(ISwitchFacade s, short id) : base(s, id) {
            Maximum = ascomSwitchHub.MaxSwitchValue(id);
            Minimum = ascomSwitchHub.MinSwitchValue(id);
            StepSize = ascomSwitchHub.SwitchStep(id);
            this.TargetValue = this.Value;
        }

        public Task SetValue() {
            Logger.Trace($"Try setting value {TargetValue} for switch id {Id}");
            ascomSwitchHub.SetSwitchValue(Id, TargetValue);
            return CoreUtil.Wait(TimeSpan.FromMilliseconds(50));
        }

        public double Maximum { get; }

        public double Minimum { get; }

        public double StepSize { get; }

        private double targetValue;

        public double TargetValue {
            get => targetValue;
            set {
                targetValue = value;
                RaisePropertyChanged();
            }
        }
    }
}