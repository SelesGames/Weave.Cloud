using Common.Compression.Windows;
using Microsoft.Owin.Hosting;
using System;
using System.Web.Http;

namespace SelesGames.WebApi.SelfHost
{
    public static class SelfHost
    {
        public static HttpConfiguration Config { get; private set; }

        static SelfHost()
        {
            Config = new StandardHttpSelfHostConfiguration();
            Common.Net.Http.Compression.Settings.GlobalCompressionSettings.CompressionHandlers = new CompressionHandlerCollection();
        }

        public static IDisposable StartServer(string url)
        {
            return WebApp.Start<Startup>(url);
        }
    }
}