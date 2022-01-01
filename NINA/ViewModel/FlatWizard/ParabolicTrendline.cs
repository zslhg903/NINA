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
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Regression.Linear;
using OxyPlot.Series;

namespace NINA.ViewModel.FlatWizard {

    public class ParabolicTrendline {

        public ParabolicTrendline(IEnumerable<ScatterErrorPoint> l) {
            DataPoints = l;

            IEnumerable<ScatterErrorPoint> scatterErrorPoints = DataPoints.ToList();
            if (scatterErrorPoints.Count() <= 1) {
                return;
            }

            double[] inputs = scatterErrorPoints.Select(dp => dp.X).ToArray();
            double[] outputs = scatterErrorPoints.Select(dp => dp.Y).ToArray();

            PolynomialLeastSquares pls = new PolynomialLeastSquares { Degree = 2 };
            PolynomialRegression regression = pls.Learn(inputs, outputs);
            A = regression.Weights[0];
            B = regression.Weights[1];
            C = regression.Intercept;
        }

        public double A { get; }
        public double B { get; }
        public double C { get; }

        public IEnumerable<ScatterErrorPoint> DataPoints { get; set; }

        public double[] Solve(double output) {
            var result = new double[2];
            result[0] = (-B + Math.Sqrt(B * B - 4 * A * (C - output))) / (2 * A);
            result[1] = (-B - Math.Sqrt(B * B - 4 * A * (C - output))) / (2 * A);
            return result;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"    y={A}x^2 {B}x {C}");
            sb.AppendLine("    Datapoints:");
            var sortedList = DataPoints.OrderBy(x => x.X);
            foreach (var point in sortedList) {
                sb.AppendLine($"        X: {point.X} Y: {point.Y} Error: {point.ErrorY}");
            }
            return sb.ToString();
        }
    }
}