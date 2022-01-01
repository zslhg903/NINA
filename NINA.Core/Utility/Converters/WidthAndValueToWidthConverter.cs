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
using System.Windows.Data;

namespace NINA.Core.Utility.Converters {

    /// <summary>
    /// Very specific converter to convert width and a value up to 0.5 to a new width
    /// </summary>
    public class WidthAndValueToWidthConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            double width;
            double value;
            double.TryParse(values[0].ToString(), out width);
            double.TryParse(values[1].ToString(), out value);
            value = value - 0.5;
            if (value < 0) value *= -1;
            return width * value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}