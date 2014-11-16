using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Weave.FeedUpdater.HighFrequency.Role
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("RssAggregator.Role.HighFrequency entry point called", "Information");

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
                Common.Net.Http.Compression.Settings.GlobalCompressionSettings.CompressionHandlers = 
                    new Common.Compression.Windows.CompressionHandlerCollection();

                var feedLibraryUrl = RoleEnvironment.GetConfigurationSettingValue("FeedLibraryUrl");
                var hfUpdater = new HighFrequencyFeedUpdater(feedLibraryUrl);

                hfUpdater.Initialize();
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("failed to start HF SERVER: \r\n{0}\r\n{1}\r\n", e.Message, e.StackTrace));
            }

            return base.OnStart();
        }
    }
}