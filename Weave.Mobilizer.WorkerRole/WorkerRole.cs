using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Weave.Mobilizer.Core.Cache;
using Weave.Mobilizer.Core.Web;

namespace Weave.Mobilizer.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("Weave.Mobilizer.WorkerRole entry point called", "Information");

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

            //var baseUrl = RoleEnvironment.GetConfigurationSettingValue("BaseUrl");

            var cloudCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("CloudCacheTTLInHours");
            double cloudCacheTTL;
            if (!double.TryParse(cloudCacheTTLString, out cloudCacheTTL))
            {
                return false;
            }

            var cloudCacheCleanupIntervalString = RoleEnvironment.GetConfigurationSettingValue("CloudCacheCleanupInterval");
            double cloudCacheCleanupInterval;
            if (!double.TryParse(cloudCacheCleanupIntervalString, out cloudCacheCleanupInterval))
            {
                return false;
            }

            var localCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("LocalCacheTTLInMinutes");
            int localCacheTTL;
            if (!int.TryParse(localCacheTTLString, out localCacheTTL))
            {
                return false;
            }


            //InstapaperMobilizerFormatter.BaseArticleRedirectUrl = baseUrl;
            //InstapaperMobilizerFormatter.CacheTTLInMinutes = cacheTTLInMinutes;
            IPEndPoint ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["MobilizerService"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            try
            {
                Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));
                var kernel = new Weave.Mobilizer.NinjectKernel.Kernel();
                var localCache = kernel.Get<LocalMemoryCache>();
                localCache.SetCacheTTLInMinutes(localCacheTTL);
                var cloudCache = kernel.Get<AzureStorageCache>();
                cloudCache.SetCacheTTLAndCleanupIntervalInHours(cloudCacheTTL, cloudCacheCleanupInterval);

                var resolver = new NinjectResolver(kernel);
                var selfHost = new SelfHost(ipString, resolver);
                selfHost.StartServer().Wait();
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("failed to start mobilizer: \r\n{0}\r\n{1}\r\n", e.Message, e.StackTrace));
            }
            Trace.WriteLine("IP Formatter Service Running!", "Information");

            return base.OnStart();
        }
    }
}
