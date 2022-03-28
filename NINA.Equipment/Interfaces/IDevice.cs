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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Interfaces {

    public interface IDevice : INotifyPropertyChanged {
        bool HasSetupDialog { get; }
        string Id { get; }
        string Name { get; }

        string Category { get; }
        bool Connected { get; }
        string Description { get; }
        string DriverInfo { get; }
        string DriverVersion { get; }

        Task<bool> Connect(CancellationToken token);

        void Disconnect();

        void SetupDialog();

        IList<string> SupportedActions { get; }

        string Action(string actionName, string actionParameters);

        string SendCommandString(string command, bool raw = true);

        bool SendCommandBool(string command, bool raw = true);

        void SendCommandBlind(string command, bool raw = true);
    }
}