using System;

namespace Weave.User.Service.Redis
{
    public class TimingHelper
    {
        System.Diagnostics.Stopwatch sw;

        public void Start()
        {
            if (Timings.AreEnabled)
            {
                if (sw == null)
                    sw = System.Diagnostics.Stopwatch.StartNew();
                else
                    sw.Restart();
            }
        }

        public TimeSpan Record()
        {
            if (Timings.AreEnabled && sw != null)
            {
                sw.Stop();
                return sw.Elapsed;
            }

            return TimeSpan.Zero;
        }
    }

    public static class TimeSpanFormattingExtensions
    {
        public static string Dump(this TimeSpan t)
        {
            return t.TotalMilliseconds + " ms";
        }
    }
}
