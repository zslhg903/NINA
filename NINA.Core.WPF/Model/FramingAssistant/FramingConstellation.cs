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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NINA.WPF.Base.Model.FramingAssistant {

    public class FramingConstellation {
        private Constellation constellation;

        public FramingConstellation(Constellation constellation, ViewportFoV viewport) {
            this.constellation = constellation;

            Id = constellation.Id;
            Name = constellation.Name;

            var constellationStartDec = constellation.Stars.Select(m => m.Coords.Dec).Min();
            var constellationStopDec = constellation.Stars.Select(m => m.Coords.Dec).Max();

            if (constellation.GoesOverRaZero) {
                double stopRA = double.MaxValue;
                double startRA = 0;
                foreach (var star in constellation.Stars) {
                    if (star.Coords.RADegrees > 180) {
                        stopRA = Math.Min(stopRA, star.Coords.RADegrees);
                    } else {
                        startRA = Math.Max(startRA, star.Coords.RADegrees);
                    }
                }
                if (stopRA == double.MaxValue) {
                    stopRA = 0;
                }

                var distance = startRA + 360 - stopRA;

                var centerRa = stopRA + distance / 2;
                if (centerRa > 360) {
                    centerRa -= 360;
                }

                constellationCenter = new Coordinates(centerRa,
                    constellationStopDec + (constellationStartDec - constellationStopDec) / 2, Epoch.J2000, Coordinates.RAType.Degrees);
            } else {
                var constellationStartRA = constellation.Stars.Select(m => m.Coords.RADegrees).Min();
                var constellationStopRA = constellation.Stars.Select(m => m.Coords.RADegrees).Max();

                constellationCenter = new Coordinates(
                    constellationStopRA + (constellationStartRA - constellationStopRA) / 2,
                    constellationStopDec + (constellationStartDec - constellationStopDec) / 2, Epoch.J2000,
                    Coordinates.RAType.Degrees);
            }

            Points = new HashSet<Tuple<Star, Star>>();
            Stars = new HashSet<Star>();

            foreach (var star in constellation.Stars) {
                star.Radius = (-3.375f * star.Mag + 23.25f) / (float)(viewport.VFoVDeg / 8f);
            }
        }

        private readonly Coordinates constellationCenter;

        public void RecalculateConstellationPoints(ViewportFoV reference, bool calculateConnections) {
            // calculate all star positions for the constellation once and add them to the star collection for drawing if they're visible
            foreach (var star in constellation.Stars) {
                var starPosition = star.Coords.XYProjection(reference);
                star.Position = new PointF((float)starPosition.X, (float)starPosition.Y);
                var isInBounds = !reference.IsOutOfViewportBounds(star.Position);
                var contains = Stars.Contains(star);
                if (isInBounds && !contains) {
                    Stars.Add(star);
                } else if (!isInBounds && contains) {
                    Stars.Remove(star);
                }
            }

            if (calculateConnections) {
                // now we check what lines are visible in the fov and only add those connections as well
                foreach (var starConnection in constellation.StarConnections) {
                    var isInBounds = !(reference.IsOutOfViewportBounds(starConnection.Item1.Position) &&
                                       reference.IsOutOfViewportBounds(starConnection.Item2.Position));
                    var contains = Points.Contains(starConnection);
                    if (isInBounds && !contains) {
                        Points.Add(starConnection);
                    } else if (!isInBounds && contains) {
                        Points.Remove(starConnection);
                    }
                }

                var p = constellationCenter.XYProjection(reference);
                CenterPoint = new PointF((float)p.X, (float)p.Y);
            }
        }

        public void DrawAnnotations(Graphics g) {
            var constellationSize = g.MeasureString(this.Name, font);
            g.DrawString(this.Name, font, constColorBrush, (this.CenterPoint.X - constellationSize.Width / 2), (this.CenterPoint.Y));
            foreach (var starConnection in this.Points) {
                g.DrawLine(constLinePen, starConnection.Item1.Position.X,
                starConnection.Item1.Position.Y, starConnection.Item2.Position.X,
                starConnection.Item2.Position.Y);
            }

            foreach (var star in this.Stars) {
                var size = g.MeasureString(star.Name, starfont);
                g.DrawString(star.Name, starfont, starFontColorBrush, (star.Position.X + star.Radius - size.Width / 2), (star.Position.Y + star.Radius * 2 + 5));
            }
        }

        public void DrawStars(Graphics g) {
            foreach (var star in this.Stars) {
                g.FillEllipse(starColorBrush, (star.Position.X - star.Radius), (star.Position.Y - star.Radius), star.Radius * 2, star.Radius * 2);
            }
        }

        private static Font font = new Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
        private static Font starfont = new Font("Segoe UI", 8, System.Drawing.FontStyle.Italic);
        private static SolidBrush constColorBrush = new SolidBrush(Color.FromArgb(128, 255, 255, 153));
        private static Pen starPen = new Pen(Color.FromArgb(128, 255, 255, 255));
        private static Pen constLinePen = new Pen(Color.FromArgb(128, 0, 255, 0));
        private static SolidBrush starFontColorBrush = new SolidBrush(Color.FromArgb(128, 255, 215, 0));
        private static SolidBrush starColorBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));

        public PointF CenterPoint { get; private set; }

        public string Id { get; }
        public string Name { get; }

        public HashSet<Star> Stars { get; private set; }

        public HashSet<Tuple<Star, Star>> Points { get; private set; }
    }
}