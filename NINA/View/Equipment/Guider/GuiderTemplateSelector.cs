#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Equipment.Equipment.MyGuider;
using NINA.Equipment.Equipment.MyGuider.PHD2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NINA.View.Equipment.Guider {

    internal class GuiderTemplateSelector : DataTemplateSelector {
        public DataTemplate MGen { get; set; }
        public DataTemplate PHD2 { get; set; }
        public DataTemplate MetaGuide { get; set; }
        public DataTemplate DirectGuider { get; set; }
        public DataTemplate Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item is MGENGuider) {
                return MGen;
            } else if (item is PHD2Guider) {
                return PHD2;
            } else if (item is MetaGuideGuider) {
                return MetaGuide;
            } else if (item is DirectGuider) {
                return DirectGuider;
            } else {
                return Default;
            }
        }
    }
}