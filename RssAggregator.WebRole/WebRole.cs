using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;
using Weave.RssAggregator.WorkerRole.Startup;

namespace RssAggregator.WebRole
{
    public class WebRole : RoleEntryPoint
    {
        StartupTask startupTask;

        public WebRole()
        {
            startupTask = new StartupTask();
        }

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            DebugEx.WriteLine(ServicePointManager.DefaultConnectionLimit);

            startupTask.OnStart();

            return base.OnStart();
        }
    }
}
