#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Windows;
using Math = System.Math;
using Point = System.Windows.Point;

namespace NINA.Astrometry {

    public class ViewportFoV {
        public Coordinates AbsoluteCenterCoordinates { get; private set; }
        public Coordinates TopCenter { get; private set; }
        public Coordinates TopLeft { get; private set; }
        public Coordinates BottomLeft { get; private set; }
        public Coordinates BottomCenter { get; private set; }
        public Coordinates CenterCoordinates { get; private set; }
        public double AbsCalcTopDec { get; private set; }
        public double AbsCalcBottomDec { get; private set; }
        public double CalcTopDec { get; private set; }
        public double CalcBottomDec { get; private set; }
        public double VFoVDegTop { get; private set; }
        public double VFoVDegBottom { get; private set; }
        public double VFoVDeg => VFoVDegTop + VFoVDegBottom;
        public double HFoVDeg { get; private set; }
        public bool AboveZero { get; private set; }
        public bool IsAbove90 { get; private set; }
        public double Width { get; }
        public double Height { get; }
        public double ArcSecWidth { get; }
        public double ArcSecHeight { get; }
        public Point ViewPortCenterPoint { get; }
        public double Rotation { get; }
        public double OriginalVFoV { get; }
        public double OriginalHFoV { get; }
        public double OriginalWidth { get; }
        public double OriginalHeight { get; }
        public double CalcRAMin { get; private set; }
        public double CalcRAMax { get; private set; }
        private readonly double horizontalBoundsPadding;
        private readonly double verticalBoundsPadding;

        public ViewportFoV(Coordinates centerCoordinates, double vFoVDegrees, double width, double height, double rotation) {
            Rotation = rotation;

            OriginalWidth = width;
            OriginalHeight = height;

            Width = width;
            Height = height;

            OriginalVFoV = vFoVDegrees;
            OriginalHFoV = (vFoVDegrees / height) * width;

            ArcSecWidth = AstroUtil.DegreeToArcsec(OriginalHFoV) / OriginalWidth;
            ArcSecHeight = AstroUtil.DegreeToArcsec(OriginalVFoV) / OriginalHeight;

            CenterCoordinates = centerCoordinates;

            ViewPortCenterPoint = new Point(width / 2, height / 2);

            Shift(new Vector(0, 0));

            horizontalBoundsPadding = Width / 6;
            verticalBoundsPadding = Height / 6;
        }

        private (double width, double height) GetBoundingBoxOfRotatedRectangle(double rotation, double width, double height) {
            if (rotation == 0) {
                return (width, height);
            }
            var rot = AstroUtil.ToRadians(rotation);

            return (Math.Abs(width * Math.Sin(rot) + height * Math.Cos(rot)), Math.Abs(height * Math.Sin(rot) + width * Math.Cos(rot)));
        }

        public bool ContainsCoordinates(Coordinates coordinates) {
            return ContainsCoordinates(coordinates.RADegrees, coordinates.Dec);
        }

        public bool ContainsCoordinates(double ra, double dec) {
            return ((CalcRAMin > CalcRAMax && (ra > CalcRAMin || ra < CalcRAMax)) // case viewport is going over 0
                    || (ra < CalcRAMax && ra > CalcRAMin) // case viewport is "normal"
                    || IsAbove90 // case dec viewport is above 90deg
                    ) && ((!AboveZero && dec < CalcBottomDec && dec > CalcTopDec) || (AboveZero && dec > CalcBottomDec && dec < CalcTopDec)); // is in between dec
        }

        public bool IsOutOfViewportBounds(Point point) {
            return (point.X < -1 * horizontalBoundsPadding ||
                     point.X > Width + horizontalBoundsPadding ||
                     point.Y < -1 * verticalBoundsPadding ||
                     point.Y > Height + verticalBoundsPadding);
        }

        public bool IsOutOfViewportBounds(System.Drawing.PointF point) {
            return (point.X < -1 * horizontalBoundsPadding ||
                     point.X > Width + horizontalBoundsPadding ||
                     point.Y < -1 * verticalBoundsPadding ||
                     point.Y > Height + verticalBoundsPadding);
        }

        public void Shift(Vector delta) {
            if (delta.X == 0 && delta.Y == 0 && AbsoluteCenterCoordinates != null) {
                return;
            }

            CenterCoordinates = CenterCoordinates.Shift(delta.X, delta.Y, Rotation, ArcSecWidth, ArcSecHeight);

            AbsoluteCenterCoordinates = new Coordinates(CenterCoordinates.RADegrees, Math.Abs(CenterCoordinates.Dec), Epoch.J2000, Coordinates.RAType.Degrees);

            TopCenter = AbsoluteCenterCoordinates.Shift(0, -OriginalVFoV / 2, 0);
            TopLeft = AbsoluteCenterCoordinates.Shift(-OriginalHFoV / 2, (-OriginalVFoV / 2), 0);
            BottomLeft = AbsoluteCenterCoordinates.Shift(-OriginalHFoV / 2, (OriginalVFoV / 2), 0);
            BottomCenter = AbsoluteCenterCoordinates.Shift(0, OriginalVFoV / 2, 0);

            VFoVDegTop = Math.Abs(Math.Max(TopCenter.Dec, TopLeft.Dec) - AbsoluteCenterCoordinates.Dec);
            VFoVDegBottom = Math.Abs(AbsoluteCenterCoordinates.Dec - Math.Min(BottomLeft.Dec, BottomCenter.Dec));

            HFoVDeg = TopLeft.RADegrees > TopCenter.RADegrees
                ? TopLeft.RADegrees - TopCenter.RADegrees
                : TopLeft.RADegrees - TopCenter.RADegrees + 360;
            HFoVDeg *= 2;

            // if we're below 0 we need to flip all calculated decs
            if (CenterCoordinates.Dec < 0) {
                BottomLeft.Dec *= -1;
                TopLeft.Dec *= -1;
                TopCenter.Dec *= -1;
                BottomCenter.Dec *= -1;
                AboveZero = false;
            } else {
                AboveZero = true;
            }

            // if the top center ra is different than the center coordinate ra then the top center point is above 90deg
            if (Math.Abs(TopCenter.RADegrees - CenterCoordinates.RADegrees) > 0.0001) {
                // horizontal fov becomes 360 here and vertical fov the difference between center and 90deg
                HFoVDeg = 360;
                VFoVDegTop = 90 - AbsoluteCenterCoordinates.Dec;
                IsAbove90 = true;
            } else {
                IsAbove90 = false;
            }

            AbsCalcTopDec = AbsoluteCenterCoordinates.Dec + VFoVDegTop;
            AbsCalcBottomDec = AbsoluteCenterCoordinates.Dec - VFoVDegBottom;

            if (AboveZero) {
                CalcTopDec = CenterCoordinates.Dec + VFoVDegTop;
                CalcBottomDec = CenterCoordinates.Dec - VFoVDegBottom;
            } else {
                CalcTopDec = CenterCoordinates.Dec - VFoVDegTop;
                CalcBottomDec = CenterCoordinates.Dec + VFoVDegBottom;
            }

            CalcRAMax = TopLeft.RADegrees;
            CalcRAMin = CalcRAMax - HFoVDeg;
            if (CalcRAMin < 0) {
                CalcRAMin += 360;
            }
        }
    }
}