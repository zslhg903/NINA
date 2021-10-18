#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NINA.Core.Model;
using NINA.Image.ImageAnalysis;
using NINA.Image.Interfaces;

namespace NINA.Image.ImageData {

    public class RenderedImage : IRenderedImage {
        public IImageData RawImageData { get; private set; }

        public BitmapSource Image { get; private set; }

        public RenderedImage(BitmapSource image, IImageData rawImageData) {
            this.Image = image;
            this.RawImageData = rawImageData;
        }

        public static async Task<IRenderedImage> FromBitmapSource(BitmapSource source, bool calculateStatistics = false) {
            var exposureData = await ImageArrayExposureData.FromBitmapSource(source);
            var rawImageData = await exposureData.ToImageData();
            return Create(source: source, rawImageData: rawImageData, calculateStatistics: calculateStatistics);
        }

        public static RenderedImage Create(BitmapSource source, IImageData rawImageData, bool calculateStatistics = false) {
            return new RenderedImage(image: source, rawImageData: rawImageData);
        }

        public virtual IRenderedImage ReRender() {
            return new RenderedImage(image: this.RawImageData.RenderBitmapSource(), rawImageData: this.RawImageData);
        }

        public IDebayeredImage Debayer(bool saveColorChannels = false, bool saveLumChannel = false, SensorType bayerPattern = SensorType.RGGB) {
            return DebayeredImage.Debayer(this, saveColorChannels: saveColorChannels, saveLumChannel: saveLumChannel, bayerPattern: bayerPattern);
        }

        public virtual async Task<IRenderedImage> Stretch(double factor, double blackClipping, bool unlinked) {
            var stretchedImage = await ImageUtility.Stretch(this, factor, blackClipping);
            return new RenderedImage(image: stretchedImage, rawImageData: this.RawImageData);
        }

        public async Task<IRenderedImage> DetectStars(
            bool annotateImage,
            StarSensitivityEnum sensitivity,
            NoiseReductionEnum noiseReduction,
            CancellationToken cancelToken = default,
            IProgress<ApplicationStatus> progress = default(Progress<ApplicationStatus>)) {
            var starDetection = new StarDetection();
            var starDetectionParams = new StarDetectionParams() {
                Sensitivity = sensitivity,
                NoiseReduction = noiseReduction
            };
            var starDetectionResult = await starDetection.Detect(this, this.Image.Format, starDetectionParams, progress, cancelToken);
            var image = this.Image;
            if (annotateImage) {
                var starAnnotator = new StarAnnotator();
                image = await starAnnotator.GetAnnotatedImage(starDetectionParams, starDetectionResult, this.Image, token: cancelToken);
            }

            this.RawImageData.StarDetectionAnalysis.HFR = starDetectionResult.AverageHFR;
            this.RawImageData.StarDetectionAnalysis.DetectedStars = starDetectionResult.DetectedStars;
            return new RenderedImage(image: image, rawImageData: this.RawImageData);
        }

        public async Task<BitmapSource> GetThumbnail() {
            BitmapSource image = null;
            await _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                var factor = 300 / this.Image.Width;
                image = new WriteableBitmap(new TransformedBitmap(this.Image, new ScaleTransform(factor, factor)));
                image.Freeze();
            }));
            return image;
        }

        private static Dispatcher _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
}