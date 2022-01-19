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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NINA.Core.Utility;
using NINA.Core.Utility.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Equipment.MySwitch.Eagle4 {

    internal class EagleUSBSwitch : EagleWritableSwitch {

        public EagleUSBSwitch(short index, string baseUrl) : base(index, baseUrl) {
            getRoute = $"getpwrhub?idx={Id}";
            setRoute = $"setpwrhub?idx={Id}";
            setValueAttribute = "state";
            name = GetDefaultName();
            Description = GetDescription();
        }

        private string GetDefaultName() {
            switch (Id) {
                case 1: return "USB A";
                case 2: return "USB B";
                case 3: return "USB C";
                case 4: return "USB D";
                default: return "USB Unknown";
            }
        }

        private string GetDescription() {
            switch (Id) {
                case 1: return "Usb hub output A";
                case 2: return "Usb hub output B";
                case 3: return "Usb hub output C";
                case 4: return "Usb hub output D";
                default: return "USB Unknown";
            }
        }

        public override double Maximum {
            get => 1d;
        }

        public override double Minimum {
            get => 0d;
        }

        public override double StepSize {
            get => 1d;
        }

        public override string Description { get; }

        protected override async Task<double> GetValue() {
            var url = baseUrl + getRoute;

            Logger.Trace($"Try getting value via {url}");

            var request = new HttpGetRequest(url, Id);
            var response = await request.Request(new CancellationToken());

            var jobj = JObject.Parse(response);
            var regoutResponse = jobj.ToObject<PowerHubResponse>();
            if (regoutResponse.Success()) {
                if (!string.IsNullOrWhiteSpace(regoutResponse.Label)) {
                    ReceivedName(regoutResponse.Label);
                }
                return regoutResponse.Status;
            } else {
                return double.NaN;
            }
        }

        private class PowerHubResponse : EagleResponse {

            [JsonProperty(PropertyName = "status")]
            public int Status;

            [JsonProperty(PropertyName = "label")]
            public string Label;

            public PowerHubResponse(int status, string label) {
                Status = status;
                Label = label;
            }
        }
    }
}