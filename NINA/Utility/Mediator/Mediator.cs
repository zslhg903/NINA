﻿using NINA.Model;
using NINA.Model.MyCamera;
using NINA.Model.MyFilterWheel;
using NINA.PlateSolving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NINA.Utility.Mediator {

    internal class Mediator {

        private Mediator() {
        }

        private static readonly Lazy<Mediator> lazy =
            new Lazy<Mediator>(() => new Mediator());

        public static Mediator Instance { get { return lazy.Value; } }

        private Dictionary<MediatorMessages, List<Action<Object>>> _internalList
            = new Dictionary<MediatorMessages, List<Action<Object>>>();

        public void ClearAll() {
            _internalList.Clear();
            _handlers.Clear();
            _asyncHandlers.Clear();
        }

        public void Register(Action<Object> callback,
              MediatorMessages message) {
            if (!_internalList.ContainsKey(message)) {
                _internalList[message] = new List<Action<object>>();
            }
            _internalList[message].Add(callback);
        }

        public void Notify(MediatorMessages message, object args) {
            if (_internalList.ContainsKey(message)) {
                //forward the message to all listeners
                foreach (Action<object> callback in _internalList[message]) {
                    callback(args);
                }
            }
        }

        /// <summary>
        /// Holds reference to handlers and identified by message type name
        /// </summary>
        private Dictionary<string, MessageHandle> _handlers = new Dictionary<string, MessageHandle>();

        /// <summary>
        /// Register handler to react on requests
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool RegisterRequest(MessageHandle handle) {
            if (!_handlers.ContainsKey(handle.RegisteredClass?.ToString() + handle.MessageType)) {
                _handlers.Add(handle.RegisteredClass?.ToString() + handle.MessageType, handle);
                return true;
            } else {
                throw new Exception("Handle already registered");
            }
        }

        /// <summary>
        /// Request a value from a handler based on message
        /// </summary>
        /// <typeparam name="T">Has to match the return type of the handle.Send()</typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        private T Request<T>(MediatorMessage<T> msg, Type requestedClass = null) {
            var key = requestedClass?.ToString() + msg.GetType().Name;
            if (_handlers.ContainsKey(key)) {
                var entry = _handlers[key];
                var handle = (MessageHandle<T>)entry;
                return handle.Send(msg);
            } else {
                return default(T);
            }
        }

        private ICollection<T> Request<T>(MediatorMessage<T> msg) {
            var key = msg.GetType().Name;
            List<T> returnMessages = new List<T>();
            foreach (var handler in _handlers.Where((k, v) => k.Key.EndsWith(key))) {
                var handle = (MessageHandle<T>)handler.Value;
                returnMessages.Add(handle.Send(msg));
            }
            return returnMessages;
        }

        public double Request(MediatorMessage<double> msg, Type requestedClass = null) {
            return Request<double>(msg, requestedClass);
        }

        public string Request(MediatorMessage<string> msg, Type requestedClass = null) {
            return Request<string>(msg, requestedClass);
        }

        public bool Request(MediatorMessage<bool> msg, Type requestedClass = null) {
            return Request<bool>(msg, requestedClass);
        }

        public FilterInfo Request(MediatorMessage<FilterInfo> msg) {
            return Request<FilterInfo>(msg, null);
        }

        public RMS Request(MediatorMessage<RMS> msg) {
            return Request<RMS>(msg, null);
        }

        public ICollection<FilterInfo> Request(MediatorMessage<ICollection<FilterInfo>> msg) {
            return Request(msg, null);
        }

        /// <summary>
        /// Holds reference to handlers and identified by message type name
        /// </summary>
        private Dictionary<string, AsyncMessageHandle> _asyncHandlers = new Dictionary<string, AsyncMessageHandle>();

        /// <summary>
        /// Register handler to react on requests
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool RegisterAsyncRequest(AsyncMessageHandle handle) {
            if (!_asyncHandlers.ContainsKey(handle.MessageType)) {
                _asyncHandlers.Add(handle.MessageType, handle);
                return true;
            } else {
                throw new Exception("Handle already registered");
            }
        }

        /// <summary>
        /// Request a value from a handler based on message
        /// </summary>
        /// <typeparam name="T">Has to match the return type of the handle.Send()</typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        private async Task<T> RequestAsync<T>(AsyncMediatorMessage<T> msg) {
            var key = msg.GetType().Name;
            if (_asyncHandlers.ContainsKey(key)) {
                var entry = _asyncHandlers[key];
                var handle = (AsyncMessageHandle<T>)entry;
                return await handle.Send(msg);
            } else {
                return default(T);
            }
        }

        public async Task<bool> RequestAsync(AsyncMediatorMessage<bool> msg) {
            return await RequestAsync<bool>(msg);
        }

        public async Task<int> RequestAsync(AsyncMediatorMessage<int> msg) {
            return await RequestAsync<int>(msg);
        }

        public async Task<PlateSolveResult> RequestAsync(AsyncMediatorMessage<PlateSolveResult> msg) {
            return await RequestAsync<PlateSolveResult>(msg);
        }

        public async Task<double> RequestAsync(AsyncMediatorMessage<double> msg) {
            return await RequestAsync<double>(msg);
        }

        public async Task<ImageArray> RequestAsync(AsyncMediatorMessage<ImageArray> msg) {
            return await RequestAsync<ImageArray>(msg);
        }

        public async Task<BitmapSource> RequestAsync(AsyncMediatorMessage<BitmapSource> msg) {
            return await RequestAsync<BitmapSource>(msg);
        }

        public async Task<FilterInfo> RequestAsync(AsyncMediatorMessage<FilterInfo> msg) {
            return await RequestAsync<FilterInfo>(msg);
        }
    }

    public enum MediatorMessages {
        TelescopeChanged = 3,
        AutoStrechChanged = 7,
        DetectStarsChanged = 8,
        ChangeAutoStretch = 14,
        ChangeDetectStars = 15,
        LocaleChanged = 18,
        LocationChanged = 19,
        FocuserTemperatureChanged = 26,
        FocuserConnectedChanged = 28,
        ProfileChanged = 31,
        TelescopeConnectedChanged = 32,
    };
}