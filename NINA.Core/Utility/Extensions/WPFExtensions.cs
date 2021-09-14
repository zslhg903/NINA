﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NINA.Core.Utility.Extensions {

    internal static class OSInterop {

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int smIndex);

        public const int SM_CMONITORS = 80;

        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref RECT rc, int nUpdate);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);

        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public int width { get { return right - left; } }
            public int height { get { return bottom - top; } }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public class MONITORINFOEX {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice = new char[32];

            public int dwFlags;
        }
    }

    internal static class WPFExtensionMethods {

        public static Rect GetAbsolutePosition(this Window w) {
            if (w.WindowState != WindowState.Maximized)
                return new Rect(w.Left, w.Top, w.Width, w.Height);

            Rect r;
            bool multimonSupported = OSInterop.GetSystemMetrics(OSInterop.SM_CMONITORS) != 0;
            if (!multimonSupported) {
                OSInterop.RECT rc = new OSInterop.RECT();
                OSInterop.SystemParametersInfo(48, 0, ref rc, 0);
                r = new Rect(rc.left, rc.top, rc.width, rc.height);
            } else {
                WindowInteropHelper helper = new WindowInteropHelper(w);
                IntPtr hmonitor = OSInterop.MonitorFromWindow(new HandleRef((object)null, helper.EnsureHandle()), 2);
                OSInterop.MONITORINFOEX info = new OSInterop.MONITORINFOEX();
                OSInterop.GetMonitorInfo(new HandleRef((object)null, hmonitor), info);
                r = new Rect(info.rcWork.left, info.rcWork.top, info.rcWork.width, info.rcWork.height);
            }
            return r;
        }
    }
}