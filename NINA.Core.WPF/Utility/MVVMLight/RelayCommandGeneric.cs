﻿#region "copyright"

// ****************************************************************************
// <copyright file="RelayCommandGeneric.cs" company="GalaSoft Laurent Bugnion">
// Copyright © GalaSoft Laurent Bugnion 2009-2016
// </copyright>
// ****************************************************************************
// <author>Laurent Bugnion</author>
// <email>laurent@galasoft.ch</email>
// <date>22.4.2009</date>
// <project>GalaSoft.MvvmLight</project>
// <web>http://www.mvvmlight.net</web>
// <license>
// See http://www.galasoft.ch/license_MIT.txt
// </license>
// ****************************************************************************
// <credits>This class was developed by Josh Smith (http://joshsmithonwpf.wordpress.com) and
// slightly modified with his permission.</credits>
// ****************************************************************************

#endregion "copyright"

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using GalaSoft.MvvmLight.Helpers;

#if NETFX_CORE
using System.Reflection;
#endif

////using GalaSoft.Utilities.Attributes;

#if PLATFORMNET45
namespace GalaSoft.MvvmLight.CommandWpf
#else

namespace GalaSoft.MvvmLight.Command
#endif
{
    /// <summary>
    /// A generic command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute
    /// method is 'true'. This class allows you to accept command parameters in the
    /// Execute and CanExecute callback methods.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <remarks>If you are using this class in WPF4.5 or above, you need to use the
    /// GalaSoft.MvvmLight.CommandWpf namespace (instead of GalaSoft.MvvmLight.Command).
    /// This will enable (or restore) the CommandManager class which handles
    /// automatic enabling/disabling of controls based on the CanExecute delegate.</remarks>
    ////[ClassInfo(typeof(RelayCommand))]
    public class RelayCommand<T> : ICommand {
        private readonly WeakAction<T> _execute;

        private readonly WeakFunc<T, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class that
        /// can always execute.
        /// </summary>
        /// <param name="execute">The execution logic. IMPORTANT: If the action causes a closure,
        /// you must set keepTargetAlive to true to avoid side effects. </param>
        /// <param name="keepTargetAlive">If true, the target of the Action will
        /// be kept as a hard reference, which might cause a memory leak. You should only set this
        /// parameter to true if the action is causing a closure. See
        /// http://galasoft.ch/s/mvvmweakaction. </param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public RelayCommand(Action<T> execute, bool keepTargetAlive = false)
            : this(execute, null, keepTargetAlive) {
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic. IMPORTANT: If the action causes a closure,
        /// you must set keepTargetAlive to true to avoid side effects. </param>
        /// <param name="canExecute">The execution status logic.  IMPORTANT: If the func causes a closure,
        /// you must set keepTargetAlive to true to avoid side effects. </param>
        /// <param name="keepTargetAlive">If true, the target of the Action will
        /// be kept as a hard reference, which might cause a memory leak. You should only set this
        /// parameter to true if the action is causing a closure. See
        /// http://galasoft.ch/s/mvvmweakaction. </param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute, bool keepTargetAlive = false) {
            if (execute == null) {
                throw new ArgumentNullException("execute");
            }

            _execute = new WeakAction<T>(execute, keepTargetAlive);

            if (canExecute != null) {
                _canExecute = new WeakFunc<T, bool>(canExecute, keepTargetAlive);
            }
        }

#if SILVERLIGHT
        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
#elif NETFX_CORE
        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
#elif XAMARIN
        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
#else

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged {
            add {
                if (_canExecute != null) {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove {
                if (_canExecute != null) {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

#endif

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "The this keyword is used in the Silverlight version")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1030:UseEventsWhereAppropriate",
            Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged() {
#if SILVERLIGHT
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#elif NETFX_CORE
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#elif XAMARIN
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#else
            CommandManager.InvalidateRequerySuggested();
#endif
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data
        /// to be passed, this object can be set to a null reference</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter) {
            if (_canExecute == null) {
                return true;
            }

            if (_canExecute.IsStatic || _canExecute.IsAlive) {
                if (parameter == null
#if NETFX_CORE
                    && typeof(T).GetTypeInfo().IsValueType)
#else
                    && typeof(T).IsValueType)
#endif
                {
                    return _canExecute.Execute(default(T));
                }

                if (parameter == null || parameter is T) {
                    return (_canExecute.Execute((T)parameter));
                }
            }

            return false;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data
        /// to be passed, this object can be set to a null reference</param>
        public virtual void Execute(object parameter) {
            var val = parameter;

#if !NETFX_CORE
            if (parameter != null
                && parameter.GetType() != typeof(T)) {
                if (parameter is IConvertible) {
                    val = Convert.ChangeType(parameter, typeof(T), null);
                }
            }
#endif

            if (CanExecute(val)
                && _execute != null
                && (_execute.IsStatic || _execute.IsAlive)) {
                if (val == null) {
#if NETFX_CORE
                    if (typeof(T).GetTypeInfo().IsValueType)
#else
                    if (typeof(T).IsValueType)
#endif
                    {
                        _execute.Execute(default(T));
                    } else {
                        // ReSharper disable ExpressionIsAlwaysNull
                        _execute.Execute((T)val);
                        // ReSharper restore ExpressionIsAlwaysNull
                    }
                } else {
                    _execute.Execute((T)val);
                }
            }
        }
    }
}