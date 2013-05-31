using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using System;
using System.Web.Http;
using Weave.Mobilizer.Cache;
using Weave.Mobilizer.Core.Web;

namespace Weave.Mobilizer.WebRole
{
    public static class WebApiConfig
    {
        static double cloudCacheTTL;
        static double cloudCacheCleanupInterval;
        static int localCacheTTL;

        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            if (!ReadConfigValues())
                throw new Exception("Unable to read the config values from Azure! (WebApiConfig.Register function)");

            var kernel = new Weave.Mobilizer.NinjectKernel.Kernel();
            var localCache = kernel.Get<LocalMemoryCache>();
            localCache.SetCacheTTLInMinutes(localCacheTTL);
            var cloudCache = kernel.Get<AzureStorageCache>();
            cloudCache.SetCacheTTLAndCleanupIntervalInHours(cloudCacheTTL, cloudCacheCleanupInterval);

            var resolver = new NinjectResolver(kernel);

            config.Configure(resolver);     
        }

        static bool ReadConfigValues()
        {
            var cloudCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("CloudCacheTTLInHours");
            if (!double.TryParse(cloudCacheTTLString, out cloudCacheTTL))
            {
                return false;
            }

            var cloudCacheCleanupIntervalString = RoleEnvironment.GetConfigurationSettingValue("CloudCacheCleanupInterval");
            if (!double.TryParse(cloudCacheCleanupIntervalString, out cloudCacheCleanupInterval))
            {
                return false;
            }

            var localCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("LocalCacheTTLInMinutes");
            if (!int.TryParse(localCacheTTLString, out localCacheTTL))
            {
                return false;
            }

            return true;
        }
    }
}
