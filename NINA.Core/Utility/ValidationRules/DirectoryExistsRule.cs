#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace NINA.Core.Utility.ValidationRules {

    public class DirectoryExistsRule : ValidationRule {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            var dir = value.ToString();
            if (!Directory.Exists(dir)) {
                return new ValidationResult(false, "Invalid Directory");
            } else {
                return new ValidationResult(true, null);
            }
        }
    }

    public class DirectoryExistsOrEmptyRule : ValidationRule {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            var dir = value.ToString();
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir)) {
                return new ValidationResult(false, "Invalid Directory");
            } else {
                return new ValidationResult(true, null);
            }
        }
    }
}