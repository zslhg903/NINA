#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Utility;
using NINA.Utility.Astrometry;
using NINA.Utility.Exceptions;
using NINA.Utility.TcpRaw;
using NINA.Profile;
using System;
using System.Threading.Tasks;

namespace NINA.Model.MyPlanetarium {

    internal class HNSKY : IPlanetarium {
        private string address;
        private int port;

        public HNSKY(IProfileService profileService) {
            this.address = profileService.ActiveProfile.PlanetariumSettings.HNSKYHost;
            this.port = profileService.ActiveProfile.PlanetariumSettings.HNSKYPort;
        }

        public string Name {
            get {
                return "HNSKY";
            }
        }

        /// <summary>
        /// Get the selected object in TheSkyX
        /// </summary>
        /// <returns></returns>
        public async Task<DeepSkyObject> GetTarget() {
            try {
                string command = "GET_TARGET\r\n";

                var query = new BasicQuery(address, port, command);
                string response = await query.SendQuery();

                response = response.TrimEnd('\r', '\n');

                /*
                 * Split the coordinates and object name from the returned message.
                 * GET_TARGET returns 4 fields, space-separated:
                 * RA Dec Name Position_angle
                 *
                 * RA and Dec are in radians. Epoch is J2000.
                 */
                string[] info = response.Split(' ');

                if (!(info[0].Equals("?") || string.IsNullOrEmpty(info[2]))) {
                    Coordinates newCoordinates = new Coordinates(Astrometry.RadianToHour(double.Parse(info[0].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture)),
                                                         Astrometry.ToDegree(double.Parse(info[1].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture)),
                                                         Epoch.J2000, Coordinates.RAType.Hours);

                    DeepSkyObject dso = new DeepSkyObject(info[2], newCoordinates, string.Empty);
                    return dso;
                } else {
                    throw new PlanetariumObjectNotSelectedException();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Return the configured user location from HNSKY
        /// </summary>
        /// <returns></returns>
        public async Task<Coords> GetSite() {
            try {
                string command = "GET_LOCATION\r\n";

                var query = new BasicQuery(address, port, command);
                string response = await query.SendQuery();

                response = response.TrimEnd('\r', '\n');

                /*
                 * Split the latitude and longitude from the returned message.
                 * GET_LOCATION returns 3 fields, space-separated:
                 * Latitude Longitude Julian_Date
                 *
                 * Latitude and Logitude are in radians.
                 */
                var info = response.Split(' ');

                if (!(info[0].Equals("?") || string.IsNullOrEmpty(info[1]))) {
                    /*
                     * East is negative and West is positive in HNSKY.
                     * We must flip longitude's sign here.
                     */
                    var loc = new Coords {
                        Latitude = Astrometry.ToDegree(double.Parse(info[1].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture)),
                        Longitude = Astrometry.ToDegree(double.Parse(info[0].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture)) * -1,
                        Elevation = 0
                    };

                    return loc;
                } else {
                    throw new PlanetariumFailedToGetCoordinates();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
                throw ex;
            }
        }
    }
}