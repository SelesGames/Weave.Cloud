﻿using System;

namespace Weave.RssAggregator.WorkerRole.LowFrequency.Startup
{
    public static class AppSettings
    {
        public static string InternalHighFrequencyEndpoint { get; set; }
        public static TimeSpan LowFrequencyHttpWebRequestTimeout { get; set; }

        public static void SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(int milliseconds)
        {
            LowFrequencyHttpWebRequestTimeout = TimeSpan.FromMilliseconds(milliseconds);
        }

        static AppSettings()
        {
            LowFrequencyHttpWebRequestTimeout = TimeSpan.FromSeconds(10);
        }
    }
}