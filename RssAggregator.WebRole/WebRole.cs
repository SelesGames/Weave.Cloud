using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;

namespace RssAggregator.WebRole
{
    public class WebRole : RoleEntryPoint
    {

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            ServicePointManager.DefaultConnectionLimit = 12;
            return base.OnStart();
        }
    }
}
