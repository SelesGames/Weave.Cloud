using System;

namespace Weave.RssAggregator.Core
{
    public static class AppSettings
    {
        public static TimeSpan LowFrequencyHttpWebRequestTimeout { get; set; }

        public static void SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(int milliseconds)
        {
            LowFrequencyHttpWebRequestTimeout = TimeSpan.FromMilliseconds(milliseconds);
        }

        //public static TimeSpan HighFrequencyRefreshPeriod { get; private set; }

        //public static void SetHighFrequencyRefreshPeriodInMinutes(double minutes)
        //{
        //    HighFrequencyRefreshPeriod = TimeSpan.FromMinutes(minutes);
        //}

        //public static int HighFrequencyRefreshSplit { get; set; }

        static AppSettings()
        {
            LowFrequencyHttpWebRequestTimeout = TimeSpan.FromSeconds(10);
            //HighFrequencyRefreshPeriod = TimeSpan.FromMinutes(8);
            //HighFrequencyRefreshSplit = 32;
        }
    }
}
