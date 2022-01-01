#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NINA.Core.Utility;
using NINA.Core.Utility.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Equipment.MySwitch.Eagle {

    internal class Eagle12VPower : EagleWritableSwitch {

        public Eagle12VPower(short index, string baseUrl) : base(index, baseUrl) {
            getRoute = "getpwrout?idx={0}";
            setRoute = "setpwrout?idx={0}";
            setValueAttribute = "state";
            name = "12V Power Out " + (4 - Id).ToString();
            Description = "Fixed Power Out " + (4 - Id).ToString();
        }

        public override string Description { get; }

        public override double Maximum {
            get => 1d;
        }

        public override double Minimum {
            get => 0d;
        }

        public override double StepSize {
            get => 1d;
        }

        protected override async Task<double> GetValue() {
            var url = baseUrl + getRoute;

            Logger.Trace($"Try getting value via {url}");

            var request = new HttpGetRequest(url, Id);
            var response = await request.Request(new CancellationToken());

            var jobj = JObject.Parse(response);
            var regoutResponse = jobj.ToObject<PowerOutResponse>();
            if (regoutResponse.Success()) {
                if (!string.IsNullOrWhiteSpace(regoutResponse.Label)) {
                    ReceivedName(regoutResponse.Label);
                }
                return regoutResponse.Voltage > 0d ? 1d : 0d;
            } else {
                return double.NaN;
            }
        }

        private class PowerOutResponse : EagleResponse {

            [JsonProperty(PropertyName = "voltage")]
            public double Voltage;

            [JsonProperty(PropertyName = "current")]
            public double Current;

            [JsonProperty(PropertyName = "power")]
            public double Power;

            [JsonProperty(PropertyName = "label")]
            public string Label;

            public PowerOutResponse(double voltage, double current, double power, string label) {
                Voltage = voltage;
                Current = current;
                Power = power;
                Label = label;
            }
        }
    }
}