using Common.Windows.Compression;
using Microsoft.Owin.Hosting;
using System;
using System.Web.Http;

namespace SelesGames.WebApi.SelfHost
{
    public class SelfHost
    {
        public HttpConfiguration Config { get; private set; }

        public SelfHost()
        {
            Config = new StandardHttpSelfHostConfiguration();
            Common.Compression.Settings.CompressionHandlers = new CompressionHandlerCollection();
        }

        public IDisposable StartServer(string url)
        {
            return WebApp.Start(url, appBuilder => appBuilder.Use(Config));
        }
    }
}