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
using System.Threading.Tasks;
using System.Collections.Generic;
using NINA.Core.Enum;
using System.Threading;

namespace NINA.Equipment.Interfaces {

    public enum TrackingMode {
        Sidereal,
        Lunar,
        Solar,
        King,
        Custom,
        Stopped
    }

    public struct TrackingRate {
        public static TrackingRate STOPPED = new TrackingRate() { TrackingMode = TrackingMode.Stopped };
        public TrackingMode TrackingMode;
        public double? CustomRightAscensionRate;
        public double? CustomDeclinationRate;
    }

    public interface ITelescope : IDevice {
        Coordinates Coordinates { get; }
        double RightAscension { get; }
        string RightAscensionString { get; }
        double Declination { get; }
        string DeclinationString { get; }
        double SiderealTime { get; }
        string SiderealTimeString { get; }
        double Altitude { get; }
        string AltitudeString { get; }
        double Azimuth { get; }
        string AzimuthString { get; }
        double HoursToMeridian { get; }
        string HoursToMeridianString { get; }
        double TimeToMeridianFlip { get; }
        string TimeToMeridianFlipString { get; }
        double PrimaryMovingRate { get; set; }
        double SecondaryMovingRate { get; set; }
        PierSide SideOfPier { get; }
        bool CanSetTrackingEnabled { get; }
        bool TrackingEnabled { get; set; }
        IList<TrackingMode> TrackingModes { get; }
        TrackingRate TrackingRate { get; }
        TrackingMode TrackingMode { get; set; }
        double SiteLatitude { get; set; }
        double SiteLongitude { get; set; }
        double SiteElevation { get; set; }
        bool AtHome { get; }
        bool CanFindHome { get; }
        bool AtPark { get; }
        bool CanPark { get; }
        bool CanUnpark { get; }
        bool CanSetPark { get; }
        Epoch EquatorialSystem { get; }
        bool HasUnknownEpoch { get; }
        Coordinates TargetCoordinates { get; }
        PierSide? TargetSideOfPier { get; }
        bool Slewing { get; }
        double GuideRateRightAscensionArcsecPerSec { get; }
        double GuideRateDeclinationArcsecPerSec { get; }
        bool CanMovePrimaryAxis { get; }
        bool CanMoveSecondaryAxis { get; }
        bool CanSetDeclinationRate { get; }
        bool CanSetRightAscensionRate { get; }

        IList<(double, double)> GetAxisRates(TelescopeAxes axis);

        Task<bool> MeridianFlip(Coordinates targetCoordinates, CancellationToken token);

        void MoveAxis(TelescopeAxes axis, double rate);

        void PulseGuide(GuideDirections direction, int duration);

        void Park();

        void Setpark();

        Task<bool> SlewToCoordinates(Coordinates coordinates, CancellationToken token);

        void StopSlew();

        bool Sync(Coordinates coordinates);

        void Unpark();

        void SetCustomTrackingRate(double rightAscensionRate, double declinationRate);

        void FindHome();
    }
}