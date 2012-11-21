using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Weave.RssAggregator.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("Weave.RssAggregator.WorkerRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                //Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;


            IPEndPoint ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["RssAggregatorService"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());

            RoleEnvironment.Changed += (s, e) =>
            {
                var configChanges = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>().ToList();

                if (configChanges.Any(o =>
                    o.ConfigurationSettingName == "DummyHighFrequencyFeedsProccer" ||
                    o.ConfigurationSettingName == "HighFrequencyRefreshPeriod" ||
                    o.ConfigurationSettingName == "HighFrequencyRefreshSplit"))
                {
                    SetHighFrequencyValues();
                }

                if (configChanges.Any(o => o.ConfigurationSettingName == "LowFrequencyHttpWebRequestTimeout"))
                {
                    SetLowFrequencyValues();
                }
            };

            SetHighFrequencyValues();
            SetLowFrequencyValues();

            try
            {
                Weave.RssAggregator.Core.WcfEndpointCreator.CreateEndpoint(ipString);
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("failed to start Rss Aggregator Service: \r\n{0}\r\n{1}\r\n", e.Message, e.StackTrace));
                return false;
            }

            Trace.WriteLine("Rss Aggregator Service Running!");


            return base.OnStart();
        }

        static void SetHighFrequencyValues()
        {
            var temp = RoleEnvironment.GetConfigurationSettingValue("HighFrequencyRefreshPeriod");
            var value = double.Parse(temp);
            AppSettings.SetHighFrequencyRefreshPeriodInMinutes(value);

            temp = RoleEnvironment.GetConfigurationSettingValue("HighFrequencyRefreshSplit");
            var ivalue = int.Parse(temp);
            AppSettings.HighFrequencyRefreshSplit = ivalue;

            temp = RoleEnvironment.GetConfigurationSettingValue("FeedLibraryUrl");
            var feedLibraryUrl = temp;// "http://weavestorage.blob.core.windows.net/settings/masterfeeds.xml";
            HighFrequencyFeedCacheService.CreateCache(feedLibraryUrl);
        }

        static void SetLowFrequencyValues()
        {
            var temp = RoleEnvironment.GetConfigurationSettingValue("LowFrequencyHttpWebRequestTimeout");
            var value = int.Parse(temp);
            AppSettings.SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(value);
        }
    }
}
