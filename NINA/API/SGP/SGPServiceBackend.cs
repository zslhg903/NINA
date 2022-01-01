﻿#region "copyright"
/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using NINA.Core.API.SGP;
using NINA.Core.Enum;
using NINA.Core.Interfaces.API.SGP;
using NINA.Core.Model.Equipment;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Equipment.Model;
using NINA.Image.FileFormat;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.API.SGP {

    public class SGPServiceBackend : ISGPServiceBackend {

        private struct ImageCaptureReceipt {
            public Guid Receipt;
            public string Path;
            public Task<string> SavePathTask;
        }

        private static readonly TimeSpan RECEIPT_TIMEOUT = TimeSpan.FromHours(2);
        private readonly ICameraVM cameraVM;
        private readonly IImagingMediator imagingMediator;
        private readonly MemoryCache imageCaptureReceipts;

        public SGPServiceBackend(ICameraVM cameraVM, IImagingMediator imagingMediator) {
            this.cameraVM = cameraVM;
            this.imagingMediator = imagingMediator;
            this.imageCaptureReceipts = new MemoryCache("SGP Image Receipts");
        }

        public SgAbortImageResponse AbortImage() {
            var cameraInfo = cameraVM.GetDeviceInfo();
            if (cameraInfo?.Connected != true) {
                return new SgAbortImageResponse() {
                    Success = false,
                    Message = "No camera is connected"
                };
            }
            cameraVM.AbortExposure();
            return new SgAbortImageResponse() {
                Success = true
            };
        }

        public SgCaptureImageResponse CaptureImage(SgCaptureImage input) {
            var cameraInfo = cameraVM.GetDeviceInfo();
            if (cameraInfo.Connected != true) {
                return new SgCaptureImageResponse() {
                    Success = false,
                    Message = "No camera is connected"
                };
            }
            if (cameraInfo.IsExposing) {
                return new SgCaptureImageResponse() {
                    Success = false,
                    Message = "Camera is busy"
                };
            }

            // TODO: If ISO is provided, figure out a way to map it to a gain value
            var gain = -1;
            if (!String.IsNullOrEmpty(input.Gain)) {
                gain = int.Parse(input.Gain);
            }

            var imageType = CaptureSequence.ImageTypes.LIGHT;
            if (!string.IsNullOrEmpty(input.FrameType)) {
                var frameType = (ImageType)Enum.Parse(typeof(ImageType), input.FrameType);
                switch (frameType) {
                    case ImageType.Light:
                        imageType = CaptureSequence.ImageTypes.LIGHT;
                        break;

                    case ImageType.Bias:
                        imageType = CaptureSequence.ImageTypes.BIAS;
                        break;

                    case ImageType.Dark:
                        imageType = CaptureSequence.ImageTypes.DARK;
                        break;

                    case ImageType.Flat:
                        imageType = CaptureSequence.ImageTypes.FLAT;
                        break;
                }
            }

            var captureSequence = new CaptureSequence() {
                ExposureTime = input.ExposureLength,
                ImageType = imageType,
                TotalExposureCount = 1,
                Dither = false,
                Binning = new BinningMode((short)input.BinningMode, (short)input.BinningMode),
                Gain = gain
            };

            if (input.UseSubframe == true) {
                if (!input.X.HasValue || !input.Y.HasValue || !input.Width.HasValue || !input.Height.HasValue) {
                    throw new ArgumentException("If UseSubframe enabled, you must provide X, Y, Width, and Height");
                }
                if (!cameraInfo.CanSubSample) {
                    return new SgCaptureImageResponse() {
                        Success = false,
                        Message = "Camera does not support subframes"
                    };
                }

                captureSequence.EnableSubSample = true;
                captureSequence.SubSambleRectangle = new ObservableRectangle(input.X.Value, input.Y.Value, input.Width.Value, input.Height.Value);
            }

            var inputDirectory = Path.GetDirectoryName(input.Path);
            var inputFilenameWithoutExtension = Path.GetFileNameWithoutExtension(input.Path);
            var inputExtension = Path.GetExtension(input.Path);
            var savePathTask = Task.Run(async () => {
                var exposureData = await imagingMediator.CaptureImage(captureSequence, CancellationToken.None, null);
                var imageData = await exposureData.ToImageData();
                var fileSaveInfo = new FileSaveInfo() {
                    FileType = FileTypeEnum.FITS,
                    ForceExtension = inputExtension,
                    FilePath = inputDirectory,
                    FilePattern = inputFilenameWithoutExtension
                };
                return await imageData.SaveToDisk(fileSaveInfo, forceFileType: true);
            });
            var receipt = new ImageCaptureReceipt() {
                Receipt = Guid.NewGuid(),
                Path = input.Path,
                SavePathTask = savePathTask
            };
            imageCaptureReceipts.Add(receipt.Receipt.ToString(), receipt, DateTime.Now.Add(RECEIPT_TIMEOUT));
            return new SgCaptureImageResponse() {
                Success = true,
                Receipt = receipt.Receipt
            };
        }

        public async Task<SgConnectDeviceResponse> ConnectDevice(SgConnectDevice input) {
            Logger.Trace(String.Format("SGP API: ConnectDevice {0} {1}", input.Device, input.DeviceName));
            if (input.Device == DeviceType.Camera) {
                var response = new SgConnectDeviceResponse();
                var cameraChooserVM = cameraVM.CameraChooserVM;
                var selectedDevice = cameraChooserVM.Devices.FirstOrDefault(d => d.Name == input.DeviceName);
                if (selectedDevice == null) {
                    response.Success = false;
                    response.Message = String.Format("Device {0} not found", input.DeviceName);
                    return response;
                }

                if (cameraChooserVM.SelectedDevice == selectedDevice && cameraVM.GetDeviceInfo().Connected == true) {
                    Logger.Trace(String.Format("Camera device {0} already connected", input.DeviceName));
                    response.Success = true;
                } else {
                    cameraChooserVM.SelectedDevice = selectedDevice;
                    response.Success = await cameraVM.Connect();
                    if (!response.Success) {
                        response.Message = "Failed to connect device";
                    }
                }
                return response;
            } else {
                return new SgConnectDeviceResponse() {
                    Success = false,
                    Message = "Only Camera implemented at this time"
                };
            }
        }

        public async Task<SgDisconnectDeviceResponse> DisconnectDevice(SgDisconnectDevice input) {
            Logger.Trace(String.Format("SGP API: DisconnectDevice {0}", input.Device));
            if (input.Device == DeviceType.Camera) {
                await cameraVM.Disconnect();
                return new SgDisconnectDeviceResponse() {
                    Success = true,
                };
            } else {
                return new SgDisconnectDeviceResponse() {
                    Success = false,
                    Message = "Only Camera implemented at this time"
                };
            }
        }

        public SgEnumerateDevicesResponse EnumerateDevices(SgEnumerateDevices input) {
            if (input.Device == DeviceType.Camera) {
                var cameraChooserVM = cameraVM.CameraChooserVM;
                cameraChooserVM.GetEquipment();
                var devices = cameraChooserVM.Devices.ToList();
                return new SgEnumerateDevicesResponse() {
                    Success = true,
                    Devices = devices.Select(d => d.Name).ToArray()
                };
            } else {
                return new SgEnumerateDevicesResponse() {
                    Success = false,
                    Message = "Only Camera implemented at this time"
                };
            }
        }

        public SgGetCameraPropsResponse GetCameraProps() {
            var cameraInfo = cameraVM.GetDeviceInfo();
            if (cameraInfo.Connected != true) {
                return new SgGetCameraPropsResponse() {
                    Success = false,
                    Message = "No camera connected"
                };
            }

            return new SgGetCameraPropsResponse() {
                Success = true,
                GainValues = cameraInfo.Gains.Select(g => g.ToString()).ToArray(),
                IsoValues = cameraInfo.Gains.Select(g => g.ToString()).ToArray(),
                NumPixelsX = cameraInfo.XSize,
                NumPixelsY = cameraInfo.YSize,
                SupportsSubframe = cameraInfo.CanSubSample,
                CanSetTemperature = cameraInfo.CanSetTemperature
            };
        }

        public SgGetDeviceStatusResponse GetDeviceStatus(SgGetDeviceStatus input) {
            if (input.Device == DeviceType.Camera) {
                var cameraInfo = cameraVM.GetDeviceInfo();
                var state = StateType.IDLE;
                if (cameraInfo.Connected != true) {
                    state = StateType.DISCONNECTED;
                } else if (cameraVM.GetDeviceInfo().IsExposing) {
                    state = StateType.BUSY;
                }
                return new SgGetDeviceStatusResponse() {
                    Success = true,
                    State = state
                };
            } else {
                return new SgGetDeviceStatusResponse() {
                    Success = false,
                    Message = "Only Camera implemented at this time"
                };
            }
        }

        public SgGetImagePathResponse GetImagePath(SgGetImagePath input) {
            var imageCaptureReceipt = (ImageCaptureReceipt?)imageCaptureReceipts.Get(input.Receipt.ToString());
            if (!imageCaptureReceipt.HasValue) {
                return new SgGetImagePathResponse() {
                    Success = false,
                    Message = "Receipt does not exist"
                };
            }

            if (!imageCaptureReceipt.Value.SavePathTask.IsCompleted) {
                return new SgGetImagePathResponse() {
                    Success = false,
                    Message = "Image capture and save are not completed yet"
                };
            }

            // SGP's convention here is a bit weird. For this method, Message is the output file path on success
            return new SgGetImagePathResponse() {
                Success = true,
                Message = imageCaptureReceipt.Value.SavePathTask.Result
            };
        }

        public SgSetCameraCoolerEnabledResponse SetCameraCoolerEnabled(SgSetCameraCoolerEnabled input) {
            var cameraInfo = cameraVM.GetDeviceInfo();
            if (cameraInfo?.Connected != true) {
                return new SgSetCameraCoolerEnabledResponse() {
                    Success = false,
                    Message = "No camera connected"
                };
            }

            try {
                cameraVM.SetCooler(input.Enabled);
                return new SgSetCameraCoolerEnabledResponse() {
                    Success = true
                };
            } catch (Exception ex) {
                return new SgSetCameraCoolerEnabledResponse() {
                    Success = false,
                    Message = ex.ToString()
                };
            }
        }

        public SgCameraCoolerResponse GetCameraCooler() {
            var cameraInfo = cameraVM.GetDeviceInfo();
            if (cameraInfo.Connected != true) {
                return new SgCameraCoolerResponse() {
                    Success = false,
                    Message = "No camera connected"
                };
            }

            try {
                return new SgCameraCoolerResponse() {
                    Success = true,
                    Enabled = cameraInfo.CoolerOn
                };
            } catch (Exception ex) {
                return new SgCameraCoolerResponse() {
                    Success = false,
                    Message = ex.ToString()
                };
            }
        }

        public SgGetCameraTempResponse GetCameraTemp() {
            var cameraInfo = cameraVM.GetDeviceInfo();
            if (cameraInfo.Connected != true) {
                return new SgGetCameraTempResponse() {
                    Success = false,
                    Message = "No camera connected"
                };
            }

            try {
                return new SgGetCameraTempResponse() {
                    Success = true,
                    Temperature = cameraInfo.Temperature
                };
            } catch (Exception ex) {
                return new SgGetCameraTempResponse() {
                    Success = false,
                    Message = ex.ToString()
                };
            }
        }

        public SgSetCameraTempResponse SetCameraTemp(SgSetCameraTemp input) {
            var cameraInfo = cameraVM.GetDeviceInfo();
            if (cameraInfo.Connected != true) {
                return new SgSetCameraTempResponse() {
                    Success = false,
                    Message = "No camera connected"
                };
            }

            try {
                cameraVM.SetTemperature(input.Temperature);
                return new SgSetCameraTempResponse() {
                    Success = true
                };
            } catch (Exception ex) {
                return new SgSetCameraTempResponse() {
                    Success = false,
                    Message = ex.ToString()
                };
            }
        }
    }
}