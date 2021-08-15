﻿#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.Validations;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Locale;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Equipment.MySwitch;

namespace NINA.Sequencer.SequenceItem.Switch {

    [ExportMetadata("Name", "Lbl_SequenceItem_Switch_SetSwitchValue_Name")]
    [ExportMetadata("Description", "Lbl_SequenceItem_Switch_SetSwitchValue_Description")]
    [ExportMetadata("Icon", "ButtonSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Switch")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SetSwitchValue : SequenceItem, IValidatable {
        private ISwitchMediator switchMediator;

        [ImportingConstructor]
        public SetSwitchValue(ISwitchMediator switchMediator) {
            this.switchMediator = switchMediator;

            WritableSwitches = new ReadOnlyCollection<IWritableSwitch>(CreateDummyList());
            SelectedSwitch = WritableSwitches.First();
        }

        private SetSwitchValue(SetSwitchValue cloneMe) : this(cloneMe.switchMediator) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new SetSwitchValue(this) {
                SwitchIndex = SwitchIndex,
                Value = Value
            };
        }

        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = value;
                RaisePropertyChanged();
            }
        }

        private double value;

        [JsonProperty]
        public double Value {
            get => value;
            set {
                this.value = value;
                Validate();
                RaisePropertyChanged();
            }
        }

        private short switchIndex;

        [JsonProperty]
        public short SwitchIndex {
            get => switchIndex;
            set {
                if (value > -1) {
                    switchIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IWritableSwitch selectedSwitch;

        [JsonIgnore]
        public IWritableSwitch SelectedSwitch {
            get => selectedSwitch;
            set {
                selectedSwitch = value;
                SwitchIndex = (short)WritableSwitches.IndexOf(selectedSwitch);
                RaisePropertyChanged();
            }
        }

        private ReadOnlyCollection<IWritableSwitch> writableSwitches;

        public ReadOnlyCollection<IWritableSwitch> WritableSwitches {
            get => writableSwitches;
            set {
                writableSwitches = value;
                RaisePropertyChanged();
            }
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            return switchMediator.SetSwitchValue(switchIndex, Value, progress, token);
        }

        private IList<IWritableSwitch> CreateDummyList() {
            var dummySwitches = new List<IWritableSwitch>();
            for (short i = 0; i < 20; i++) {
                dummySwitches.Add(new DummySwitch((short)(i + 1)));
            }
            return dummySwitches;
        }

        public bool Validate() {
            var i = new List<string>();
            var info = switchMediator.GetInfo();
            if (info?.Connected != true) {
                //When switch gets disconnected the real list will be changed to the dummy list
                if (!(WritableSwitches.FirstOrDefault() is DummySwitch)) {
                    WritableSwitches = new ReadOnlyCollection<IWritableSwitch>(CreateDummyList());
                }

                i.Add(Loc.Instance["LblSwitchNotConnected"]);
            } else {
                if (WritableSwitches.Count > 0) {
                    //When switch gets connected the dummy list will be changed to the real list
                    if (WritableSwitches.FirstOrDefault() is DummySwitch) {
                        WritableSwitches = info.WritableSwitches;

                        if (switchIndex >= 0 && WritableSwitches.Count > switchIndex) {
                            SelectedSwitch = WritableSwitches[switchIndex];
                        } else {
                            SelectedSwitch = null;
                        }
                    }
                } else {
                    SelectedSwitch = null;
                    i.Add(Loc.Instance["Lbl_SequenceItem_Validation_NoWritableSwitch"]);
                }
            }

            if (switchIndex >= 0 && WritableSwitches.Count > switchIndex) {
                if (WritableSwitches[switchIndex] != SelectedSwitch) {
                    SelectedSwitch = WritableSwitches[switchIndex];
                }
            }

            var s = SelectedSwitch;

            if (s == null) {
                i.Add(string.Format(Loc.Instance["Lbl_SequenceItem_Validation_NoSwitchSelected"]));
            } else {
                if (Value < s.Minimum || Value > s.Maximum)
                    i.Add(string.Format(Loc.Instance["Lbl_SequenceItem_Validation_InvalidSwitchValue"], s.Minimum, s.Maximum, s.StepSize));
            }

            Issues = i;
            return i.Count == 0;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(SetSwitchValue)}, SwitchIndex {SwitchIndex}, Value: {Value}";
        }
    }
}