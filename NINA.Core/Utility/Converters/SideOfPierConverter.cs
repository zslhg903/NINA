﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NINA.Core.Utility.Converters {

    public class SideOfPierConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                PierSide pierSide = (PierSide)value;
                switch (pierSide) {
                    case PierSide.pierEast:
                        return Locale.Loc.Instance["LblEast"];

                    case PierSide.pierWest:
                        return Locale.Loc.Instance["LblWest"];

                    default:
                        return string.Empty;
                }
            } catch (Exception ex) {
                Logger.Error(ex, $"Failed to convert {value} to PierSide");
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}