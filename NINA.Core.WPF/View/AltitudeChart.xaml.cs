#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Astrometry;
using System.Windows;
using System.Windows.Controls;

namespace NINA.WPF.Base.View {

    /// <summary>
    /// Interaction logic for AltitudeChart.xaml
    /// </summary>
    public partial class AltitudeChart : UserControl {

        public AltitudeChart() {
            InitializeComponent();
        }

        public static DependencyProperty NighttimeDataProperty = DependencyProperty.Register("NighttimeData", typeof(NighttimeData), typeof(AltitudeChart));

        public NighttimeData NighttimeData {
            get => (NighttimeData)GetValue(NighttimeDataProperty);
            set => SetValue(NighttimeDataProperty, value);
        }

        public static DependencyProperty AnnotateAltitudeAxisProperty = DependencyProperty.Register("AnnotateAltitudeAxis", typeof(bool), typeof(AltitudeChart), new PropertyMetadata(true));

        public bool AnnotateAltitudeAxis {
            get => (bool)GetValue(AnnotateAltitudeAxisProperty);
            set => SetValue(AnnotateAltitudeAxisProperty, value);
        }

        public static DependencyProperty AnnotateTimeAxisProperty = DependencyProperty.Register("AnnotateTimeAxis", typeof(bool), typeof(AltitudeChart), new PropertyMetadata(true));

        public bool AnnotateTimeAxis {
            get => (bool)GetValue(AnnotateTimeAxisProperty);
            set => SetValue(AnnotateTimeAxisProperty, value);
        }
    }
}