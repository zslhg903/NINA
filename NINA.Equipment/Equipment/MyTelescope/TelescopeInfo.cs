#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Enum;
using NINA.Astrometry;
using System.Collections.Generic;
using NINA.Equipment.Interfaces;

namespace NINA.Equipment.Equipment.MyTelescope {

    public class TelescopeInfo : DeviceInfo {
        private double siderealTime;

        public double SiderealTime {
            get {
                return siderealTime;
            }
            set {
                siderealTime = value;
                RaisePropertyChanged();
            }
        }

        private double rightAscension;

        public double RightAscension {
            get {
                return rightAscension;
            }
            set {
                rightAscension = value;
                RaisePropertyChanged();
            }
        }

        private double declination;

        public double Declination {
            get {
                return declination;
            }
            set {
                declination = value;
                RaisePropertyChanged();
            }
        }

        private double siteLatitude;

        public double SiteLatitude {
            get {
                return siteLatitude;
            }
            set {
                siteLatitude = value;
                RaisePropertyChanged();
            }
        }

        private double siteLongitude;

        public double SiteLongitude {
            get {
                return siteLongitude;
            }
            set {
                siteLongitude = value;
                RaisePropertyChanged();
            }
        }

        private double siteElevation;

        public double SiteElevation {
            get {
                return siteElevation;
            }
            set {
                siteElevation = value;
                RaisePropertyChanged();
            }
        }

        private string rightAscensionString;

        public string RightAscensionString {
            get {
                return rightAscensionString;
            }
            set {
                rightAscensionString = value;
                RaisePropertyChanged();
            }
        }

        private string declinationString;

        public string DeclinationString {
            get {
                return declinationString;
            }
            set {
                declinationString = value;
                RaisePropertyChanged();
            }
        }

        private Coordinates coordinates;

        public Coordinates Coordinates {
            get {
                return coordinates;
            }
            set {
                coordinates = value;
                RaisePropertyChanged();
            }
        }

        private double timeToMeridianFlip;

        public double TimeToMeridianFlip {
            get {
                return timeToMeridianFlip;
            }
            set {
                timeToMeridianFlip = value;
                RaisePropertyChanged();
            }
        }

        private PierSide sideOfPier;

        public PierSide SideOfPier {
            get {
                return sideOfPier;
            }
            set {
                sideOfPier = value;
                RaisePropertyChanged();
            }
        }

        private double altitude = double.NaN;

        public double Altitude {
            get { return altitude; }
            set { altitude = value; RaisePropertyChanged(); }
        }

        private string altitudeString = string.Empty;

        public string AltitudeString {
            get { return altitudeString; }
            set { altitudeString = value; RaisePropertyChanged(); }
        }

        private double azimuth = double.NaN;

        public double Azimuth {
            get { return azimuth; }
            set { azimuth = value; RaisePropertyChanged(); }
        }

        private string azimuthString = string.Empty;

        public string AzimuthString {
            get { return azimuthString; }
            set { azimuthString = value; RaisePropertyChanged(); }
        }

        private string siderealTimeString;

        public string SiderealTimeString {
            get { return siderealTimeString; }
            set { siderealTimeString = value; RaisePropertyChanged(); }
        }

        private string hoursToMeridianString;

        public string HoursToMeridianString {
            get { return hoursToMeridianString; }
            set { hoursToMeridianString = value; RaisePropertyChanged(); }
        }

        private bool atPark;

        public bool AtPark {
            get { return atPark; }
            set { atPark = value; RaisePropertyChanged(); }
        }

        private TrackingRate trackingRate;

        public TrackingRate TrackingRate {
            get { return trackingRate; }
            set { trackingRate = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(TrackingMode)); }
        }

        private bool trackingEnabled;

        public bool TrackingEnabled {
            get { return trackingEnabled; }
            set { trackingEnabled = value; RaisePropertyChanged(); }
        }

        private IList<TrackingMode> trackingModes;

