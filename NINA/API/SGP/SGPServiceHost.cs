﻿using NINA.Core.Interfaces.API.SGP;
using NINA.Core.Locale;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Utility;
using Nito.AsyncEx;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.API.SGP {

    public class SGPServiceHost : ISGPServiceHost {
        private readonly AsyncManualResetEvent stopServiceEvent;
        private readonly ISGPService sgpService;
        private volatile Task serviceTask;

        public SGPServiceHost(ISGPService sgpService) {
            this.sgpService = sgpService;
            stopServiceEvent = new AsyncManualResetEvent(true);
            serviceTask = null;
        }

        /*
         * SGP is hardcoded to listen on localhost:59590. Depending on the system, this may be either an IPv4 or IPv6 loopback, so both should be configured to allow http connections.
         * The following commands should be run from an elevated command prompt:
         *
         * 1) netsh http add iplisten ipaddress=::
         * 2) netsh http add iplisten ipaddress=0.0.0.0
         * 3) netsh http add urlacl url=http://+:59590/ user=Everyone
         */

        public void RunService() {
            if (this.serviceTask != null) {
                Logger.Trace("SGP Service already running during start attempt");
                return;
            }

            Logger.Info("Starting SGP Service");
            stopServiceEvent.Reset();
            this.serviceTask = Task.Run(async () => {
                WebServiceHost hostWeb = null;
                try {
                    var webBinding = new WebHttpBinding() {
                        Name = "SGP-Compatible REST Server",
                        Security = new WebHttpSecurity() {
                            Mode = WebHttpSecurityMode.None
                        }
                    };

                    // SGP is hardcoded to listen on a specific port, and cannot be customized
                    hostWeb = new WebServiceHost(sgpService);
                    hostWeb.AddServiceEndpoint(typeof(ISGPService), webBinding, "http://127.0.0.1:59590");
                    ServiceDebugBehavior stp = hostWeb.Description.Behaviors.Find<ServiceDebugBehavior>();
                    stp.HttpHelpPageEnabled = false;
                    stp.IncludeExceptionDetailInFaults = true;
                    hostWeb.Open();

                    Logger.Info("SGP Service started");
                    await stopServiceEvent.WaitAsync();
                } catch (Exception ex) {
                    Logger.Error("Failed to start SGP Server", ex);
                    Notification.ShowError(string.Format(Loc.Instance["LblServerFailed"], ex.Message));
                    throw;
                } finally {
                    hostWeb?.Close();
                }
            });
        }

        public void Stop() {
            if (serviceTask != null) {
                Logger.Info("Stopping SGP Service");
                stopServiceEvent.Set();
                try {
                    serviceTask.Wait(new CancellationTokenSource(2000).Token);
                    Logger.Info("SGP Service stopped");
                } catch (Exception ex) {
                    Logger.Error("Failed to stop SGP Server", ex.Message);
                } finally {
                    serviceTask = null;
                }
            }
        }
    }
}