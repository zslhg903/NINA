#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using Accord.Imaging;
using NINA.Core.Enum;
using NINA.Core.Locale;
using NINA.Core.Utility;
using NINA.Image.ImageData;
using NINA.Image.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NINA.Image.ImageAnalysis {

    public class ImageUtility {

        public static ColorRemappingGeneral GetColorRemappingFilter(
            IImageStatistics statistics,
            double targetHistogramMeanPct,
            double shadowsClipping,
            System.Windows.Media.PixelFormat pf) {
            ushort[] map = GetStretchMap(statistics, targetHistogramMeanPct, shadowsClipping);

            if (pf == PixelFormats.Gray16) {
                var filter = new ColorRemappingGeneral(map);
                return filter;
            } else if (pf == PixelFormats.Rgb48) {
                var filter = new ColorRemappingGeneral(map, map, map);
                return filter;
            } else {
                throw new NotSupportedException();
            }
        }

        public static ColorRemappingGeneral GetColorRemappingFilterUnlinked(
            IImageStatistics redStatistics,
            IImageStatistics greenStatistics,
            IImageStatistics blueStatistics,
            double targetHistogramMeanPct,
            double shadowsClipping,
            System.Windows.Media.PixelFormat pf) {
            ushort[] mapRed = GetStretchMap(redStatistics, targetHistogramMeanPct, shadowsClipping);
            ushort[] mapGreen = GetStretchMap(greenStatistics, targetHistogramMeanPct, shadowsClipping);
            ushort[] mapBlue = GetStretchMap(blueStatistics, targetHistogramMeanPct, shadowsClipping);
            if (pf == PixelFormats.Rgb48) {
                var filter = new ColorRemappingGeneral(mapRed, mapGreen, mapBlue);
                return filter;
            } else {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Adjusts x for a given midToneBalance
        /// </summary>
        /// <param name="midToneBalance"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double MidtonesTransferFunction(double midToneBalance, double x) {
            if (x > 0) {
                if (x < 1) {
                    return (midToneBalance - 1) * x / ((2 * midToneBalance - 1) * x - midToneBalance);
                }
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Converts a value from range [0;65535] to [0;1]
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static double NormalizeUShort(double val, int bitDepth) {
            return val / (double)(1 << bitDepth);
        }

        /// <summary>
        /// Converts a value from range [0;1] to [0;65535]
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static ushort DenormalizeUShort(double val) {
            return (ushort)(val * ushort.MaxValue);
        }

        private static ushort[] GetStretchMap(IImageStatistics statistics, double targetHistogramMedianPercent, double shadowsClipping) {
            ushort[] map = new ushort[ushort.MaxValue + 1];

            var normalizedMedian = NormalizeUShort(statistics.Median, statistics.BitDepth);
            var normalizedMAD = NormalizeUShort(statistics.MedianAbsoluteDeviation, statistics.BitDepth);

            var scaleFactor = 1.4826; // see https://en.wikipedia.org/wiki/Median_absolute_deviation

            double shadows = 0d;
            double midtones = 0.5d;
            double highlights = 1d;

            //Assume the image is inverted or overexposed when median is higher than half of the possible value
            if (normalizedMedian > 0.5) {
                shadows = 0.0d;
                highlights = normalizedMedian - shadowsClipping * normalizedMAD * scaleFactor;
                midtones = MidtonesTransferFunction(targetHistogramMedianPercent, 1.0 - (highlights - normalizedMedian));
            } else {
                shadows = normalizedMedian + shadowsClipping * normalizedMAD * scaleFactor;
                midtones = MidtonesTransferFunction(targetHistogramMedianPercent, normalizedMedian - shadows);
                highlights = 1;
            }

            for (int i = 0; i < map.Length; i++) {
                double value = NormalizeUShort(i, statistics.BitDepth);

                map[i] = DenormalizeUShort(MidtonesTransferFunction(midtones, 1 - highlights + value - shadows));
            }

            return map;
        }

        public static BitmapSource ConvertBitmap(System.Drawing.Bitmap bitmap) {
            System.Windows.Media.PixelFormat pf;

            switch (bitmap.PixelFormat) {
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    pf = System.Windows.Media.PixelFormats.Bgr565;
                    break;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    pf = System.Windows.Media.PixelFormats.Bgra32;
                    break;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    pf = System.Windows.Media.PixelFormats.Bgra32;
                    break;

                default:
                    pf = System.Windows.Media.PixelFormats.Gray16;
                    break;
            }
            return ConvertBitmap(bitmap, pf);
        }

        public static BitmapSource ConvertBitmap(System.Drawing.Bitmap bitmap, System.Windows.Media.PixelFormat pf) {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, pf, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static Bitmap BitmapFromSource(BitmapSource source) {
            return BitmapFromSource(source, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
        }

        public static Bitmap BitmapFromSource(BitmapSource source, System.Drawing.Imaging.PixelFormat pf) {
            Bitmap bmp = new Bitmap(
                    source.PixelWidth,
                    source.PixelHeight,
                    pf);
            BitmapData data = bmp.LockBits(
                    new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                    ImageLockMode.WriteOnly,
                    pf);
            source.CopyPixels(
                    Int32Rect.Empty,
                    data.Scan0,
                    data.Height * data.Stride,
                    data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static Bitmap Convert16BppTo8Bpp(BitmapSource source) {
            using(MyStopWatch.Measure()) { 
                using (var bmp = BitmapFromSource(source)) {
                    return Accord.Imaging.Image.Convert16bppTo8bpp(bmp);
                }
            }
        }

        public static BitmapSource Convert16BppTo8BppSource(BitmapSource source) {
            FormatConvertedBitmap s = new FormatConvertedBitmap();
            s.BeginInit();
            s.Source = source;
            s.DestinationFormat = System.Windows.Media.PixelFormats.Gray8;
            s.EndInit();
            s.Freeze();
            return s;
        }

        public static BitmapSource CreateSourceFromArray(IImageArray arr, ImageProperties props, System.Windows.Media.PixelFormat pf) {
            //int stride = C.CameraYSize * ((Convert.ToString(C.MaxADU, 2)).Length + 7) / 8;
            int stride = (props.Width * pf.BitsPerPixel + 7) / 8;
            double dpi = 96;

            BitmapSource source = BitmapSource.Create(props.Width, props.Height, dpi, dpi, pf, null, arr.FlatArray, stride);
            source.Freeze();
            return source;
        }

        public static DebayeredImageData Debayer(BitmapSource source, System.Drawing.Imaging.PixelFormat pf, bool saveColorChannels = false, bool saveLumChannel = false, SensorType bayerPattern = SensorType.RGGB) {
            using (MyStopWatch.Measure()) {
                if (pf != System.Drawing.Imaging.PixelFormat.Format16bppGrayScale) {
                    throw new NotSupportedException();
                }
                using (var bmp = BitmapFromSource(source, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)) {
                    return Debayer(bmp, saveColorChannels, saveLumChannel, bayerPattern);
                }
            }
        }

        public static DebayeredImageData Debayer(Bitmap bmp, bool saveColorChannels = false, bool saveLumChannel = false, SensorType bayerPattern = SensorType.RGGB) {
            using (MyStopWatch.Measure()) {
                var filter = new BayerFilter16bpp();
                filter.SaveColorChannels = saveColorChannels;
                filter.SaveLumChannel = saveLumChannel;

                Logger.Debug($"Debayering pattern {bayerPattern}");

                switch (bayerPattern) {
                    case SensorType.RGGB:
                        filter.BayerPattern = new int[,] { { RGB.B, RGB.G }, { RGB.G, RGB.R } };
                        break;

                    case SensorType.RGBG:
                        filter.BayerPattern = new int[,] { { RGB.G, RGB.B }, { RGB.G, RGB.R } };
                        break;

                    case SensorType.GRGB:
                        filter.BayerPattern = new int[,] { { RGB.B, RGB.G }, { RGB.R, RGB.G } };
                        break;

                    case SensorType.GRBG:
                        filter.BayerPattern = new int[,] { { RGB.G, RGB.B }, { RGB.R, RGB.G } };
                        break;

                    case SensorType.GBGR:
                        filter.BayerPattern = new int[,] { { RGB.R, RGB.G }, { RGB.B, RGB.G } };
                        break;

                    case SensorType.GBRG:
                        filter.BayerPattern = new int[,] { { RGB.G, RGB.R }, { RGB.B, RGB.G } };
                        break;

                    case SensorType.BGRG:
                        filter.BayerPattern = new int[,] { { RGB.G, RGB.R }, { RGB.G, RGB.B } };
                        break;

                    case SensorType.BGGR:
                        filter.BayerPattern = new int[,] { { RGB.R, RGB.G }, { RGB.G, RGB.B } };
                        break;

                    default:
                        throw new InvalidImagePropertiesException(string.Format(Loc.Instance["LblUnsupportedCfaPattern"], bayerPattern));
                }

                DebayeredImageData debayered = new DebayeredImageData();
                debayered.ImageSource = ConvertBitmap(filter.Apply(bmp), PixelFormats.Rgb48);
                debayered.ImageSource.Freeze();
                debayered.Data = filter.LRGBArrays;
                return debayered;
            }
        }

        public static ColorPalette GetGrayScalePalette() {
            using (var bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed)) {
                ColorPalette monoPalette = bmp.Palette;

                System.Drawing.Color[] entries = monoPalette.Entries;

                for (int i = 0; i < 256; i++) {
                    entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                }

                return monoPalette;
            }
        }

        public static Task<BitmapSource> Stretch(IRenderedImage image, double factor, double blackClipping) {
            return Task.Run(async () => {
                var imageStatistics = await image.RawImageData.Statistics.Task;
                if (image.OriginalImage.Format == PixelFormats.Gray16) {
                    using (var bmp = ImageUtility.BitmapFromSource(image.OriginalImage, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)) {
                        return Stretch(imageStatistics, bmp, image.OriginalImage.Format, factor, blackClipping);
                    }
                } else if (image.OriginalImage.Format == PixelFormats.Rgb48) {
                    using (var bmp = ImageUtility.BitmapFromSource(image.OriginalImage, System.Drawing.Imaging.PixelFormat.Format48bppRgb)) {
                        return Stretch(imageStatistics, bmp, image.OriginalImage.Format, factor, blackClipping);
                    }
                } else {
                    throw new NotSupportedException();
                }
            });
        }

        public static Task<BitmapSource> StretchUnlinked(IDebayeredImage data, double factor, double blackClipping) {
            return Task.Run(async () => {
                if (data.OriginalImage.Format != PixelFormats.Rgb48) {
                    throw new NotSupportedException();
                } else {
                    var asyncR = Task.Run(() => ImageData.ImageStatistics.Create(data.RawImageData.Properties, data.DebayeredData.Red));
                    var asyncG = Task.Run(() => ImageData.ImageStatistics.Create(data.RawImageData.Properties, data.DebayeredData.Green));
                    var asyncB = Task.Run(() => ImageData.ImageStatistics.Create(data.RawImageData.Properties, data.DebayeredData.Blue));
                    await Task.WhenAll(asyncR, asyncG, asyncB);
                    using (var img = ImageUtility.BitmapFromSource(data.OriginalImage, System.Drawing.Imaging.PixelFormat.Format48bppRgb)) {
                        return StretchUnlinked(asyncR.Result, asyncG.Result, asyncB.Result, img, data.OriginalImage.Format, factor, blackClipping);
                    }
                }
            });
        }

        public static BitmapSource StretchUnlinked(
            IImageStatistics redStatistics,
            IImageStatistics greenStatistics,
            IImageStatistics blueStatistics,
            Bitmap img,
            System.Windows.Media.PixelFormat pf,
            double factor,
            double blackClipping) {
            using (MyStopWatch.Measure()) {
                var filter = ImageUtility.GetColorRemappingFilterUnlinked(redStatistics, greenStatistics, blueStatistics, factor, blackClipping, pf);
                filter.ApplyInPlace(img);

                var source = ImageUtility.ConvertBitmap(img, pf);
                source.Freeze();
                return source;
            }
        }

        public static BitmapSource Stretch(IImageStatistics statistics, Bitmap img, System.Windows.Media.PixelFormat pf, double factor, double blackClipping) {
            using (MyStopWatch.Measure()) {
                var filter = ImageUtility.GetColorRemappingFilter(statistics, factor, blackClipping, pf);
                filter.ApplyInPlace(img);

                var source = ImageUtility.ConvertBitmap(img, pf);
                source.Freeze();
                return source;
            }
        }
    }
}