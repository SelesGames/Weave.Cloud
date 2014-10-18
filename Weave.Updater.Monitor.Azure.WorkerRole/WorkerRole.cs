using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Weave.FeedUpdater.Messaging;

namespace Weave.FeedUpdater.Monitor.Role
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("Weave.Updater.Monitor.Azure.WorkerRole entry point called");

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
                var persister = new FeedUpdatePersister(
                    "weaveuser2",
                    "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                    "updaterfeeds");

                persister.Initialize();
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