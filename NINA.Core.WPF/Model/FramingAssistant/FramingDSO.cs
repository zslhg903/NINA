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
using System;
using System.Drawing;
using System.Linq;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace NINA.WPF.Base.Model.FramingAssistant {

    public class FramingDSO {
        private const int DSO_DEFAULT_SIZE = 30;

        private double arcSecWidth;
        private double arcSecHeight;
        private readonly double sizeWidth;
        private readonly double sizeHeight;
        private readonly float positionAngle;
        private Coordinates coordinates;

        /// <summary>
        /// Constructor for a Framing DSO.
        /// It takes a ViewportFoV and a DeepSkyObject and calculates XY values in pixels from the top left edge of the image subtracting half of its size.
        /// Those coordinates can be used to place the DSO including its name and size in any given image.
        /// </summary>
        /// <param name="dso">The DSO including its coordinates</param>
        /// <param name="viewport">The viewport of the offending DSO</param>
        public FramingDSO(DeepSkyObject dso, ViewportFoV viewport) {
            dsoType = dso.DSOType;
            arcSecWidth = viewport.ArcSecWidth;
            arcSecHeight = viewport.ArcSecHeight;

            if (dso.Size != null && dso.Size >= arcSecWidth) {
                sizeWidth = dso.Size.Value;
            } else {
                sizeWidth = DSO_DEFAULT_SIZE;
            }

            if(dso.PositionAngle.HasValue) {
                positionAngle = (90 - (float)dso.PositionAngle.Value);

                if (dso.SizeMin != null && dso.SizeMin >= arcSecHeight) {
                    sizeHeight = dso.SizeMin.Value;
                } else {
                    sizeHeight = DSO_DEFAULT_SIZE;
                }
            } else {
                positionAngle = 0f;
                sizeHeight = sizeWidth;
            }

            Id = dso.Id;
            Name1 = dso.Name;
            Name2 = dso.AlsoKnownAs.FirstOrDefault(m => m.StartsWith("M "));
            Name3 = dso.AlsoKnownAs.FirstOrDefault(m => m.StartsWith("NGC "));

            if (Name3 != null && Name1 == Name3.Replace(" ", "")) {
                Name1 = null;
            }

            if (Name1 == null && Name2 == null) {
                Name1 = Name3;
                Name3 = null;
            }

            if (Name1 == null && Name2 != null) {
                Name1 = Name2;
                Name2 = Name3;
                Name3 = null;
            }

            coordinates = dso.Coordinates;

            RecalculateTopLeft(viewport);
        }

        public PointF TextPosition { get; private set; }

        public void RecalculateTopLeft(ViewportFoV reference) {
            ViewPortCenter = reference.CenterCoordinates;
            ViewPortCenterPoint = reference.ViewPortCenterPoint;
            ViewPortRotation = reference.Rotation;
            CenterPoint = coordinates.XYProjection(reference);
            arcSecWidth = reference.ArcSecWidth;
            arcSecHeight = reference.ArcSecHeight;
            TextPosition = new PointF((float)CenterPoint.X, (float)(CenterPoint.Y + RadiusHeight + 5));
        }

        public double RadiusWidth => (sizeWidth / arcSecWidth) / 2;

        public double RadiusHeight => (sizeHeight / arcSecHeight) / 2;

        public System.Windows.Point CenterPoint { get; private set; }
        public Coordinates ViewPortCenter { get; private set; }
        public System.Windows.Point ViewPortCenterPoint { get; private set; }
        public double ViewPortRotation { get; private set; }

        public string Id { get; }
        public string Name1 { get; }
        public string Name2 { get; }
        public string Name3 { get; }

        private static SolidBrush dsoFillColorBrush = new SolidBrush(Color.FromArgb(10, 255, 255, 255));

        private static Pen galxyStrokePen = new Pen(Color.FromArgb(128, Color.BurlyWood));
        private static SolidBrush galxyFontColorBrush = new SolidBrush(Color.BurlyWood);

        private static Pen nebulaStrokePen = new Pen(Color.FromArgb(128, Color.Violet));
        private static SolidBrush nebulaFontColorBrush = new SolidBrush(Color.Violet);

        private static Pen plNebulaStrokePen = new Pen(Color.FromArgb(128, Color.Cyan));
        private static SolidBrush plNebulaFontColorBrush = new SolidBrush(Color.Cyan);

        private static Pen gloclStrokePen = new Pen(Color.FromArgb(128, Color.Yellow));
        private static SolidBrush gloclFontColorBrush = new SolidBrush(Color.Yellow);

        private static Pen dsoDefaultStrokePen = new Pen(Color.FromArgb(127, 255, 255, 255));
        private static SolidBrush dsoDefaultFontColorBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));

        private static Font dsoFont = new Font("Segoe UI", 10, System.Drawing.FontStyle.Regular);
        private string dsoType;

        public void Draw(System.Drawing.Graphics g) {
            Pen dsoPen;
            SolidBrush dsoSolidBrush;
            switch (dsoType) {
                case "GALXY":
                case "GALCL":
                    dsoPen = galxyStrokePen;
                    dsoSolidBrush = galxyFontColorBrush;
                    break;

                case "PLNNB":
                    dsoPen = plNebulaStrokePen;
                    dsoSolidBrush = plNebulaFontColorBrush;
                    break;

                case "BRTNB":
                case "CL+NB":
                    dsoPen = nebulaStrokePen;
                    dsoSolidBrush = nebulaFontColorBrush;
                    break;

                case "GLOCL":
                    dsoPen = gloclStrokePen;
                    dsoSolidBrush = gloclFontColorBrush;
                    break;

                default:
                    dsoPen = dsoDefaultStrokePen;
                    dsoSolidBrush = dsoDefaultFontColorBrush;
                    break;
            }

            var panelDeltaX = CenterPoint.X - ViewPortCenterPoint.X;
            var panelDeltaY = CenterPoint.Y - ViewPortCenterPoint.Y;
            var referenceCenter = ViewPortCenter.Shift(panelDeltaX < 1E-10 ? 1 : 0, panelDeltaY, ViewPortRotation, arcSecWidth, arcSecHeight);
            
            float adjustedAngle = positionAngle;
            if (Math.Abs(ViewPortCenter.RA - coordinates.RA) > 1E-13 || Math.Abs(ViewPortCenter.Dec - coordinates.Dec) > 1E-13) {
                adjustedAngle = positionAngle - ( 90 - ((float)AstroUtil.CalculatePositionAngle(referenceCenter.RADegrees, coordinates.RADegrees, referenceCenter.Dec, coordinates.Dec) /*+ (float)ViewPortRotation*/)) ;
            }

            g.TranslateTransform((float)CenterPoint.X, (float)CenterPoint.Y);
            g.RotateTransform(adjustedAngle);
            g.TranslateTransform(-(float)CenterPoint.X, -(float)CenterPoint.Y);
            g.FillEllipse(dsoFillColorBrush, (float)(this.CenterPoint.X - this.RadiusWidth), (float)(this.CenterPoint.Y - this.RadiusHeight),
                    (float)(this.RadiusWidth * 2), (float)(this.RadiusHeight * 2));
            g.DrawEllipse(dsoPen, (float)(this.CenterPoint.X - this.RadiusWidth), (float)(this.CenterPoint.Y - this.RadiusHeight),
                (float)(this.RadiusWidth * 2), (float)(this.RadiusHeight * 2));
            g.ResetTransform();

            var size1 = g.MeasureString(this.Name1, dsoFont);
            g.DrawString(this.Name1, dsoFont, dsoSolidBrush, this.TextPosition.X - size1.Width / 2, (float)(this.TextPosition.Y));
            if (this.Name2 != null) {
                var size2 = g.MeasureString(this.Name2, dsoFont);
                g.DrawString(this.Name2, dsoFont, dsoSolidBrush, this.TextPosition.X - size2.Width / 2, (float)(this.TextPosition.Y + size1.Height + 2));
                if (this.Name3 != null) {
                    var size3 = g.MeasureString(this.Name3, dsoFont);
                    g.DrawString(this.Name3, dsoFont, dsoSolidBrush, this.TextPosition.X - size3.Width / 2, (float)(this.TextPosition.Y + size1.Height + 2 + size2.Height + 2));
                }
            }
        }
    }
}