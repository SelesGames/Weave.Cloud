using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Weave.RssAggregator.WorkerRole.Startup;

namespace Weave.RssAggregator.WorkerRole
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
            Trace.WriteLine("Weave.RssAggregator.WorkerRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
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
                Trace.WriteLine(string.Format("failed to start service: \r\n{0}\r\n{1}\r\n", e.Message, e.StackTrace));
            }
            return base.OnStart();
        }
    }
}
