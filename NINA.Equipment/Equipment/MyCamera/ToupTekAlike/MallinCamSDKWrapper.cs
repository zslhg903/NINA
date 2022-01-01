﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using MallinCam;
using NINA.Equipment.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NINA.Equipment.Equipment.MyCamera.ToupTekAlike {

    public static class MallinCamEnumExtensions {

        public static MallinCam.MallinCam.eOPTION ToMallinCam(this ToupTekAlikeOption option) {
            return (MallinCam.MallinCam.eOPTION)Enum.Parse(typeof(ToupTekAlikeOption), option.ToString());
        }

        public static ToupTekAlikeEvent ToEvent(this MallinCam.MallinCam.eEVENT info) {
            return (ToupTekAlikeEvent)Enum.Parse(typeof(MallinCam.MallinCam.eEVENT), info.ToString());
        }

        public static ToupTekAlikeFrameInfo ToFrameInfo(this MallinCam.MallinCam.FrameInfoV2 info) {
            var ttInfo = new ToupTekAlikeFrameInfo();
            ttInfo.flag = info.flag;
            ttInfo.height = info.height;
            ttInfo.width = info.width;
            ttInfo.timestamp = info.timestamp;
            ttInfo.seq = info.seq;
            return ttInfo;
        }

        public static ToupTekAlikeDeviceInfo ToDeviceInfo(this MallinCam.MallinCam.DeviceV2 info) {
            var ttInfo = new ToupTekAlikeDeviceInfo();
            ttInfo.displayname = info.displayname;
            ttInfo.id = info.id;
            ttInfo.model = info.model.ToModel();

            return ttInfo;
        }

        public static ToupTekAlikeModel ToModel(this MallinCam.MallinCam.ModelV2 modelV2) {
            var ttModel = new ToupTekAlikeModel();
            ttModel.flag = modelV2.flag;
            ttModel.ioctrol = modelV2.ioctrol;
            ttModel.maxfanspeed = modelV2.maxfanspeed;
            ttModel.maxspeed = modelV2.maxspeed;
            ttModel.name = modelV2.name;
            ttModel.preview = modelV2.preview;
            ttModel.still = modelV2.still;
            ttModel.xpixsz = modelV2.xpixsz;
            ttModel.ypixsz = modelV2.ypixsz;
            ttModel.res = new ToupTekAlikeResolution[modelV2.res.Length];
            for (var i = 0; i < modelV2.res.Length; i++) {
                ttModel.res[i] = new ToupTekAlikeResolution() { height = modelV2.res[i].height, width = modelV2.res[i].width };
            }
            return ttModel;
        }
    }

    public class MallinCamSDKWrapper : IToupTekAlikeCameraSDK {
        private MallinCam.MallinCam sdk;

        public string Category => "MallinCam";

        public IToupTekAlikeCameraSDK Open(string id) {
            this.sdk = MallinCam.MallinCam.Open(id);
            return this;
        }

        public uint MaxSpeed => sdk.MaxSpeed;

        public bool MonoMode => sdk.MonoMode;

        public void Close() {
            sdk.Close();
            sdk = null;
        }

        public bool get_ExpoAGain(out ushort gain) {
            return sdk.get_ExpoAGain(out gain);
        }

        public void get_ExpoAGainRange(out ushort min, out ushort max, out ushort def) {
            sdk.get_ExpoAGainRange(out min, out max, out def);
        }

        public void get_ExpTimeRange(out uint min, out uint max, out uint def) {
            sdk.get_ExpTimeRange(out min, out max, out def);
        }

        public void get_Option(ToupTekAlikeOption option, out int target) {
            sdk.get_Option(option.ToMallinCam(), out target);
        }

        public bool get_RawFormat(out uint fourCC, out uint bitDepth) {
            return sdk.get_RawFormat(out fourCC, out bitDepth);
        }

        public void get_Size(out int width, out int height) {
            sdk.get_Size(out width, out height);
        }

        public void get_Speed(out ushort speed) {
            sdk.get_Speed(out speed);
        }

        public void get_Temperature(out short temp) {
            sdk.get_Temperature(out temp);
        }

        public bool PullImageV2(ushort[] data, int bitDepth, out ToupTekAlikeFrameInfo info) {
            MallinCam.MallinCam.FrameInfoV2 ToupcampInfo;
            var result = sdk.PullImageV2(data, bitDepth, out ToupcampInfo);
            info = ToupcampInfo.ToFrameInfo();
            return result;
        }

        public bool put_ROI(uint x, uint y, uint width, uint height) {
            return sdk.put_Roi(x, y, width, height);
        }

        public bool put_AutoExpoEnable(bool v) {
            return sdk.put_AutoExpoEnable(v);
        }

        public bool put_ExpoAGain(ushort value) {
            return sdk.put_ExpoAGain(value);
        }

        public bool put_ExpoTime(uint µsTime) {
            return sdk.put_ExpoTime(µsTime);
        }

        public bool put_Option(ToupTekAlikeOption option, int v) {
            return sdk.put_Option(option.ToMallinCam(), v);
        }

        public bool put_Speed(ushort value) {
            return sdk.put_Speed(value);
        }

        private ToupTekAlikeCallback toupTekAlikeCallback;

        public bool StartPullModeWithCallback(ToupTekAlikeCallback toupTekAlikeCallback) {
            this.toupTekAlikeCallback = toupTekAlikeCallback;
            var delegateCb = new MallinCam.MallinCam.DelegateEventCallback(EventCallback);

            return sdk.StartPullModeWithCallback(delegateCb);
        }

        private void EventCallback(MallinCam.MallinCam.eEVENT nEvent) {
            toupTekAlikeCallback(nEvent.ToEvent());
        }

        public bool Trigger(ushort v) {
            return sdk.Trigger(v);
        }

        public string Version() {
            return MallinCam.MallinCam.Version();
        }
    }
}