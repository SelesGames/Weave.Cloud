using System;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    public static class AppSettings
    {
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
