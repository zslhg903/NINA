﻿using NINA.Model.MyTelescope;
using NINA.Profile;
using NINA.Utility.Astrometry;
using System;

namespace NINA.Utility {

    public static class MeridianFlip {

        public static TimeSpan TimeToMeridian(Coordinates coordinates, Angle localSiderealTime) {
            coordinates = coordinates.Transform(Epoch.JNOW);
            var rightAscension = Angle.ByHours(coordinates.RA);
            var hoursToMeridian = (rightAscension.Hours - localSiderealTime.Hours) % 12.0;
            if (hoursToMeridian < 0.0) {
                hoursToMeridian += 12.0;
            }
            return TimeSpan.FromHours(hoursToMeridian);
        }

        public static PierSide ExpectedPierSide(Coordinates coordinates, Angle localSiderealTime) {
            coordinates = coordinates.Transform(Epoch.JNOW);
            var rightAscension = Angle.ByHours(coordinates.RA);
            var hoursToLST = (rightAscension.Hours - localSiderealTime.Hours) % 24.0;
            if (hoursToLST < 0.0) {
                hoursToLST += 24.0;
            }

            if (hoursToLST < 12.0) {
                return PierSide.pierWest;
            } else {
                return PierSide.pierEast;
            }
        }

        public static TimeSpan TimeToMeridianFlip(
            IMeridianFlipSettings settings,
            Coordinates coordinates,
            Angle localSiderealTime,
            PierSide currentSideOfPier) {
            // Shift the coordiantes by the time after the meridian to retrieve the time to the flip instead of the time to the meridian
            // This is critical to do instead of just adding to the meridian time, when the scope is already past the meridian but not past the flip
            var flipCoordinatesProjected = new Coordinates(Angle.ByHours(coordinates.RA + settings.MaxMinutesAfterMeridian / 60d), Angle.ByDegree(coordinates.Dec), coordinates.Epoch);
            var timeToMeridianFlip = TimeToMeridian(flipCoordinatesProjected, localSiderealTime: localSiderealTime);

            if (settings.UseSideOfPier) {
                if (currentSideOfPier == PierSide.pierUnknown) {
                    throw new ArgumentException("UseSideOfPier is enabled but pier side is Unknown");
                }
                var timeToMeridian = TimeToMeridian(coordinates, localSiderealTime: localSiderealTime);

                var expectedPierSide = ExpectedPierSide(coordinates: coordinates, localSiderealTime: localSiderealTime);
                if (timeToMeridian < TimeSpan.FromHours(1) && expectedPierSide != currentSideOfPier) {
                    // The telescope did not yet traverse the meridian, but is close to it
                    // However the current side of pier is not what the expected pier side should be,
                    // which means the scope is already in the flipped state and won't require a flip.
                    // Thus, the next meridian flip won't be for another 12 hours
                    timeToMeridianFlip += TimeSpan.FromHours(12.0);
                }
                if (timeToMeridianFlip < TimeSpan.FromHours(1) && timeToMeridian > TimeSpan.FromHours(11) && expectedPierSide == currentSideOfPier) {
                    // The telescope did travers the meridian recently, but the flip is soon
                    // The side of pier is already what it should be
                    // Thus, the next meridian flip won't be for another 12 hours
                    timeToMeridianFlip += TimeSpan.FromHours(12.0);
                }
            }

            // Safeguard against unrealistic timespan
            if (timeToMeridianFlip >= TimeSpan.FromDays(1)) {
                timeToMeridianFlip -= TimeSpan.FromDays(1);
            }
            return timeToMeridianFlip;
        }
    }
}