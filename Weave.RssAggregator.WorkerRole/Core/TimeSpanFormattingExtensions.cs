﻿using System;

namespace Weave.RssAggregator
{
    static class TimeSpanFormattingExtensions
    {
        public static string Dump(this TimeSpan t)
        {
            return t.TotalMilliseconds + " ms";
        }
    }
}