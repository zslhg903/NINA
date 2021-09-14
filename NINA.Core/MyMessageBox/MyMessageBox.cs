#region "copyright"

/*
    Copyright � 2016 - 2021 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Core.Utility;
using NINA.Core.Utility.Extensions;
using System;
using System.Windows;

namespace NINA.Core.MyMessageBox {

    public class MyMessageBox : BaseINPC {
        private string _title;

        public string Title {
            get {
                return _title;
            }
            set {
                _title = value;
                RaisePropertyChanged();
            }
        }

        private string _text;

        public string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                RaisePropertyChanged();
            }
        }

        private bool? _dialogResult;

        public bool? DialogResult {
            get {
                return _dialogResult;
            }
            set {
                _dialogResult = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _cancelVisibility;

        public Visibility CancelVisibility {
            get {
                return _cancelVisibility;
            }
            set {
                _cancelVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _oKVisibility;

        public Visibility OKVisibility {
            get {
                return _oKVisibility;
            }
            set {
                _oKVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _yesVisibility;

        public Visibility YesVisibility {
            get {
                return _yesVisibility;
            }
            set {
                _yesVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _noVisibility;

        public Visibility NoVisibility {
            get {
                return _noVisibility;
            }
            set {
                _noVisibility = value;
                RaisePropertyChanged();
            }
        }

        public static MessageBoxResult Show(string messageBoxText) {
            return Show(messageBoxText, "", MessageBoxButton.OK, MessageBoxResult.OK);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption) {
            return Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxResult.OK);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultresult) {
            var dialogresult = defaultresult;
            dialogresult = Application.Current.Dispatcher.Invoke(() => {
                var MyMessageBox = new MyMessageBox();
                MyMessageBox.Title = caption;
                MyMessageBox.Text = messageBoxText;

                if (button == MessageBoxButton.OKCancel) {
                    MyMessageBox.CancelVisibility = System.Windows.Visibility.Visible;
                    MyMessageBox.OKVisibility = System.Windows.Visibility.Visible;
                    MyMessageBox.YesVisibility = System.Windows.Visibility.Hidden;
                    MyMessageBox.NoVisibility = System.Windows.Visibility.Hidden;
                } else if (button == MessageBoxButton.YesNo) {
                    MyMessageBox.CancelVisibility = System.Windows.Visibility.Hidden;
                    MyMessageBox.OKVisibility = System.Windows.Visibility.Hidden;
                    MyMessageBox.YesVisibility = System.Windows.Visibility.Visible;
                    MyMessageBox.NoVisibility = System.Windows.Visibility.Visible;
                } else if (button == MessageBoxButton.OK) {
                    MyMessageBox.CancelVisibility = System.Windows.Visibility.Hidden;
                    MyMessageBox.OKVisibility = System.Windows.Visibility.Visible;
                    MyMessageBox.YesVisibility = System.Windows.Visibility.Hidden;
                    MyMessageBox.NoVisibility = System.Windows.Visibility.Hidden;
                } else {
                    MyMessageBox.CancelVisibility = System.Windows.Visibility.Hidden;
                    MyMessageBox.OKVisibility = System.Windows.Visibility.Visible;
                    MyMessageBox.YesVisibility = System.Windows.Visibility.Hidden;
                    MyMessageBox.NoVisibility = System.Windows.Visibility.Hidden;
                }

                System.Windows.Window win = new MyMessageBoxView {
                    DataContext = MyMessageBox
                };
                win.SizeChanged += Win_SizeChanged;

                var mainwindow = System.Windows.Application.Current.MainWindow;
                mainwindow.Opacity = 0.8;
                win.Owner = mainwindow;
                win.Closed += (object sender, EventArgs e) => {
                    Application.Current.MainWindow.Focus();
                };
                win.ShowDialog();

                mainwindow.Opacity = 1;

                if (win.DialogResult == null) {
                    return defaultresult;
                } else if (win.DialogResult == true) {
                    if (MyMessageBox.YesVisibility == Visibility.Visible) {
                        return MessageBoxResult.Yes;
                    } else {
                        return MessageBoxResult.OK;
                    }
                } else if (win.DialogResult == false) {
                    if (MyMessageBox.NoVisibility == Visibility.Visible) {
                        return MessageBoxResult.No;
                    } else {
                        return MessageBoxResult.Cancel;
                    }
                } else {
                    return defaultresult;
                }
            });
            return dialogresult;
        }

        private static void Win_SizeChanged(object sender, SizeChangedEventArgs e) {
            var mainwindow = System.Windows.Application.Current.MainWindow;

            var win = (System.Windows.Window)sender;
            var rect = mainwindow.GetAbsolutePosition();
            win.Left = rect.Left + (rect.Width - win.ActualWidth) / 2;
            win.Top = rect.Top + (rect.Height - win.ActualHeight) / 2;
        }
    }
}