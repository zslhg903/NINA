﻿#region "copyright"
/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors 

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using NINA.Core.Enum;
using NINA.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Equipment.SDK.CameraSDKs.PlayerOneSDK {
    public class PlayerOneSDK : IPlayerOneSDK {
        private IPlayerOnePInvokeProxy playerOnePInvoke;
        private Dictionary<POAConfig, PlayerOneControl> controls;
        private List<POAImgFormat> formats;
        private int bitDepth;
        private int id;
        
        public PlayerOneSDK (int id, IPlayerOnePInvokeProxy playerOnePInvoke) {
            this.id = id;
            this.playerOnePInvoke = playerOnePInvoke;
            this.bitDepth = 16;
            this.formats = new List<POAImgFormat>();
            this.controls = new Dictionary<POAConfig, PlayerOneControl>();
        }
        public bool Connected { get; private set; }

        public void Connect() {
            controls.Clear();

            CheckAndThrowError(playerOnePInvoke.POAOpenCamera(id));
            CheckAndThrowError(playerOnePInvoke.POAInitCamera(id));

            playerOnePInvoke.POASetTrgModeEnable(id, POABool.POA_TRUE);

            CheckAndThrowError(playerOnePInvoke.POAGetCameraProperties(id, out var properties));

            formats.Clear();
            if (properties.imgFormats != null) {
                for (int i = 0; i < properties.imgFormats.Length; i++) {
                    var format = properties.imgFormats[i];
                    if (format == POAImgFormat.POA_END) { break; }
                    formats.Add(format);
                }
            }

            SetHighestBitDepth();

            playerOnePInvoke.POAGetConfigsCount(id, out var numOfControls);

            for (int i = 0; i < numOfControls; i++) {
                playerOnePInvoke.POAGetConfigAttributes(id, i, out var caps);
                if (!controls.ContainsKey(caps.configID)) {
                    controls.Add(caps.configID, new PlayerOneControl(i, caps));

                    if(caps.valueType == POAValueType.VAL_BOOL) {
                        Logger.Trace($"Found control {caps.szDescription} - default: {caps.defaultValue.boolValue}, min: {caps.minValue.boolValue}, max: {caps.maxValue.boolValue}");
                    } else if(caps.valueType == POAValueType.VAL_FLOAT) {
                        Logger.Trace($"Found control {caps.szDescription} - default: {caps.defaultValue.floatValue}, min: {caps.minValue.floatValue}, max: {caps.maxValue.floatValue}");
                    } else {
                        Logger.Trace($"Found control {caps.szDescription} - default: {caps.defaultValue.intValue}, min: {caps.minValue.intValue}, max: {caps.maxValue.intValue}");
                    }
                    
                    // Set all to default
                    if(caps.isWritable == POABool.POA_TRUE) {
                        SetControlValue(caps.configID, caps.defaultValue);
                    }
                    
                }
            }

            SetROI(0, 0, properties.maxWidth, properties.maxHeight, 1);            

            Connected = true;
        }

        public int GetBitDepth() {
            return bitDepth;
        }

        private void SetHighestBitDepth() {
            if (formats.Contains(POAImgFormat.POA_RAW16)) {
                CheckAndLogError(playerOnePInvoke.POASetImageFormat(id, POAImgFormat.POA_RAW16));
                bitDepth = 16;
                return;
            } else if (formats.Contains(POAImgFormat.POA_RAW8)) {
                CheckAndLogError(playerOnePInvoke.POASetImageFormat(id, POAImgFormat.POA_RAW8));
                bitDepth = 8;
            } else {
                throw new NotSupportedException();
            }
        }

        public bool SetROI(int startX, int startY, int width, int height, int binning) {
            var (oldX, oldY, oldWidth, oldHeight, oldBin) = GetROI();

            if (oldX != startX || oldY != startY || oldWidth != width || oldHeight != height || oldBin != binning) {                
                startX = startX / binning;
                startY = startY / binning;
                width = width / binning;
                height = height / binning;
                width = width - width % 8;
                height = height - height % 2;
                Logger.Debug($"Setting ROI to {startX}x{startY}:{width}x{height} with binning {binning}");
                var result = CheckAndLogError(playerOnePInvoke.POASetImageBin(id, binning));
                result = result && CheckAndLogError(playerOnePInvoke.POASetImageStartPos(id, startX, startY));
                result = result && CheckAndLogError(playerOnePInvoke.POASetImageSize(id, width, height));
                
                return result;
            } else {
                return true;
            }
        }

        public (int, int, int, int, int) GetROI() {
            CheckAndLogError(playerOnePInvoke.POAGetImageBin(id, out var binning));
            CheckAndLogError(playerOnePInvoke.POAGetImageStartPos(id, out var startX, out var startY));
            CheckAndLogError(playerOnePInvoke.POAGetImageSize(id, out var width, out var height));
            return (startX, startY, width, height, binning);
        }

        public void StartExposure(double exposureTime, int width, int height) {
            var transformedExposureTime = (int)(exposureTime * 1000000d);
            SetControlValue(POAConfig.POA_EXPOSURE, transformedExposureTime);

            CheckAndThrowError(playerOnePInvoke.POAStartExposure(id, POABool.POA_TRUE));                  
        }

        public bool IsExposureReady() {
            CheckAndThrowError(playerOnePInvoke.POAImageReady(id, out var ready));

            return ready == POABool.POA_TRUE;
        }


        [HandleProcessCorruptedStateExceptions]
        public async Task<ushort[]> GetExposure(double exposureTime, int width, int height, CancellationToken ct) {
            var transformedExposureTime = (int)(exposureTime * 1000000d);

            int size = width * height;
            ushort[] buffer = new ushort[size];
            int buffersize = width * height * 2;

            try {
                CheckAndThrowError(playerOnePInvoke.POAImageReady(id, out var ready));
                while (!(ready ==  POABool.POA_TRUE)) {
                    if (!Connected) {
                        break;
                    }

                    if (ct.IsCancellationRequested) {
                        break;
                    }
                    await CoreUtil.Wait(TimeSpan.FromMilliseconds(10), ct);
                    CheckAndThrowError(playerOnePInvoke.POAImageReady(id, out ready));

                }
                if (ready == POABool.POA_TRUE) {
                    if (playerOnePInvoke.POAGetImageData(id, buffer, buffersize, (int)(exposureTime * 2 * 100) + 500) == POAErrors.POA_OK) {
                        return buffer;
                    }
                }
            } catch (AccessViolationException ex) {
                Logger.Error($"{nameof(SVBonySDK)} - Access Violation Exception occurred during frame download!", ex);
            } catch (Exception ex) {
                Logger.Error($"{nameof(SVBonySDK)} - Unexpected exception occurred during frame download!", ex);
            }

            return null;
        }

        public int[] GetBinningInfo() {
            playerOnePInvoke.POAGetCameraProperties(id, out var property);
            List<int> binnings = new List<int>();
            for (int i = 0; i < property.bins.Length; i++) {
                var bin = property.bins[i];
                if (bin == 0) { break; }
                binnings.Add(bin);
            }

            return binnings.ToArray();
        }

        public void Disconnect() {            
            CheckAndLogError(playerOnePInvoke.POACloseCamera(id));

            controls.Clear();
            Connected = false;
        }

        public bool SetGain(int value) {
            Logger.Trace($"Setting gain to {value}");
            return SetControlValue(POAConfig.POA_GAIN, value);
        }

        public int GetGain() {
            return GetControlValue(POAConfig.POA_GAIN).intValue;
        }

        public int GetMinGain() {
            return GetMinControlValue(POAConfig.POA_GAIN).intValue;
        }

        public int GetMaxGain() {
            return GetMaxControlValue(POAConfig.POA_GAIN).intValue;
        }

        public bool SetOffset(int value) {
            Logger.Trace($"Setting offset to {value}");
            return SetControlValue(POAConfig.POA_OFFSET, value);
        }

        public int GetOffset() {
            return GetControlValue(POAConfig.POA_OFFSET).intValue;
        }

        public int GetMinOffset() {
            return GetMinControlValue(POAConfig.POA_OFFSET).intValue;
        }

        public int GetMaxOffset() {
            return GetMaxControlValue(POAConfig.POA_OFFSET).intValue;
        }

        public bool SetUSBLimit(int value) {
            return SetControlValue(POAConfig.POA_FRAME_LIMIT, value);
        }

        public int GetUSBLimit() {
            return GetControlValue(POAConfig.POA_USB_BANDWIDTH_LIMIT).intValue;
        }

        public int GetMinUSBLimit() {
            return GetMinControlValue(POAConfig.POA_USB_BANDWIDTH_LIMIT).intValue;
        }

        public int GetMaxUSBLimit() {
            return GetMaxControlValue(POAConfig.POA_USB_BANDWIDTH_LIMIT).intValue;
        }

        public double GetPixelSize() {
            playerOnePInvoke.POAGetCameraProperties(id, out var property);
            return Math.Round(property.pixelSize,2);
        }

        public double GetMinExposureTime() {
            return GetMinControlValue(POAConfig.POA_EXPOSURE).intValue / 1000000d;
        }

        public double GetMaxExposureTime() {
            return GetMaxControlValue(POAConfig.POA_EXPOSURE).intValue / 1000000d;
        }

        public SensorType GetSensorInfo() {
            playerOnePInvoke.POAGetCameraProperties(id, out var property);
            if (property.isColorCamera == POABool.POA_TRUE) {
                switch (property.bayerPattern) {
                    case POABayerPattern.POA_BAYER_GB:
                        return SensorType.GBRG;

                    case POABayerPattern.POA_BAYER_GR:
                        return SensorType.GRBG;

                    case POABayerPattern.POA_BAYER_BG:
                        return SensorType.BGGR;

                    case POABayerPattern.POA_BAYER_RG:
                        return SensorType.RGGB;

                    default:
                        return SensorType.BGGR;
                };
            }

            return SensorType.Monochrome;
        }

        public (int, int) GetDimensions() {
            playerOnePInvoke.POAGetCameraProperties(id, out var property);
            return ((int)property.maxWidth, (int)property.maxHeight);
        }

        public bool HasTemperatureControl() {
            playerOnePInvoke.POAGetCameraProperties(id, out var property);
            return (property.isHasCooler == POABool.POA_TRUE);
        }

        public bool SetTargetTemperature(double temperature) {
            var convertedTemp = temperature * 10;
            var nearest = (int)Math.Round(convertedTemp);

            var minTemperatureSetpoint = GetMinControlValue(POAConfig.POA_TARGET_TEMP).intValue;
            var maxTemperatureSetpoint = GetMaxControlValue(POAConfig.POA_TARGET_TEMP).intValue;

            if (nearest > maxTemperatureSetpoint) {
                nearest = maxTemperatureSetpoint;
            } else if (nearest < minTemperatureSetpoint) {
                nearest = minTemperatureSetpoint;
            }
            return SetControlValue(POAConfig.POA_TARGET_TEMP, nearest);
        }

        public double GetTargetTemperature() {
            return GetControlValue(POAConfig.POA_TARGET_TEMP).intValue / 10d;
        }

        public double GetTemperature() {
            return GetControlValue(POAConfig.POA_TEMPERATURE).intValue / 10d;
        }

        public bool SetCooler(bool onOff) {
            return SetControlValue(POAConfig.POA_COOLER, onOff ? 1 : 0);
        }

        public bool GetCoolerOnOff() {
            return GetControlValue(POAConfig.POA_COOLER).boolValue == POABool.POA_TRUE ? true : false;
        }

        public double GetCoolerPower() {
            return GetControlValue(POAConfig.POA_COOLER_POWER).floatValue;
        }

        private bool SetControlValue(POAConfig type, bool value) {
            CheckAndLogError(playerOnePInvoke.POAGetConfigValueType(type, out var t));
            if(t != POAValueType.VAL_BOOL) { throw new ArgumentException(); }
            return SetControlValue(type, new POAConfigValue() { boolValue = value ? POABool.POA_TRUE : POABool.POA_FALSE });
        }
        private bool SetControlValue(POAConfig type, float value) {
            CheckAndLogError(playerOnePInvoke.POAGetConfigValueType(type, out var t));
            if (t != POAValueType.VAL_FLOAT) { throw new ArgumentException(); }
            return SetControlValue(type, new POAConfigValue() { floatValue = value });
        }
        private bool SetControlValue(POAConfig type, int value) {
            CheckAndLogError(playerOnePInvoke.POAGetConfigValueType(type, out var t));
            if (t != POAValueType.VAL_INT) { throw new ArgumentException(); }
            return SetControlValue(type, new POAConfigValue() { intValue = value  });
        }

        private bool SetControlValue(POAConfig type, POAConfigValue value) {
            if (controls.TryGetValue(type, out var control)) {
                CheckAndLogError(playerOnePInvoke.POAGetConfigAttributesByConfigID(id, type, out var attributes));
                if(!(attributes.isWritable == POABool.POA_TRUE)) { return false; }

                switch(control.Type) {
                    case POAValueType.VAL_BOOL: {
                            if (value.boolValue < control.Min.boolValue) { value.boolValue = control.Min.boolValue; }
                            if (value.boolValue > control.Max.boolValue) { value.boolValue = control.Max.boolValue; }

                            var oldValue = GetControlValue(type);
                            if (oldValue.boolValue != value.boolValue) {
                                Logger.Trace($"Setting control {attributes.szConfName} to {value}");

                                return CheckAndLogError(playerOnePInvoke.POASetConfig(id, type, value, POABool.POA_FALSE));
                            }
                            return true;
                        }
                    case POAValueType.VAL_FLOAT: {
                            if (value.floatValue < control.Min.floatValue) { value.floatValue = control.Min.floatValue; }
                            if (value.floatValue > control.Max.floatValue) { value.floatValue = control.Max.floatValue; }

                            var oldValue = GetControlValue(type);
                            if (oldValue.floatValue != value.floatValue) {
                                Logger.Trace($"Setting control {attributes.szConfName} to {value}");

                                return CheckAndLogError(playerOnePInvoke.POASetConfig(id, type, value, POABool.POA_FALSE));
                            }
                            return true;
                        }
                    case POAValueType.VAL_INT: {
                            if (value.intValue < control.Min.intValue) { value.intValue = control.Min.intValue; }
                            if (value.intValue > control.Max.intValue) { value.intValue = control.Max.intValue; }

                            var oldValue = GetControlValue(type);
                            if (oldValue.intValue != value.intValue) {
                                Logger.Trace($"Setting control {attributes.szConfName} to {value}");

                                return CheckAndLogError(playerOnePInvoke.POASetConfig(id, type, value, POABool.POA_FALSE));
                            }
                            return true;
                        }
                }

                
            }
            return false;
        }



        private POAConfigValue GetMinControlValue(POAConfig type) {
            if (controls.TryGetValue(type, out var control)) {
                return control.Min;
            }
            return default;
        }

        private POAConfigValue GetMaxControlValue(POAConfig type) {
            if (controls.TryGetValue(type, out var control)) {
                return control.Max;
            }
            return default;
        }

        private POAConfigValue GetControlValue(POAConfig type) {
            if (controls.TryGetValue(type, out var control)) {
                if (CheckAndLogError(playerOnePInvoke.POAGetConfig(id, type, out var value, out var auto))) {
                    return value;
                }
            }
            return default;
        }

        private bool CheckAndLogError(POAErrors code) {
            if (code == POAErrors.POA_OK) { return true; }

            Logger.Error(code.ToString());
            return false;
        }

        private void CheckAndThrowError(POAErrors code) {
            if (code == POAErrors.POA_OK) { return; }

            throw new Exception(code.ToString());
        }
    }
}
