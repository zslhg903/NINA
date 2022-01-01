﻿#region "copyright"
/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NINA.ViewModel.Plugins {

    internal class PluginOptionsDataTemplateSelector : DataTemplateSelector {
        public DataTemplate Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var plugin = item as IPluginManifest;
            if (plugin != null) {
                if (Application.Current.Resources.Contains(plugin.Name + "_Options")) {
                    return (DataTemplate)Application.Current.Resources[plugin.Name + "_Options"];
                }
            }
            return Default;
        }
    }
}