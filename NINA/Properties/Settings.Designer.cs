﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NINA.Properties {


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.1.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default {
            get {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ASCOM.Simulator.Camera")]
        public string CameraId {
            get {
                return ((string)(this["CameraId"]));
            }
            set {
                this["CameraId"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ASCOM.Simulator.FilterWheel")]
        public string FilterWheelId {
            get {
                return ((string)(this["FilterWheelId"]));
            }
            set {
                this["FilterWheelId"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ImageFilePath {
            get {
                return ((string)(this["ImageFilePath"]));
            }
            set {
                this["ImageFilePath"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("$$IMAGETYPE$$\\\\$$DATETIME$$_$$FILTER$$_$$SENSORTEMP$$_$$EXPOSURETIME$$s_$$FRAMENR" +
            "$$")]
        public string ImageFilePattern {
            get {
                return ((string)(this["ImageFilePattern"]));
            }
            set {
                this["ImageFilePattern"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("localhost")]
        public string PHD2ServerUrl {
            get {
                return ((string)(this["PHD2ServerUrl"]));
            }
            set {
                this["PHD2ServerUrl"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4400")]
        public int PHD2ServerPort {
            get {
                return ((int)(this["PHD2ServerPort"]));
            }
            set {
                this["PHD2ServerPort"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF000000")]
        public global::System.Windows.Media.Color PrimaryColor {
            get {
                return ((global::System.Windows.Media.Color)(this["PrimaryColor"]));
            }
            set {
                this["PrimaryColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF1D2731")]
        public global::System.Windows.Media.Color SecondaryColor {
            get {
                return ((global::System.Windows.Media.Color)(this["SecondaryColor"]));
            }
            set {
                this["SecondaryColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#AABCBCBC")]
        public global::System.Windows.Media.Color BorderColor {
            get {
                return ((global::System.Windows.Media.Color)(this["BorderColor"]));
            }
            set {
                this["BorderColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFFFFFFF")]
        public global::System.Windows.Media.Color BackgroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["BackgroundColor"]));
            }
            set {
                this["BackgroundColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1.5")]
        public double DitherPixels {
            get {
                return ((double)(this["DitherPixels"]));
            }
            set {
                this["DitherPixels"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DitherRAOnly {
            get {
                return ((bool)(this["DitherRAOnly"]));
            }
            set {
                this["DitherRAOnly"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int LogLevel {
            get {
                return ((int)(this["LogLevel"]));
            }
            set {
                this["LogLevel"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ASCOM.Simulator.Telescope")]
        public string TelescopeId {
            get {
                return ((string)(this["TelescopeId"]));
            }
            set {
                this["TelescopeId"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF0B3C5D")]
        public global::System.Windows.Media.Color ButtonBackgroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ButtonBackgroundColor"]));
            }
            set {
                this["ButtonBackgroundColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF2190DB")]
        public global::System.Windows.Media.Color ButtonBackgroundSelectedColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ButtonBackgroundSelectedColor"]));
            }
            set {
                this["ButtonBackgroundSelectedColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFFFFFFF")]
        public global::System.Windows.Media.Color ButtonForegroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ButtonForegroundColor"]));
            }
            set {
                this["ButtonForegroundColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF1D2731")]
        public global::System.Windows.Media.Color ButtonForegroundDisabledColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ButtonForegroundDisabledColor"]));
            }
            set {
                this["ButtonForegroundDisabledColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF550C18")]
        public global::System.Windows.Media.Color AltPrimaryColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltPrimaryColor"]));
            }
            set {
                this["AltPrimaryColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF1B2A41")]
        public global::System.Windows.Media.Color AltSecondaryColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltSecondaryColor"]));
            }
            set {
                this["AltSecondaryColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF550C18")]
        public global::System.Windows.Media.Color AltBorderColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltBorderColor"]));
            }
            set {
                this["AltBorderColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF02010A")]
        public global::System.Windows.Media.Color AltBackgroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltBackgroundColor"]));
            }
            set {
                this["AltBackgroundColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF550C18")]
        public global::System.Windows.Media.Color AltButtonBackgroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltButtonBackgroundColor"]));
            }
            set {
                this["AltButtonBackgroundColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF96031A")]
        public global::System.Windows.Media.Color AltButtonBackgroundSelectedColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltButtonBackgroundSelectedColor"]));
            }
            set {
                this["AltButtonBackgroundSelectedColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF02010A")]
        public global::System.Windows.Media.Color AltButtonForegroundColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltButtonForegroundColor"]));
            }
            set {
                this["AltButtonForegroundColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF443730")]
        public global::System.Windows.Media.Color AltButtonForegroundDisabledColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltButtonForegroundDisabledColor"]));
            }
            set {
                this["AltButtonForegroundDisabledColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int FileType {
            get {
                return ((int)(this["FileType"]));
            }
            set {
                this["FileType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AstrometryAPIKey {
            get {
                return ((string)(this["AstrometryAPIKey"]));
            }
            set {
                this["AstrometryAPIKey"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int PlateSolverType {
            get {
                return ((int)(this["PlateSolverType"]));
            }
            set {
                this["PlateSolverType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("750")]
        public int AnsvrFocalLength {
            get {
                return ((int)(this["AnsvrFocalLength"]));
            }
            set {
                this["AnsvrFocalLength"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3.8")]
        public double AnsvrPixelSize {
            get {
                return ((double)(this["AnsvrPixelSize"]));
            }
            set {
                this["AnsvrPixelSize"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%localappdata%\\NINA\\cygwin")]
        public string CygwinLocation {
            get {
                return ((string)(this["CygwinLocation"]));
            }
            set {
                this["CygwinLocation"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int EpochType {
            get {
                return ((int)(this["EpochType"]));
            }
            set {
                this["EpochType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int HemisphereType {
            get {
                return ((int)(this["HemisphereType"]));
            }
            set {
                this["HemisphereType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public double AnsvrSearchRadius {
            get {
                return ((double)(this["AnsvrSearchRadius"]));
            }
            set {
                this["AnsvrSearchRadius"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoMeridianFlip {
            get {
                return ((bool)(this["AutoMeridianFlip"]));
            }
            set {
                this["AutoMeridianFlip"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public double MinutesAfterMeridian {
            get {
                return ((double)(this["MinutesAfterMeridian"]));
            }
            set {
                this["MinutesAfterMeridian"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RecenterAfterFlip {
            get {
                return ((bool)(this["RecenterAfterFlip"]));
            }
            set {
                this["RecenterAfterFlip"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int WeatherDataType {
            get {
                return ((int)(this["WeatherDataType"]));
            }
            set {
                this["WeatherDataType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string OpenWeatherMapAPIKey {
            get {
                return ((string)(this["OpenWeatherMapAPIKey"]));
            }
            set {
                this["OpenWeatherMapAPIKey"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://api.openweathermap.org/data/2.5/weather")]
        public string OpenWeatherMapUrl {
            get {
                return ((string)(this["OpenWeatherMapUrl"]));
            }
            set {
                this["OpenWeatherMapUrl"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string OpenWeatherMapLocation {
            get {
                return ((string)(this["OpenWeatherMapLocation"]));
            }
            set {
                this["OpenWeatherMapLocation"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Light")]
        public string ColorSchemaType {
            get {
                return ((string)(this["ColorSchemaType"]));
            }
            set {
                this["ColorSchemaType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Dark")]
        public string AlternativeColorSchemaType {
            get {
                return ((string)(this["AlternativeColorSchemaType"]));
            }
            set {
                this["AlternativeColorSchemaType"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ASCOM.Simulator.Focuser")]
        public string FocuserId {
            get {
                return ((string)(this["FocuserId"]));
            }
            set {
                this["FocuserId"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("750")]
        public int PS2FocalLength {
            get {
                return ((int)(this["PS2FocalLength"]));
            }
            set {
                this["PS2FocalLength"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3.8")]
        public double PS2PixelSize {
            get {
                return ((double)(this["PS2PixelSize"]));
            }
            set {
                this["PS2PixelSize"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5000")]
        public int PS2Regions {
            get {
                return ((int)(this["PS2Regions"]));
            }
            set {
                this["PS2Regions"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string PS2Location {
            get {
                return ((string)(this["PS2Location"]));
            }
            set {
                this["PS2Location"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int MeridianFlipSettleTime {
            get {
                return ((int)(this["MeridianFlipSettleTime"]));
            }
            set {
                this["MeridianFlipSettleTime"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("en-GB")]
        public global::System.Globalization.CultureInfo Language {
            get {
                return ((global::System.Globalization.CultureInfo)(this["Language"]));
            }
            set {
                this["Language"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public double PauseTimeBeforeMeridian {
            get {
                return ((double)(this["PauseTimeBeforeMeridian"]));
            }
            set {
                this["PauseTimeBeforeMeridian"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%localappdata%\\NINA\\NINA.sqlite")]
        public string DatabaseLocation {
            get {
                return ((string)(this["DatabaseLocation"]));
            }
            set {
                this["DatabaseLocation"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double Latitude {
            get {
                return ((double)(this["Latitude"]));
            }
            set {
                this["Latitude"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double Longitude {
            get {
                return ((double)(this["Longitude"]));
            }
            set {
                this["Longitude"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SkyAtlasImageRepository {
            get {
                return ((string)(this["SkyAtlasImageRepository"]));
            }
            set {
                this["SkyAtlasImageRepository"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFF5A300")]
        public global::System.Windows.Media.Color NotificationWarningColor {
            get {
                return ((global::System.Windows.Media.Color)(this["NotificationWarningColor"]));
            }
            set {
                this["NotificationWarningColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFDB0606")]
        public global::System.Windows.Media.Color NotificationErrorColor {
            get {
                return ((global::System.Windows.Media.Color)(this["NotificationErrorColor"]));
            }
            set {
                this["NotificationErrorColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFF5A300")]
        public global::System.Windows.Media.Color AltNotificationWarningColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltNotificationWarningColor"]));
            }
            set {
                this["AltNotificationWarningColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FFF5A300")]
        public global::System.Windows.Media.Color AltNotificationErrorColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AltNotificationErrorColor"]));
            }
            set {
                this["AltNotificationErrorColor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.2")]
        public double AutoStretchFactor {
            get {
                return ((double)(this["AutoStretchFactor"]));
            }
            set {
                this["AutoStretchFactor"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AnnotateImage {
            get {
                return ((bool)(this["AnnotateImage"]));
            }
            set {
                this["AnnotateImage"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool FocuserUseFilterWheelOffsets {
            get {
                return ((bool)(this["FocuserUseFilterWheelOffsets"]));
            }
            set {
                this["FocuserUseFilterWheelOffsets"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int FocuserAutoFocusStepSize {
            get {
                return ((int)(this["FocuserAutoFocusStepSize"]));
            }
            set {
                this["FocuserAutoFocusStepSize"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4")]
        public int FocuserAutoFocusInitialOffsetSteps {
            get {
                return ((int)(this["FocuserAutoFocusInitialOffsetSteps"]));
            }
            set {
                this["FocuserAutoFocusInitialOffsetSteps"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6")]
        public int FocuserAutoFocusExposureTime {
            get {
                return ((int)(this["FocuserAutoFocusExposureTime"]));
            }
            set {
                this["FocuserAutoFocusExposureTime"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseTelescopeSnapPort {
            get {
                return ((bool)(this["UseTelescopeSnapPort"]));
            }
            set {
                this["UseTelescopeSnapPort"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(":SNAP1,1#")]
        public string TelescopeSnapPortStart {
            get {
                return ((string)(this["TelescopeSnapPortStart"]));
            }
            set {
                this["TelescopeSnapPortStart"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(":SNAP1,0#")]
        public string TelescopeSnapPortStop {
            get {
                return ((string)(this["TelescopeSnapPortStop"]));
            }
            set {
                this["TelescopeSnapPortStop"] = value;
            }
        }
    }
}
