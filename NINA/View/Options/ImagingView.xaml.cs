#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NINA.View.Options {

    /// <summary>
    /// Interaction logic for OptionsImagingView.xaml
    /// </summary>
    public partial class ImagingView : UserControl {

        public ImagingView() {
            InitializeComponent();
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e) {
            TextBox textBox = sender as TextBox;
            if (textBox != null && e != null) {
                // Set the caret at the position where user ended the drag-drop operation
                Point dropPosition = e.GetPosition(textBox);

                textBox.SelectionStart = GetCaretIndexFromPoint(textBox, dropPosition);
                textBox.SelectionLength = 0;

                // don't forget to set focus to the text box to make the caret visible!
                textBox.Focus();
                e.Handled = true;
            }
        }

        private void TextBox_Drop(object sender, DragEventArgs e) {
            var tb = sender as TextBox;
            string tstring;
            tstring = e.Data.GetData(DataFormats.StringFormat).ToString();
            tb.Text += tstring;
        }

        private void TextBox_DragEnter(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.Copy;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var item = sender as ListViewItem;
            if (item != null) {
                ImagePattern mySelectedItem = item.Content as ImagePattern;
                if (mySelectedItem != null) {
                    DragDrop.DoDragDrop(ImagePatternList, mySelectedItem.Key, DragDropEffects.Copy);
                }
            }
        }

        private void ListViewItem_PreviewMouseDoubleClick(object sender, RoutedEventArgs e) {
            var item = sender as ListViewItem;
            if (item != null) {
                ImagePattern mySelectedItem = item.Content as ImagePattern;
                if (mySelectedItem != null) {
                    ImageFilePatternTextBox.Text += mySelectedItem.Key;
                }
            }
        }

        /// <summary>
        /// Gets the caret index of a given point in the given textbox
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static int GetCaretIndexFromPoint(TextBox textBox, Point point) {
            int index = textBox.GetCharacterIndexFromPoint(point, true);

            // GetCharacterIndexFromPoint is missing one caret position, as there is one extra caret position than there are characters (an extra one at the end).
            //  We have to add that caret index if the given point is at the end of the textbox
            if (index == textBox.Text.Length - 1) {
                // Get the position of the character index using the bounding rectangle
                Rect caretRect = textBox.GetRectFromCharacterIndex(index);
                Point caretPoint = new Point(caretRect.X, caretRect.Y);

                if (point.X > caretPoint.X) {
                    index += 1;
                }
            }
            return index;
        }
    }
}