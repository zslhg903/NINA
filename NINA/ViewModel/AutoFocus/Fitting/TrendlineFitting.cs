﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Utility;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.ViewModel.AutoFocus {

    public class TrendlineFitting : BaseINPC {

        public TrendlineFitting() {
        }

        private Trendline _leftTrend;

        public Trendline LeftTrend {
            get {
                return _leftTrend;
            }
            set {
                _leftTrend = value;
                RaisePropertyChanged();
            }
        }

        private Trendline _rightTrend;

        public Trendline RightTrend {
            get {
                return _rightTrend;
            }
            set {
                _rightTrend = value;
                RaisePropertyChanged();
            }
        }

        private DataPoint _intersection;

        public DataPoint Intersection {
            get {
                return _intersection;
            }
            set {
                _intersection = value;
                RaisePropertyChanged();
            }
        }

        private ScatterErrorPoint _minimum;

        public ScatterErrorPoint Minimum {
            get {
                return _minimum;
            }
            set {
                _minimum = value;
                RaisePropertyChanged();
            }
        }

        public TrendlineFitting Calculate(ICollection<ScatterErrorPoint> points) {
            //if (profileService.ActiveProfile.FocuserSettings.AutoFocusMethod == AFMethodEnum.STARHFR) {
            //Get the minimum based on HFR and Error, rather than just HFR. This ensures 0 HFR is never used, and low HFR / High error numbers are also ignored
            Minimum = points.Aggregate((l, r) => l.Y + l.ErrorY < r.Y + r.ErrorY ? l : r);
            IEnumerable<ScatterErrorPoint> leftTrendPoints = points.Where((x) => x.X < Minimum.X && x.Y > (Minimum.Y + 0.1));
            IEnumerable<ScatterErrorPoint> rightTrendPoints = points.Where((x) => x.X > Minimum.X && x.Y > (Minimum.Y + 0.1));
            LeftTrend = new Trendline(leftTrendPoints);
            RightTrend = new Trendline(rightTrendPoints);
            Intersection = LeftTrend.Intersect(RightTrend);
            /*} else {
                var max = FocusPoints.Aggregate((l, r) => l.Y - l.ErrorY > r.Y - r.ErrorY ? l : r);
                IEnumerable<ScatterErrorPoint> leftTrendPoints = FocusPoints.Where((x) => x.X < max.X && x.Y < (max.Y - 0.01));
                IEnumerable<ScatterErrorPoint> rightTrendPoints = FocusPoints.Where((x) => x.X > max.X && x.Y < (max.Y - 0.01));
                LeftTrend = new TrendLine(leftTrendPoints);
                RightTrend = new TrendLine(rightTrendPoints);
            }*/
            return this;
        }
    }
}