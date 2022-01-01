#region "copyright"

/*
    Copyright � 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Windows;
using System.Windows.Input;

namespace NINA.WPF.Base.Behaviors {

    public class DragCommandBehavior {

        public static DragCommandBehavior GetBehavior(DependencyObject obj) {
            return (DragCommandBehavior)obj.GetValue(BehaviorProperty);
        }

        public static readonly DependencyProperty BehaviorProperty =
          DependencyProperty.RegisterAttached(
              "Behavior",
              typeof(DragCommandBehavior),
              typeof(DragCommandBehavior),
              new PropertyMetadata(new DragCommandBehavior()));

        public static bool GetOverrideCursor(DependencyObject obj) {
            return (bool)obj.GetValue(OverrideCursorProperty);
        }

        public static void SetOverrideCursor(DependencyObject obj, bool value) {
            obj.SetValue(OverrideCursorProperty, value);
        }

        public static readonly DependencyProperty OverrideCursorProperty =
          DependencyProperty.RegisterAttached(
              "OverrideCursor",
              typeof(bool),
              typeof(DragCommandBehavior),
              new PropertyMetadata(false));

        public static bool GetDrag(DependencyObject obj) {
            return (bool)obj.GetValue(IsDragProperty);
        }

        public static void SetDrag(DependencyObject obj, bool value) {
            obj.SetValue(IsDragProperty, value);
        }

        public static readonly DependencyProperty IsDragProperty =
          DependencyProperty.RegisterAttached(
              "Drag",
              typeof(bool),
              typeof(DragCommandBehavior),
              new PropertyMetadata(false, OnDragChanged));

        public static readonly DependencyProperty ResizeBoundaryProperty =
            DependencyProperty.RegisterAttached("ResizeBoundary", typeof(double), typeof(DragCommandBehavior), new PropertyMetadata(double.NaN));

        public static double GetResizeBoundary(DependencyObject obj) {
            return (double)obj.GetValue(ResizeBoundaryProperty);
        }

        public static void SetResizeBoundary(DependencyObject obj, double value) {
            obj.SetValue(ResizeBoundaryProperty, value);
        }

        //private RectangleDragMode _subSampleRectangleDragMode;

        private static void OnDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            // ignoring error checking
            var element = (UIElement)d;
            var isDrag = (bool)(e.NewValue);

            var instance = GetBehavior(element);

            if (isDrag) {
                element.MouseLeftButtonDown += instance.ElementOnMouseLeftButtonDown;
                element.MouseLeftButtonUp += instance.ElementOnMouseLeftButtonUp;
                element.MouseMove += instance.ElementOnMouseMove;
                element.MouseLeave += instance.Element_MouseLeave;

                element.TouchDown += instance.ElementOnTouchDown;
                element.TouchMove += instance.ElementOnTouchMove;
                element.TouchUp += instance.ElementOnTouchUp;

                element.StylusDown += instance.ElementOnStylusDown;
                element.StylusMove += instance.ElementOnStylusMove;
                element.StylusUp += instance.ElementOnStylusUp;
            } else {
                element.MouseLeftButtonDown -= instance.ElementOnMouseLeftButtonDown;
                element.MouseLeftButtonUp -= instance.ElementOnMouseLeftButtonUp;
                element.MouseMove -= instance.ElementOnMouseMove;
                element.MouseLeave -= instance.Element_MouseLeave;

                element.TouchDown -= instance.ElementOnTouchDown;
                element.TouchMove -= instance.ElementOnTouchMove;
                element.TouchUp -= instance.ElementOnTouchUp;

                element.StylusDown -= instance.ElementOnStylusDown;
                element.StylusMove -= instance.ElementOnStylusMove;
                element.StylusUp -= instance.ElementOnStylusUp;
            }
        }

        private void OnMove(FrameworkElement element, Point p) {
            var delta = p - _prevPosition;

            var cmd = GetDragMoveCommand(element);
            cmd?.Execute(new DragResult() { Delta = delta, Mode = _mode });

            _prevPosition = p;
        }

        private void OnUp(FrameworkElement element) {
            var cmd = GetDragStopCommand(element);
            cmd?.Execute(null);
        }

        private void OnDown(FrameworkElement element, Point p) {
            _prevPosition = p;

            var cmd = GetDragStartCommand(element);
            cmd?.Execute(null);
        }

        private void ElementOnStylusMove(object sender, StylusEventArgs e) {
            if (e.StylusDevice.Id == stylusId) {
                var element = (FrameworkElement)sender;
                var parent = (UIElement)element.Parent;
                var point = e.GetPosition(parent);

                OnMove(element, point);
            }
        }

        private void ElementOnStylusUp(object sender, StylusEventArgs e) {
            if (e.StylusDevice.Id == stylusId) {
                var element = (FrameworkElement)sender;
                element.ReleaseStylusCapture();
                OnUp(element);
                stylusId = -1;
            }
        }

        private int stylusId = -1;

        private void ElementOnStylusDown(object sender, StylusDownEventArgs e) {
            if (stylusId == -1) {
                var id = e.StylusDevice.Id;
                var element = (FrameworkElement)sender;

                var parent = (UIElement)element.Parent;
                var point = e.GetPosition(parent);

                OnDown(element, point);

                stylusId = id;
                element.CaptureStylus();
            }
        }

        private void ElementOnTouchMove(object sender, TouchEventArgs e) {
            var element = (FrameworkElement)sender;
            var parent = (UIElement)element.Parent;
            var point = e.TouchDevice.GetTouchPoint(parent).Position;

            OnMove(element, point);
        }

        private void ElementOnTouchUp(object sender, TouchEventArgs e) {
            var element = (FrameworkElement)sender;
            element.ReleaseTouchCapture(e.TouchDevice);
            OnUp(element);
        }

        private void ElementOnTouchDown(object sender, TouchEventArgs e) {
            var element = (FrameworkElement)sender;

            var parent = (UIElement)element.Parent;
            var point = e.TouchDevice.GetTouchPoint(parent).Position;

            OnDown(element, point);

            element.CaptureTouch(e.TouchDevice);
        }

        private void Element_MouseLeave(object sender, MouseEventArgs e) {
            var element = (FrameworkElement)sender;
            var overrideCursor = GetOverrideCursor(element);
            if (overrideCursor) {
                Mouse.OverrideCursor = null;
            }
        }

        private void ElementOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
            /*var parent = Application.Current.MainWindow;
            _mouseStartPosition2 = mouseButtonEventArgs.GetPosition(parent);*/

            var element = (FrameworkElement)sender;

            var parent = (UIElement)element.Parent;
            var boundary = GetResizeBoundary(element);
            var startPoint = mouseButtonEventArgs.GetPosition(element);
            var mousePos = mouseButtonEventArgs.GetPosition(parent);

            var overrideCursor = GetOverrideCursor(element);
            if (overrideCursor) {
                if (!double.IsNaN(boundary)) {
                    if (startPoint.X < boundary && startPoint.Y < boundary) {
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                        _mode = DragMode.Resize_Top_Left;
                    } else if (startPoint.X < boundary && startPoint.Y > element.Height - boundary) {
                        Mouse.OverrideCursor = Cursors.SizeNESW;
                        _mode = DragMode.Resize_Bottom_Left;
                    } else if (startPoint.X > element.Width - boundary && startPoint.Y < boundary) {
                        Mouse.OverrideCursor = Cursors.SizeNESW;
                        _mode = DragMode.Resize_Top_Right;
                    } else if (startPoint.X > element.Width - boundary && startPoint.Y > element.Height - boundary) {
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                        _mode = DragMode.Resize_Bottom_Right;
                    } else if (startPoint.X < boundary) {
                        Mouse.OverrideCursor = Cursors.SizeWE;
                        _mode = DragMode.Resize_Left;
                    } else if (startPoint.Y < boundary) {
                        Mouse.OverrideCursor = Cursors.SizeNS;
                        _mode = DragMode.Resize_Top;
                    } else if (startPoint.X > element.Width - boundary) {
                        Mouse.OverrideCursor = Cursors.SizeWE;
                        _mode = DragMode.Resize_Right;
                    } else if (startPoint.Y > element.Height - boundary) {
                        Mouse.OverrideCursor = Cursors.SizeNS;
                        _mode = DragMode.Resize_Bottom;
                    } else {
                        Mouse.OverrideCursor = Cursors.SizeAll;
                        _mode = DragMode.Move;
                    }
                } else {
                    Mouse.OverrideCursor = Cursors.SizeAll;
                    _mode = DragMode.Move;
                }
            }

            OnDown(element, mousePos);

            element.CaptureMouse();
        }

        private void ElementOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
            /*((UIElement)sender).ReleaseMouseCapture();
            _elementStartPosition2.X = Transform.X;
            _elementStartPosition2.Y = Transform.Y;*/
            var element = (FrameworkElement)sender;
            element.ReleaseMouseCapture();
            OnUp(element);
        }

        private Point _prevPosition;
        private DragMode _mode;

        private void ElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs) {
            var element = (FrameworkElement)sender;

            var parent = (UIElement)element.Parent;
            var startPoint = mouseEventArgs.GetPosition(element);
            var mousePos = mouseEventArgs.GetPosition(parent);
            var hitTest = System.Windows.Media.VisualTreeHelper.HitTest(element, startPoint);
            var boundary = GetResizeBoundary(element);

            if (!element.IsMouseCaptured) {
                var overrideCursor = GetOverrideCursor(element);
                if (overrideCursor) {
                    if (!double.IsNaN(boundary)) {
                        if (startPoint.X < boundary && startPoint.Y < boundary) {
                            Mouse.OverrideCursor = Cursors.SizeNWSE;
                        } else if (startPoint.X < boundary && startPoint.Y > element.Height - boundary) {
                            Mouse.OverrideCursor = Cursors.SizeNESW;
                        } else if (startPoint.X > element.Width - boundary && startPoint.Y < boundary) {
                            Mouse.OverrideCursor = Cursors.SizeNESW;
                        } else if (startPoint.X > element.Width - boundary && startPoint.Y > element.Height - boundary) {
                            Mouse.OverrideCursor = Cursors.SizeNWSE;
                        } else if (startPoint.X < boundary) {
                            Mouse.OverrideCursor = Cursors.SizeWE;
                        } else if (startPoint.Y < boundary) {
                            Mouse.OverrideCursor = Cursors.SizeNS;
                        } else if (startPoint.X > element.Width - boundary) {
                            Mouse.OverrideCursor = Cursors.SizeWE;
                        } else if (startPoint.Y > element.Height - boundary) {
                            Mouse.OverrideCursor = Cursors.SizeNS;
                        } else {
                            Mouse.OverrideCursor = Cursors.SizeAll;
                        }
                    } else {
                        Mouse.OverrideCursor = Cursors.SizeAll;
                    }
                }
            } else {
                OnMove(element, mousePos);
            }
        }

        public static readonly DependencyProperty DragStartCommandProperty =
            DependencyProperty.RegisterAttached("DragStartCommand", typeof(ICommand), typeof(DragCommandBehavior), new PropertyMetadata(null));

        public static ICommand GetDragStartCommand(DependencyObject obj) {
            return (ICommand)obj.GetValue(DragStartCommandProperty);
        }

        public static void SetDragStartCommand(DependencyObject obj, bool value) {
            obj.SetValue(DragStartCommandProperty, value);
        }

        public static readonly DependencyProperty DragStopCommandProperty =
            DependencyProperty.RegisterAttached("DragStopCommand", typeof(ICommand), typeof(DragCommandBehavior), new PropertyMetadata(null));

        public static ICommand GetDragStopCommand(DependencyObject obj) {
            return (ICommand)obj.GetValue(DragStopCommandProperty);
        }

        public static void SetDragStopCommand(DependencyObject obj, bool value) {
            obj.SetValue(DragStopCommandProperty, value);
        }

        public static readonly DependencyProperty DragMoveCommandProperty =
            DependencyProperty.RegisterAttached("DragMoveCommand", typeof(ICommand), typeof(DragCommandBehavior), new PropertyMetadata(null));

        public static ICommand GetDragMoveCommand(DependencyObject obj) {
            return (ICommand)obj.GetValue(DragMoveCommandProperty);
        }

        public static void SetDragMoveCommand(DependencyObject obj, bool value) {
            obj.SetValue(DragMoveCommandProperty, value);
        }
    }

    public enum DragMode {
        Move,
        Resize_Top_Left,
        Resize_Top_Right,
        Resize_Bottom_Left,
        Resize_Bottom_Right,
        Resize_Bottom,
        Resize_Top,
        Resize_Left,
        Resize_Right
    }

    public class DragResult {
        public DragMode Mode { get; set; }
        public Vector Delta { get; set; }
    }
}