        public IList<TrackingMode> TrackingModes {
            get { return trackingModes; }
            set { trackingModes = value; RaisePropertyChanged(); }
        }

        private bool atHome;

        public bool AtHome {
            get { return atHome; }
            set { atHome = value; RaisePropertyChanged(); }
        }

        private bool canFindHome;

        public bool CanFindHome {
            get { return canFindHome; }
            set { canFindHome = value; RaisePropertyChanged(); }
        }

        private bool canPark;

        public bool CanPark {
            get { return canPark; }
            set { canPark = value; RaisePropertyChanged(); }
        }

        private bool canSetPark;

        public bool CanSetPark {
            get { return canSetPark; }
            set { canSetPark = value; RaisePropertyChanged(); }
        }

        private bool canSetTracking;

        public bool CanSetTrackingEnabled {
            get { return canSetTracking; }
            set { canSetTracking = value; RaisePropertyChanged(); }
        }

        private bool canSetDeclinationRate;

        public bool CanSetDeclinationRate {
            get { return canSetDeclinationRate; }
            set { canSetDeclinationRate = value; RaisePropertyChanged(); }
        }

        private bool canSetRightAscensionRate;

        public bool CanSetRightAscensionRate {
            get { return canSetRightAscensionRate; }
            set { canSetRightAscensionRate = value; RaisePropertyChanged(); }
        }

        private Epoch equatorialSystem;

        public Epoch EquatorialSystem {
            get { return equatorialSystem; }
            set { equatorialSystem = value; RaisePropertyChanged(); }
        }

        private bool hasUnknownEpoch;

        public bool HasUnknownEpoch {
            get { return hasUnknownEpoch; }
            set { hasUnknownEpoch = value; RaisePropertyChanged(); }
        }

        private string timeToMeridianFlipString;

        public string TimeToMeridianFlipString {
            get { return timeToMeridianFlipString; }
            set { timeToMeridianFlipString = value; RaisePropertyChanged(); }
        }

        private Coordinates targetCoordinates;

        public Coordinates TargetCoordinates {
            get { return targetCoordinates; }
            set { targetCoordinates = value; RaisePropertyChanged(); }
        }

        private PierSide? targetSideOfPier;

        public PierSide? TargetSideOfPier {
            get {
                return targetSideOfPier;
            }
            set {
                targetSideOfPier = value;
                RaisePropertyChanged();
            }
        }

        private bool slewing;

        public bool Slewing {
            get { return slewing; }
            set { slewing = value; RaisePropertyChanged(); }
        }

        private double guideRateRightAscensionArcsecPerSec;

        public double GuideRateRightAscensionArcsecPerSec {
            get { return guideRateRightAscensionArcsecPerSec; }
            set { guideRateRightAscensionArcsecPerSec = value; RaisePropertyChanged(); }
        }

        private double guideRateDeclinationArcsecPerSec;

        public double GuideRateDeclinationArcsecPerSec {
            get { return guideRateDeclinationArcsecPerSec; }
            set { guideRateDeclinationArcsecPerSec = value; RaisePropertyChanged(); }
        }

        private bool canMovePrimaryAxis;

        public bool CanMovePrimaryAxis {
            get { return canMovePrimaryAxis; }
            set { canMovePrimaryAxis = value; RaisePropertyChanged(); }
        }

        private bool canMoveSecondaryAxis;

        public bool CanMoveSecondaryAxis {
            get { return canMoveSecondaryAxis; }
            set { canMoveSecondaryAxis = value; RaisePropertyChanged(); }
        }

        private IList<(double, double)> primaryAxisRates;

        public IList<(double, double)> PrimaryAxisRates {
            get { return primaryAxisRates; }
            set { primaryAxisRates = value; RaisePropertyChanged(); }
        }

        private IList<(double, double)> secondaryAxisRates;

        public IList<(double, double)> SecondaryAxisRates {
            get { return secondaryAxisRates; }
            set { secondaryAxisRates = value; RaisePropertyChanged(); }
        }
    }
}