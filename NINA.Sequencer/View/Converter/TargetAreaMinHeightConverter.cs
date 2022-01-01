﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NINA.View.Sequencer.Converter {

    public class TargetAreaMinHeightConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length < 3) { throw new ArgumentException("Must provide three parameters - viewport height, itemcontrol, actual height"); }

            var availableHeight = (double)values[0];
            var itemsControl = (ItemsControl)values[1];
            var contentHeight = (double)values[2] - itemsControl.ActualHeight;
            var headerHeight = 47;

            var first = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
            var last = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(2);

            if (first == null || last == null) {
                return 0;
            } else {
                return Math.Max(100, availableHeight - first.ActualHeight - last.ActualHeight - contentHeight - headerHeight);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}