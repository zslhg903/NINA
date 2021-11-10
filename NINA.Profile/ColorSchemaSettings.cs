#region "copyright"

/*
    Copyright © 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility.ColorSchema;
using NINA.Profile.Interfaces;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NINA.Profile {

    [Serializable()]
    [DataContract]
    [KnownType(typeof(ColorSchema))]
    public class ColorSchemaSettings : Settings, IColorSchemaSettings {

        public ColorSchemaSettings() : base() {
            SetDefaultValues();
        }

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context) {
            SetDefaultValues();
        }

        [OnDeserialized]
        private void SetValuesOnDeserialized(StreamingContext context) {
            Initialize();
        }

        private ColorSchema _altColorSchema;

        [DataMember]
        public ColorSchema AltColorSchema {
            get => _altColorSchema;
            set {
                if (_altColorSchema != value) {
                    if (_altColorSchema != null) {
                        _altColorSchema.PropertyChanged -= _colorSchema_PropertyChanged;
                    }
                    _altColorSchema = value;
                    _altColorSchema.PropertyChanged += _colorSchema_PropertyChanged;
                    RaisePropertyChanged();
                }
            }
        }

        private ColorSchema _colorSchema;

        [DataMember]
        public ColorSchema ColorSchema {
            get => _colorSchema;
            set {
                if (_colorSchema != value) {
                    if (_colorSchema != null) {
                        _colorSchema.PropertyChanged -= _colorSchema_PropertyChanged;
                    }
                    _colorSchema = value;
                    _colorSchema.PropertyChanged += _colorSchema_PropertyChanged;
                    RaisePropertyChanged();
                }
            }
        }

        private void _colorSchema_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            RaisePropertyChanged("Settings");
        }

        public ColorSchemas ColorSchemas { get; set; }

        private void Initialize() {
            var index = ColorSchemas.Items.FindIndex(x => x.Name == ColorSchema.Name);
            if (index > -1) {
                ColorSchemas.Items[index] = ColorSchema;
            }

            var index2 = ColorSchemas.Items.FindIndex(x => x.Name == AltColorSchema.Name);
            if (index2 > -1) {
                ColorSchemas.Items[index2] = AltColorSchema;
            }
        }

        protected override void SetDefaultValues() {
            ColorSchemas = ColorSchemas.ReadColorSchemas();
            var customSchema = ColorSchemas.CreateDefaultSchema();
            customSchema.Name = "Custom";
            ColorSchemas.Items.Add(customSchema);
            ColorSchema = ColorSchemas.Items.Where(x => x.Name == "Light").First();

            var altCustomSchema = ColorSchemas.CreateDefaultAltSchema();
            altCustomSchema.Name = "Alternative Custom";
            ColorSchemas.Items.Add(altCustomSchema);
            AltColorSchema = ColorSchemas.Items.Where(x => x.Name == "Dark").First();
        }

        public void ToggleSchema() {
            var tmp = ColorSchema;
            ColorSchema = AltColorSchema;
            AltColorSchema = tmp;
        }

        public void CopyToCustom() {
            var schema = ColorSchemas.Items.Where((x) => x.Name == "Custom").First();

            schema.PrimaryColor = ColorSchema.PrimaryColor;
            schema.SecondaryColor = ColorSchema.SecondaryColor;
            schema.BorderColor = ColorSchema.BorderColor;
            schema.BackgroundColor = ColorSchema.BackgroundColor;
            schema.SecondaryBackgroundColor = ColorSchema.SecondaryBackgroundColor;
            schema.TertiaryBackgroundColor = ColorSchema.TertiaryBackgroundColor;
            schema.ButtonBackgroundColor = ColorSchema.ButtonBackgroundColor;
            schema.ButtonBackgroundSelectedColor = ColorSchema.ButtonBackgroundSelectedColor;
            schema.ButtonForegroundColor = ColorSchema.ButtonForegroundColor;
            schema.ButtonForegroundDisabledColor = ColorSchema.ButtonForegroundDisabledColor;
            schema.NotificationWarningColor = ColorSchema.NotificationWarningColor;
            schema.NotificationWarningTextColor = ColorSchema.NotificationWarningTextColor;
            schema.NotificationErrorColor = ColorSchema.NotificationErrorColor;
            schema.NotificationErrorTextColor = ColorSchema.NotificationErrorTextColor;
            ColorSchema = schema;
        }

        public void CopyToAltCustom() {
            var schema = ColorSchemas.Items.Where((x) => x.Name == "Alternative Custom").First();

            schema.PrimaryColor = AltColorSchema.PrimaryColor;
            schema.SecondaryColor = AltColorSchema.SecondaryColor;
            schema.BorderColor = AltColorSchema.BorderColor;
            schema.BackgroundColor = AltColorSchema.BackgroundColor;
            schema.SecondaryBackgroundColor = AltColorSchema.SecondaryBackgroundColor;
            schema.TertiaryBackgroundColor = AltColorSchema.TertiaryBackgroundColor;
            schema.ButtonBackgroundColor = AltColorSchema.ButtonBackgroundColor;
            schema.ButtonBackgroundSelectedColor = AltColorSchema.ButtonBackgroundSelectedColor;
            schema.ButtonForegroundColor = AltColorSchema.ButtonForegroundColor;
            schema.ButtonForegroundDisabledColor = AltColorSchema.ButtonForegroundDisabledColor;
            schema.NotificationWarningColor = AltColorSchema.NotificationWarningColor;
            schema.NotificationWarningTextColor = AltColorSchema.NotificationWarningTextColor;
            schema.NotificationErrorColor = AltColorSchema.NotificationErrorColor;
            schema.NotificationErrorTextColor = AltColorSchema.NotificationErrorTextColor;
            AltColorSchema = schema;
        }
    }
}