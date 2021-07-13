#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility;

namespace NINA.WPF.Base.Model.Equipment.MyCamera.Simulator {

    public class SkySurveySettings : BaseINPC {

        public SkySurveySettings() {
            WidthAndHeight = 2500;
            FieldOfView = 1;
            RAError = 0;
            DecError = 0;
        }

        private object lockObj = new object();
        private int decError;

        public int DecError {
            get => decError;
            set {
                lock (lockObj) {
                    decError = value;
                }
                RaisePropertyChanged();
            }
        }

        private int raError;

        public int RAError {
            get => raError;
            set {
                lock (lockObj) {
                    raError = value;
                }
                RaisePropertyChanged();
            }
        }

        private int azShift;

        public int AzShift {
            get => azShift;
            set {
                lock (lockObj) {
                    azShift = value;
                }
                RaisePropertyChanged();
            }
        }

        private int altShift;

        public int AltShift {
            get => altShift;
            set {
                lock (lockObj) {
                    altShift = value;
                }
                RaisePropertyChanged();
            }
        }

        private int widthAndHeight;

        public int WidthAndHeight {
            get => widthAndHeight;
            set {
                lock (lockObj) {
                    widthAndHeight = value;
                }
                RaisePropertyChanged();
            }
        }

        private double fieldOfView;

        public double FieldOfView {
            get => fieldOfView;
            set {
                lock (lockObj) {
                    fieldOfView = value;
                }
                RaisePropertyChanged();
            }
        }
    }
}