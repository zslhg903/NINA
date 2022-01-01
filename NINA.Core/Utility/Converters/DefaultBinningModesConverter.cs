﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Model.Equipment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace NINA.Core.Utility.Converters {

    public class DefaultBinningModesConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var list = value as IList<BinningMode>;
            if (list == null || list.Count == 0) {
                return new List<BinningMode> {
                    new BinningMode(1,1),
                    new BinningMode(2,2),
                    new BinningMode(3,3),
                    new BinningMode(4,4),
                };
            } else {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}