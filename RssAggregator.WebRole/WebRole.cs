using Microsoft.WindowsAzure.ServiceRuntime;
using RssAggregator.WebRole.Startup;
using System.Net;

namespace RssAggregator.WebRole
{
    public class WebRole : RoleEntryPoint
    {
        //StartupTask startupTask;

        //public WebRole()
        //{
        //    startupTask = new StartupTask();
        //}

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            ServicePointManager.DefaultConnectionLimit = 12;

            DebugEx.WriteLine(ServicePointManager.DefaultConnectionLimit);

            //startupTask.OnStart();

            return base.OnStart();
        }
    }
}
