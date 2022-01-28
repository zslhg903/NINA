﻿#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Profile.Interfaces;
using NINA.Core.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWOptical.EFWSDK;
using NINA.Core.Model.Equipment;
using NINA.Equipment.Interfaces;

namespace NINA.Equipment.Equipment.MyFilterWheel {

    public class ASIFilterWheel : BaseINPC, IFilterWheel {
        private int id;
        private IProfileService profileService;

        public ASIFilterWheel(int idx, IProfileService profileService) {
            _ = EFWdll.GetID(idx, out var id);
            this.id = id;
            this.profileService = profileService;
        }

        public short InterfaceVersion => 1;

        public int[] FocusOffsets => this.Filters.Select((x) => x.FocusOffset).ToArray();

        public string[] Names => this.Filters.Select((x) => x.Name).ToArray();

        public ArrayList SupportedActions => new ArrayList();

        public AsyncObservableCollection<FilterInfo> Filters {
            get {
                var filtersList = profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters;
                var positions = info.slotNum;

                // Find duplicate positions due to data corruption and remove duplicates
                var duplicates = filtersList.GroupBy(x => x.Position).Where(x => x.Count() > 1).ToList();
                foreach (var group in duplicates) {
                    foreach (var filterToRemove in group) {
                        Logger.Warning($"Duplicate filter position defined in filter list. Removing the duplicates and importing from filter wheel again. Removing filter: {filterToRemove.Name}, focus offset: {filterToRemove.FocusOffset}");
                        filtersList.Remove(filterToRemove);
                    }
                }

                if (filtersList.Count > 0) {
                    // Scan for missing position indexes between 0 .. maxPosition and reimport them
                    var existingPositions = filtersList.Select(x => (int)x.Position).ToList();
                    var missingPositions = Enumerable.Range(0, existingPositions.Max()).Except(existingPositions);
                    foreach (var position in missingPositions) {
                        if (positions > position) {
                            var filterToAdd = new FilterInfo(string.Format($"Slot {position}"), 0, (short)position);
                            Logger.Warning($"Missing filter position. Importing filter: {filterToAdd.Name}, focus offset: {filterToAdd.FocusOffset}");
                            filtersList.Insert(position, filterToAdd);
                        }
                    }
                }

                int i = filtersList.Count();


                if (positions < i) {
                    /* Too many filters defined. Truncate the list */
                    for (; i > positions; i--) {
                        var filterToRemove = filtersList[i - 1];
                        Logger.Warning($"Too many filters defined in the equipment filter list. Removing filter: {filterToRemove.Name}, focus offset: {filterToRemove.FocusOffset}");
                        filtersList.Remove(filterToRemove);
                    }
                } else if (positions > i) {
                    /* Too few filters defined. Add missing filter names using Slot <#> format */
                    for (; i <= positions; i++) {
                        var filter = new FilterInfo(string.Format($"Slot {i}"), 0, (short)i);
                        filtersList.Add(filter);
                        Logger.Info($"Not enough filters defined in the equipment filter list. Importing filter: {filter.Name}, focus offset: {filter.FocusOffset}");
                    }
                }

                return filtersList;
            }
        }

        public bool HasSetupDialog { get; } = false;

        public string Id {
            get {
                return $"{Name} {id}";
            }
        }

        public string Name => "ZWOptical FilterWheel";

        public string Category => "ZWOptical";

        private bool _connected = false;
        private EFWdll.EFW_INFO info;

        public bool Connected {
            get => _connected;
            private set {
                _connected = value;
                RaisePropertyChanged();
            }
        }

        public string Description => "Native driver for ZWOptical filter wheels";

        public string DriverInfo => "Native driver for ZWOptical filter wheels";

        public string DriverVersion => "1.0";

        public short Position {
            get {
                var err = EFWdll.GetPosition(this.info.ID, out var position);
                if (err == EFWdll.EFW_ERROR_CODE.EFW_SUCCESS) {
                    return (short)position;
                } else {
                    Logger.Error($"EFW Communication error to get position {err}");
                    return -1;
                }
            }

            set {
                var err = EFWdll.SetPosition(this.info.ID, value);
                if (err != EFWdll.EFW_ERROR_CODE.EFW_SUCCESS) {
                    Logger.Error($"EFW Communication error during position change {err}");
                }
            }
        }

        public Task<bool> Connect(CancellationToken token) {
            return Task.Run(() => {
                if (EFWdll.Open(this.id) == EFWdll.EFW_ERROR_CODE.EFW_SUCCESS) {
                    Connected = true;

                    EFWdll.GetProperty(this.id, out var info);
                    this.info = info;

                    EFWdll.SetDirection(this.info.ID, false);

                    Connected = true;
                    return true;
                } else {
                    Logger.Error("Failed to connect to EFW");
                    return false;
                };
            });
        }

        public void Disconnect() {
            _ = EFWdll.Close(this.id);
            this.Connected = false;
        }

        public void SetupDialog() {
        }
    }
}