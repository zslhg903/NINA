#region "copyright"
<<<<<<< HEAD

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors
=======
/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 
>>>>>>> release/2.0

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
<<<<<<< HEAD

#endregion "copyright"

=======
#endregion "copyright"
>>>>>>> release/2.0
using Newtonsoft.Json.Linq;
using NINA.Core.Utility;
using NINA.Core.Utility.Http;
using NINA.Equipment.Interfaces;
using Nito.AsyncEx;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Equipment.MySwitch.Eagle4 {

    internal abstract class EagleWritableSwitch : EagleSwitch, IWritableSwitch {

        public EagleWritableSwitch(short index, string baseUrl) : base(index, baseUrl) {
            this.TargetValue = this.Value;
        }

        // Eagle Manager prior to Version 3.3 cannot get and set labels
        protected bool canSetName = false;

        protected string name;
        private bool nameChangeInProgress = false;

        public override string Name {
            get {
                return name;
            }
            set {
                if (canSetName) {
                    _ = SetName(value);
                }
            }
        }

        private async Task SetName(string targetName) {
            nameChangeInProgress = true;
            try {
                var url = baseUrl + setRoute + "&label={1}";
                name = targetName;
                RaisePropertyChanged(nameof(Name));

                Logger.Trace($"Try setting name {targetName} via {url}");
                var request = new HttpGetRequest(url, Id, targetName);
                var response = await request.Request(new CancellationToken());
                var jobj = JObject.Parse(response);
                var regoutResponse = jobj.ToObject<EagleResponse>();
                if (!regoutResponse.Success()) {
                    Logger.Warning($"Setting Eagle Switch {Id} Label was not successful");
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                nameChangeInProgress = false;
            }
        }

        protected void ReceivedName(string newName) {
            canSetName = true;
            if (!nameChangeInProgress && name != newName) {
                name = newName;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public abstract double Maximum { get; }

        public abstract double Minimum { get; }

        public abstract double StepSize { get; }

        protected string setRoute;
        protected string setValueAttribute;

        public async Task SetValue() {
            try {
                var url = baseUrl + setRoute + $"&{setValueAttribute}={{1}}";

                Logger.Trace($"Try setting value {TargetValue} via {url}");

                var request = new HttpGetRequest(url, Id, TargetValue);
                var response = await request.Request(new CancellationToken());

                var jobj = JObject.Parse(response);
                var regoutResponse = jobj.ToObject<EagleResponse>();
                if (!regoutResponse.Success()) {
                    Logger.Warning($"Unable to set value {TargetValue} via {url}");
                } else {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

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