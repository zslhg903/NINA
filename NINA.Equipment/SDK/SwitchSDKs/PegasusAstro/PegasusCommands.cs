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
using NINA.Core.Utility.SerialCommunication;

namespace NINA.Equipment.SDK.SwitchSDKs.PegasusAstro {

    public class FirmwareVersionCommand : ISerialCommand {
        public string CommandString => "PV\n";
        public bool HasResponse => true;
    }

    public class StatusCommand : ISerialCommand {
        public string CommandString => "PA\n";
        public bool HasResponse => true;
    }

    public class SetPowerCommand : ISerialCommand {
        public short SwitchNumber { get; set; }
        public bool On { get; set; }
        public string CommandString => $"P{SwitchNumber}:{(On ? 1 : 0)}\n";
        public bool HasResponse => true;
    }

    public class SetUsbPowerCommand : ISerialCommand {
        public short SwitchNumber { get; set; }
        public bool On { get; set; }
        public string CommandString => $"U{SwitchNumber}:{(On ? 1 : 0)}\n";
        public bool HasResponse => true;
    }

    public class SetVariableVoltageCommand : ISerialCommand {
        private double _variableVoltage;

        public double VariableVoltage {
            set {
                if (value < 3d) { value = 3d; }
                if (value > 12d) { value = 12d; }
                _variableVoltage = value;
            }
        }

        public string CommandString => $"P8:{_variableVoltage}\n";
        public bool HasResponse => true;
    }

    public class PowerStatusCommand : ISerialCommand {
        public string CommandString => "PS\n";
        public bool HasResponse => true;
    }

    public class SetDewHeaterPowerCommand : ISerialCommand {
        public short SwitchNumber { get; set; }
        public double DutyCycle { get; set; }
        public string CommandString => $"P{SwitchNumber + 5}:{DutyCycle * 255 / 100:000}\n";
        public bool HasResponse => true;
    }

    public class PowerConsumptionCommand : ISerialCommand {
        public string CommandString => "PC\n";
        public bool HasResponse => true;
    }

    public class SetAutoDewCommand : ISerialCommand {
        private readonly int _newAutoDewStatus;

        public SetAutoDewCommand(ICollection<bool> currentStatus, short id, bool turnOn) {
            var channel = new bool[3];
            currentStatus.CopyTo(channel, 0);
            channel[id] = turnOn;
            if (!channel[0] && !channel[1] && !channel[2]) _newAutoDewStatus = 0;
            if (channel[0] && channel[1] && channel[2]) _newAutoDewStatus = 1;
            if (channel[0] && !channel[1] && !channel[2]) _newAutoDewStatus = 2;
            if (!channel[0] && channel[1] && !channel[2]) _newAutoDewStatus = 3;
            if (!channel[0] && !channel[1] && channel[2]) _newAutoDewStatus = 4;
            if (channel[0] && channel[1] && !channel[2]) _newAutoDewStatus = 5;
            if (channel[0] && !channel[1] && channel[2]) _newAutoDewStatus = 6;
            if (!channel[0] && channel[1] && channel[2]) _newAutoDewStatus = 7;
        }

        public string CommandString => $"PD:{_newAutoDewStatus}\n";
        public bool HasResponse => true;
    }

    public class StepperMotorTemperatureCommand : ISerialCommand {
        public string CommandString => "ST\n";
        public bool HasResponse => true;
    }

    public class StepperMotorMoveToPositionCommand : ISerialCommand {
        public int Position { get; set; }
        public string CommandString => $"SM:{Position}\n";
        public bool HasResponse => true;
    }

    public class StepperMotorHaltCommand : ISerialCommand {
        public string CommandString => "SH\n";
        public bool HasResponse => true;
    }

    public class StepperMotorDirectionCommand : ISerialCommand {
        public bool DirectionClockwise { get; set; }
        public string CommandString => $"SR:{(DirectionClockwise ? 0 : 1)}\n";
        public bool HasResponse => true;
    }

    public class StepperMotorGetCurrentPositionCommand : ISerialCommand {
        public string CommandString => "SP\n";
        public bool HasResponse => true;
    }

    public class StepperMotorSetCurrentPositionCommand : ISerialCommand {
        public int Position { get; set; }
        public string CommandString => $"SC:{Position}\n";
        public bool HasResponse => true;
    }

    public class StepperMotorSetMaximumSpeedCommand : ISerialCommand {
        public int Speed { get; set; }
        public string CommandString => $"SS:{Speed}\n";
        public bool HasResponse => false;
    }

    public class StepperMotorIsMovingCommand : ISerialCommand {
        public string CommandString => "SI\n";
        public bool HasResponse => true;
    }

    public class StepperMotorSetBacklashStepsCommand : ISerialCommand {
        public int Steps { get; set; }
        public string CommandString => $"SB:{Steps}\n";
        public bool HasResponse => true;
    }
}