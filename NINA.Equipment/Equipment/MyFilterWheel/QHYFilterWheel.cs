#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using ASCOM;
using NINA.Profile.Interfaces;
using NINA.Core.Utility;
using QHYCCD;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Model.Equipment;
using NINA.Equipment.Interfaces;

namespace NINA.Equipment.Equipment.MyFilterWheel {

    public class QHYFilterWheel : BaseINPC, IFilterWheel {
        private QhySdk.QHYCCD_FILTER_WHEEL_INFO Info;
        private bool _connected = false;
        private IProfileService profileService;
        private bool moveRequested = false;
        private string destinationPostition = string.Empty;
        public IQhySdk Sdk { get; set; } = QhySdk.Instance;

        public QHYFilterWheel(string fwheel, IProfileService profileService) {
            this.profileService = profileService;

            StringBuilder FWheelId = new StringBuilder(32);
            StringBuilder cameraModel = new StringBuilder(0);

            FWheelId.Append(fwheel);
            Sdk.GetModel(FWheelId, cameraModel);

            Info.Id = FWheelId;
            Sdk.Open(Info.Id);

            if (Sdk.IsCfwPlugged()) {
                Info.Positions = (uint)Sdk.GetControlValue(QhySdk.CONTROL_ID.CONTROL_CFWSLOTSNUM);
            } else {
                Logger.Error($"QHYCFW: {Id} suddenly has no filter wheel!");
                return;
            }

            Info.Name = string.Format($"{cameraModel} {Info.Positions}-Slot Filter Wheel");

            Sdk.Close();

            Logger.Debug($"QHYCFW: Found filter wheel: {Name}");
        }

        public async Task<bool> Connect(CancellationToken token) {
            return await Task.Run(() => {
                byte[] position = new byte[1];

                Sdk.InitSdk();

                Logger.Debug($"QHYCFW: Connecting to filter wheel {Name}");
                Sdk.Open(Info.Id);
                Sdk.InitCamera();

                if (!Sdk.IsCfwPlugged()) {
                    Sdk.Close();

                    string errMessage = $"CFW {Name} is not found on the connected camera!";
                    Logger.Error($"QHYCFW: " + errMessage);
                    throw new InvalidOperationException(errMessage);
                }

                return Connected = true;
            });
        }

        public bool Connected {
            get => _connected;
            private set {
                _connected = value;
                RaisePropertyChanged();
            }
        }

        public string Id => Info.Id.ToString();
        public string Name => Info.Name;
        public string Category => "QHYCCD";
        public string Description => string.Format($"Integrated or 4-pin Filter Wheel on {Info.Id}");
        public string DriverInfo => "Native driver for QHY integrated or 4-pin filter wheels";
        public string DriverVersion => "1.0";
        public short InterfaceVersion => 1;

        public int[] FocusOffsets => this.Filters.Select((x) => x.FocusOffset).ToArray();

        public string[] Names => this.Filters.Select((x) => x.Name).ToArray();

        public short Position {
            get {
                uint rv;
                byte[] status = new byte[1];
                short position;
                string statusString;

                if ((rv = Sdk.GetCfwStatus(status)) != QhySdk.QHYCCD_SUCCESS) {
                    Logger.Error($"QHYCFW: Failed to get filter wheel position: {rv}");
                    return -1;
                }

                statusString = string.Join("", Encoding.ASCII.GetChars(status));
                Logger.Debug($"QHYCFW: Current position: {statusString}");

                /*
                 * GetQHYCCDCFWStatus() can return the following status:
                 * - CFW2, CFW3: ASCII 78 "N" while the filter wheel is in motion.
                 * - A-Series cameras: ASCII 47 "/" file filter wheel is initializing, but the position number the wheel is at while it is moving
                 * We return -1 while the filter wheel is moving, per the ASCOM specification
                 */
                if (statusString == "N" || statusString == "/" || (moveRequested && !statusString.Equals(destinationPostition))) {
                    // The filter wheel is in motion
                    position = -1;
                } else {
                    // The filter wheel is at a filter postition
                    moveRequested = false;
                    destinationPostition = string.Empty;
                    short.TryParse(statusString, out position);
                }

                return position;
            }
            set {
                string position = destinationPostition = value.ToString("X1");
                moveRequested = true;

                Logger.Debug($"QHYCFW: Moving to position {value} (str: {position})");

                if (Sdk.SendOrderToCfw(position, position.Length) != QhySdk.QHYCCD_SUCCESS) {
                    Logger.Error($"QHYCFW: Failed to order move to position {value} (str: {position})!");
                    moveRequested = false;
                    destinationPostition = string.Empty;
                    return;
                }

                RaisePropertyChanged();
            }
        }

        public ArrayList SupportedActions => new ArrayList();

        public void Disconnect() {
            Logger.Debug($"QHYCFW: Closing filter wheel {Name}");

            Connected = false;
            Sdk.Close();
            Sdk.ReleaseSdk();
        }

        public AsyncObservableCollection<FilterInfo> Filters {
            get {
                var filtersList = profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters;
                int i = filtersList.Count();
                int positions = (int)Info.Positions;

                if (positions < i) {
                    /* Too many filters defined. Truncate the list */
                    for (; i > (int)Info.Positions; i--) {
                        filtersList.RemoveAt(i - 1);
                    }
                } else if (positions > i) {
                    /* Too few filters defined. Add missing filter names using Slot <#> format */
                    for (; i <= positions; i++) {
                        var filter = new FilterInfo(string.Format($"Slot {i}"), 0, (short)i);
                        filtersList.Add(filter);
                    }
                }

                return filtersList;
            }
        }

        public bool HasSetupDialog => false;

        public void SetupDialog() {
        }
    }
}