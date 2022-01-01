#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Interfaces;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NINACustomControlLibrary {

    public class AutoCompleteBox : HintTextBox {

        static AutoCompleteBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        public static readonly DependencyProperty PopupBackgroundProperty =
           DependencyProperty.Register(nameof(PopupBackground), typeof(Brush), typeof(AutoCompleteBox), new UIPropertyMetadata(Brushes.Transparent));

        public Brush PopupBackground {
            get {
                return (Brush)GetValue(PopupBackgroundProperty);
            }
            set {
                SetValue(PopupBackgroundProperty, value);
            }
        }

        public static readonly DependencyProperty SearchResultProperty =
           DependencyProperty.Register(nameof(SearchResult), typeof(ICollection<IAutoCompleteItem>), typeof(AutoCompleteBox), new UIPropertyMetadata(null));

        public ICollection<IAutoCompleteItem> SearchResult {
            get {
                return (ICollection<IAutoCompleteItem>)GetValue(SearchResultProperty);
            }
            set {
                SetValue(SearchResultProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedSearchResultProperty =
           DependencyProperty.Register(nameof(SelectedSearchResult), typeof(IAutoCompleteItem), typeof(AutoCompleteBox), new UIPropertyMetadata(null));

        public IAutoCompleteItem SelectedSearchResult {
            get {
                return (IAutoCompleteItem)GetValue(SelectedSearchResultProperty);
            }
            set {
                SetValue(SelectedSearchResultProperty, value);
            }
        }

        public static readonly DependencyProperty ShowPopupProperty =
           DependencyProperty.Register(nameof(ShowPopup), typeof(bool), typeof(AutoCompleteBox), new UIPropertyMetadata(false));

        public bool ShowPopup {
            get {
                return (bool)GetValue(ShowPopupProperty);
            }
            set {
                SetValue(ShowPopupProperty, value);
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var tb = GetTemplateChild("PART_TextBox") as TextBox;
            if (tb != null) {
                tb.PreviewKeyDown += Tb_PreviewKeyDown; ;
                tb.LostFocus += Tb_LostFocus;
            }
            var list = GetTemplateChild("PART_SearchCommandResultView") as ListView;
            if (list != null) {
                list.PreviewKeyDown += List_PreviewKeyDown;
                list.PreviewKeyUp += List_PreviewKeyUp;
                list.SelectionChanged += List_SelectionChanged;
            }
        }

        private void Tb_LostFocus(object sender, RoutedEventArgs e) {
            var list = GetTemplateChild("PART_SearchCommandResultView") as ListView;
            var subItemFocused = false;
            if (list != null) {
                for (int i = 0; i < list.Items.Count; i++) {
                    ListViewItem item = list.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (item != null) {
                        if (item.IsFocused) {
                            subItemFocused = true;
                            break;
                        }
                    }
                }
            }
            if (!subItemFocused) {
                ShowPopup = false;
            }
        }

        private void List_PreviewKeyUp(object sender, KeyEventArgs e) {
            forceShowPopup = false;
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!forceShowPopup) {
                ShowPopup = false;
            }
        }

        private void List_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case (Key.Back): {
                        var tb = GetTemplateChild("PART_TextBox") as TextBox;
                        if (tb != null) {
                            tb.Focus();
                        }
                        ShowPopup = false;
                        break;
                    }

                case (Key.Enter): {
                        ShowPopup = false;
                        break;
                    }
                case (Key.Up):
                case (Key.Down): {
                        forceShowPopup = true;
                        break;
                    }
            }
        }

        /// <summary>
        /// Flag to suppress popup closing on selectionchange, when navigating using arrow keys
        /// </summary>
        private bool forceShowPopup;

        private void Tb_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (ShowPopup) {
                switch (e.Key) {
                    case (Key.Enter): {
                            var list = GetTemplateChild("PART_SearchCommandResultView") as ListView;
                            if (list != null && list.Items.Count > 0) {
                                ListViewItem item = list.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                                if (item != null) {
                                    item.IsSelected = true;
                                }
                                ShowPopup = false;
                            }
                            break;
                        }
                    case (Key.Down): {
                            var list = GetTemplateChild("PART_SearchCommandResultView") as ListView;
                            if (list != null && list.Items.Count > 0) {
                                forceShowPopup = true;
                                ListViewItem item = list.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                                if (item != null) {
                                    item.Focus();
                                    item.IsSelected = true;
                                    ShowPopup = true;
                                }
                            }
                            break;
                        }
                    case (Key.Tab):
                    case (Key.Escape): {
                            ShowPopup = false;
                            break;
                        }
                }
            }
        }
    }
}