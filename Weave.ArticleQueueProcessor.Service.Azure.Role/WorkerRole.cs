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
using Weave.ArticleQueueProcessor.Service.Azure.Role.Startup;

namespace Weave.ArticleQueueProcessor.Service.Azure.Role
{
    public class WorkerRole : RoleEntryPoint
    {
        StartupTask startupTask;

        public WorkerRole()
        {
            startupTask = new StartupTask();
        }

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("Weave.ArticleQueueProcessor.Service.Azure.Role entry point called");

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
                startupTask.OnStart();
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
