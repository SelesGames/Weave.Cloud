using Common.Net.Http.Compression;
using Common.WebApi.Handlers;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using System;
using System.Web.Http;
using Weave.Mobilizer.Cache;
using Weave.Mobilizer.Core.Controllers;

namespace Weave.Mobilizer.WebRole
{
    public static class WebApiConfig
    {
        //static double cloudCacheTTL;
        //static double cloudCacheCleanupInterval;
        static int localCacheTTL;

        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "InstapaperFormatterRouting",
                routeTemplate: "{controller}",
                defaults: new
                {
                    routTemplate = "ipf",
                    controller = typeof(IPFController),
                }
            );

            if (!ReadConfigValues())
                throw new Exception("Unable to read the config values from Azure! (WebApiConfig.Register function)");

            var kernel = new Startup.Kernel();
            var localCache = kernel.Get<LocalMemoryCache>();
            localCache.SetCacheTTLInMinutes(localCacheTTL);
            //var cloudCache = kernel.Get<AzureStorageCache>();
            //cloudCache.SetCacheTTLAndCleanupIntervalInHours(cloudCacheTTL, cloudCacheCleanupInterval);

            config.DependencyResolver = new NinjectResolver(kernel);
            config.MessageHandlers.Add(new InjectAcceptEncodingHandler("gzip"));
            config.MessageHandlers.Add(new EncodingDelegateHandler());// { ForceCompression = true });
        }

        static bool ReadConfigValues()
        {
            //var cloudCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("CloudCacheTTLInHours");
            //if (!double.TryParse(cloudCacheTTLString, out cloudCacheTTL))
            //{
            //    return false;
            //}

            //var cloudCacheCleanupIntervalString = RoleEnvironment.GetConfigurationSettingValue("CloudCacheCleanupInterval");
            //if (!double.TryParse(cloudCacheCleanupIntervalString, out cloudCacheCleanupInterval))
            //{
            //    return false;
            //}

            var localCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("LocalCacheTTLInMinutes");
            if (!int.TryParse(localCacheTTLString, out localCacheTTL))
            {
                return false;
            }

            return true;
        }
    }
}
