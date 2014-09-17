﻿using SelesGames.HttpClient;
using System;
using System.Threading.Tasks;

namespace Weave.FeedUpdater.HighFrequency.Processors.BestImageSelector
{
    class ImageInfoClient
    {
        readonly string urlFormat = "http://weave-imagecache.cloudapp.net/api/info?url={0}";

        public TimeSpan? Timeout { get; set; }

        public async Task<ImageInfo> Get(string url)
        {
            var fullUrl = string.Format(urlFormat, url);

            var client = new SmartHttpClient(CompressionSettings.None);
            if (Timeout.HasValue)
                client.Timeout = Timeout.Value;

            using (var response = await client.GetAsync(fullUrl))
            {
                var responseMessage = response.HttpResponseMessage;

                if (responseMessage.IsSuccessStatusCode &&
                    responseMessage.Content.Headers.ContentLength.HasValue &&
                    responseMessage.Content.Headers.ContentLength.Value < 10)
                {
                    var stringResponse = await responseMessage.Content.ReadAsStringAsync();
                    if (stringResponse == "null")
                        throw new InvalidImageException();
                }

                var info = await response.Read<ImageInfo>();
                return info;
            }
        }
    }

    public class InvalidImageException : Exception
    {

    }
}
