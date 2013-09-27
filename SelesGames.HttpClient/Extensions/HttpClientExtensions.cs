using System;
using System.Net.Http;

namespace SelesGames.HttpClient.Extensions
{
    public static class HttpClientExtensions
    {
        public static IDisposable PollChangesToResource(this System.Net.Http.HttpClient client, string resourceUrl, TimeSpan pollingInterval, IObserver<HttpResponseMessage> observer)
        {
            var listener = new HttpResourceListener(client, resourceUrl);
            listener.PollingInterval = pollingInterval;

            listener.RegisterOnUpdatedCallback(observer.OnNext, observer.OnError);

            listener.BeginListening();

            return listener;
        }

        public static IDisposable PollChangesToResource(this System.Net.Http.HttpClient client,
            string resourceUrl,
            TimeSpan pollingInterval,
            Action<HttpResponseMessage> onUpdated,
            Action<Exception> onException = null)
        {
            var listener = new HttpResourceListener(client, resourceUrl);
            listener.PollingInterval = pollingInterval;

            listener.RegisterOnUpdatedCallback(onUpdated, onException);

            listener.BeginListening();

            return listener;
        }
    }
}
