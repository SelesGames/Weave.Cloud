using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace Weave.User.Service.UpdateProcessor
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("Weave.User.Service.UpdateProcessor entry point called");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            try
            {
                Helper.StartAsync().Wait();
            }
            catch (Exception e)
            {
                Trace.TraceInformation(string.Format("failed to start service: \r\n{0}\r\n{1}\r\n", e.Message, e.StackTrace));
                throw;
            }

            return base.OnStart();
        }
    }
}