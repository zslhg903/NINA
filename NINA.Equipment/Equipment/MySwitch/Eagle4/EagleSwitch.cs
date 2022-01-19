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
using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.Equipment.MySwitch.Eagle4 {

    internal abstract class EagleSwitch : BaseINPC, ISwitch {
        protected string baseUrl;
        protected string getRoute;

        public EagleSwitch(short index, string baseUrl) {
            this.Id = index;
            this.baseUrl = baseUrl;
        }

        public virtual string Name {
            get => Id.ToString();
            set { }
        }

        public virtual string Description {
            get => string.Empty;
        }

        public double Value { get; private set; }

        public short Id { get; }

        public async Task<bool> Poll() {
            try {
                var val = await GetValue();
                if (!double.IsNaN(val)) {
                    this.Value = val;
                    RaisePropertyChanged(nameof(Value));
                } else {
                    return false;
                }
            } catch (Exception ex) {
                Logger.Error(ex);
                return false;
            }

            return true;
        }

        protected abstract Task<double> GetValue();

        protected class EagleResponse {

            [JsonProperty(PropertyName = "result")]
            public string Result;

            public bool Success() {
                return Result.ToLower().Trim() == "ok";
            }
        }
    }
}