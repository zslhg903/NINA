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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace NINA.Core.Utility.ValidationRules {

    public abstract class IntRangeRuleBase : ValidationRule {
        public IntRangeChecker ValidRange { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            int parameter = 0;

            try {
                if (value.ToString().Length > 0) {
                    parameter = int.Parse(value.ToString(), NumberStyles.Integer, cultureInfo);
                }
            } catch (Exception e) {
                return new ValidationResult(false, "Illegal characters or "
                                             + e.Message);
            }

            if (((parameter < ValidRange.Minimum) || (parameter > ValidRange.Maximum)) && !AllowDefaultValue(parameter)) {
                return new ValidationResult(false,
                    "Please enter value in the range: "
                    + ValidRange.Minimum + " - " + ValidRange.Maximum + ".");
            }

            return new ValidationResult(true, null);
        }

        protected abstract bool AllowDefaultValue(int parameter);
    }

    public class IntRangeRule : IntRangeRuleBase {

        protected override bool AllowDefaultValue(int paramater) {
            return false;
        }
    }

    public class IntRangeRuleWithDefault : IntRangeRuleBase {

        protected override bool AllowDefaultValue(int parameter) {
            return parameter == -1;
        }
    }

    public class IntRangeChecker : DependencyObject {

        public int Minimum {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(IntRangeChecker), new UIPropertyMetadata(int.MinValue));

        public int Maximum {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(IntRangeChecker), new UIPropertyMetadata(int.MaxValue));
    }
}