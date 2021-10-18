﻿using Accord.Imaging.Filters;
using NINA.Core.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NINA.Image.ImageAnalysis {

    public class StarAnnotator : IStarAnnotator {
        private static Pen ELLIPSEPEN = new Pen(Brushes.LightYellow, 1);
        private static Pen RECTPEN = new Pen(Brushes.LightYellow, 2);
        private static SolidBrush TEXTBRUSH = new SolidBrush(Color.Yellow);
        private static FontFamily FONTFAMILY = new FontFamily("Arial");
        private static Font FONT = new Font(FONTFAMILY, 32, FontStyle.Regular, GraphicsUnit.Pixel);

        public Task<BitmapSource> GetAnnotatedImage(StarDetectionParams p, StarDetectionResult result, BitmapSource imageToAnnotate, int maxStars = 200, CancellationToken token = default) {
            return Task.Run(() => {
                using (MyStopWatch.Measure()) {
                    if (imageToAnnotate.Format == System.Windows.Media.PixelFormats.Rgb48) {
                        using (var source = ImageUtility.BitmapFromSource(imageToAnnotate, System.Drawing.Imaging.PixelFormat.Format48bppRgb)) {
                            using (var img = new Grayscale(0.2125, 0.7154, 0.0721).Apply(source)) {
                                imageToAnnotate = ImageUtility.ConvertBitmap(img, System.Windows.Media.PixelFormats.Gray16);
                                imageToAnnotate.Freeze();
                            }
                        }
                    }

                    using (var bmp = ImageUtility.Convert16BppTo8Bpp(imageToAnnotate)) {
                        using (var newBitmap = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb)) {
                            Graphics graphics = Graphics.FromImage(newBitmap);
                            graphics.DrawImage(bmp, 0, 0);
                            var starList = result.StarList;

                            if (starList.Count > 0) {
                                int offset = 10;
                                float textposx, textposy;

                                if (maxStars > 0 && starList.Count > maxStars) {
                                    starList = new List<DetectedStar>(starList);

                                    starList.Sort((item1, item2) => item2.AverageBrightness.CompareTo(item1.AverageBrightness));
                                    starList = starList.GetRange(0, maxStars);
                                }

                                foreach (var star in starList) {
                                    token.ThrowIfCancellationRequested();
                                    textposx = star.Position.X - offset;
                                    textposy = star.Position.Y - offset;
                                    graphics.DrawEllipse(ELLIPSEPEN, new RectangleF(star.BoundingBox.X, star.BoundingBox.Y, star.BoundingBox.Width, star.BoundingBox.Height));
                                    graphics.DrawString(star.HFR.ToString("##.##"), FONT, TEXTBRUSH, new PointF(Convert.ToSingle(textposx - 1.5 * offset), Convert.ToSingle(textposy + 2.5 * offset)));
                                }
                            }

                            if (p.UseROI) {
                                graphics.DrawRectangle(RECTPEN, (float)(1 - p.InnerCropRatio) * imageToAnnotate.PixelWidth / 2, (float)(1 - p.InnerCropRatio) * imageToAnnotate.PixelHeight / 2, (float)p.InnerCropRatio * imageToAnnotate.PixelWidth, (float)p.InnerCropRatio * imageToAnnotate.PixelHeight);
                                if (p.OuterCropRatio < 1) {
                                    graphics.DrawRectangle(RECTPEN, (float)(1 - p.OuterCropRatio) * imageToAnnotate.PixelWidth / 2, (float)(1 - p.OuterCropRatio) * imageToAnnotate.PixelHeight / 2, (float)p.OuterCropRatio * imageToAnnotate.PixelWidth, (float)p.OuterCropRatio * imageToAnnotate.PixelHeight);
                                }
                            }

                            var img = ImageUtility.ConvertBitmap(newBitmap, System.Windows.Media.PixelFormats.Bgr24);

                            img.Freeze();
                            return img;
                        }
                    }
                }
            });
        }
    }
}