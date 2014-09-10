using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weave.User.BusinessObjects.Mutable
{
    public struct ArticleDeletionTime
    {
        readonly TimeSpan ts;

        ArticleDeletionTime(TimeSpan ts)
        {
            this.ts = ts;
        }




        #region Implicit to/from TimeSpan

        public static implicit operator ArticleDeletionTime(TimeSpan ts)
        {
            return new ArticleDeletionTime(ts);
        }

        public static implicit operator TimeSpan(ArticleDeletionTime o)
        {
            return o.ts;
        }

        #endregion




        #region Implicit to/from string

        public static implicit operator ArticleDeletionTime(string s)
        {
            return new ArticleDeletionTime(FromString(s));
        }

        public static implicit operator string(ArticleDeletionTime o)
        {
            return ToString(o.ts);
        }

        #endregion




        #region Helper function - converts a string to a TimeSpan

        static TimeSpan FromString(string s)
        {
            if (s == null)
                return TimeSpan.MaxValue;

            if (s.Equals("never", StringComparison.OrdinalIgnoreCase)
                || s.Equals("none", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.MaxValue;

            var tuples = s.Split(' ').Pair();
            var timeSpans = tuples
                .Select(o => GetTimeSpan(o.Item1, o.Item2))
                .Where(o => o.HasValue)
                .Select(o => o.Value);

            var aggregate = timeSpans.Aggregate(TimeSpan.Zero, (seed, o) => seed += o);
            if (aggregate <= TimeSpan.Zero)
                return TimeSpan.MaxValue;

            return aggregate;
        }

        static TimeSpan? GetTimeSpan(string numeric, string qualifier)
        {
            double numVal;
            if (!double.TryParse(numeric, out numVal))
                return null;

            if (qualifier.Equals("day", StringComparison.OrdinalIgnoreCase)
                || qualifier.Equals("days", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromDays(numVal);

            if (qualifier.Equals("hour", StringComparison.OrdinalIgnoreCase)
                || qualifier.Equals("hours", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromHours(numVal);

            if (qualifier.Equals("minute", StringComparison.OrdinalIgnoreCase)
                || qualifier.Equals("minutes", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromMinutes(numVal);

            if (qualifier.Equals("second", StringComparison.OrdinalIgnoreCase)
                || qualifier.Equals("seconds", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromSeconds(numVal);

            return null;
        }

        #endregion




        #region Helper function - converts a TimeSpan to a string

        static string ToString(TimeSpan ts)
        {
            if (ts == TimeSpan.MaxValue)
                return "never";

            var sb = new StringBuilder();

            var days = ts.Days;
            if (days > 0)
                sb.Append(string.Format("{0} {1} ", days, days == 1 ? "day" : "days"));

            var hours = ts.Hours;
            if (hours > 0)
                sb.Append(string.Format("{0} {1} ", hours, hours == 1 ? "hour" : "hours"));

            var minutes = ts.Minutes;
            if (minutes > 0)
                sb.Append(string.Format("{0} {1} ", minutes, minutes == 1 ? "minute" : "minutes"));

            var seconds = ts.Seconds;
            if (minutes > 0)
                sb.Append(string.Format("{0} {1} ", seconds, seconds == 1 ? "second" : "seconds"));

            return sb.ToString().Trim();
        }

        #endregion




        public override string ToString()
        {
            return (string)this;
        }
    }

    static class ext
    {
        public static IEnumerable<Tuple<T, T>> Pair<T>(this IEnumerable<T> source)
        {
            var enumer = source.GetEnumerator();

            while (enumer.MoveNext())
            {
                var first = enumer.Current;
                if (enumer.MoveNext())
                {
                    var second = enumer.Current;
                    yield return Tuple.Create(first, second);
                }
            }
        }
    }
}
