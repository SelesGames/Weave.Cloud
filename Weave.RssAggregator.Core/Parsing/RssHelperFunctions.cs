using System;
using System.Globalization;
using System.Linq;

namespace Weave.RssAggregator.Core.Parsing
{
    public static class RssHelperFunctions
    {
        public static Tuple<bool, DateTime> TryGetUtcDate(string dateTimeString)
        {
            try
            {
                if (string.IsNullOrEmpty(dateTimeString))
                    return Tuple.Create(false, DateTime.MinValue);

                DateTime dateTime;

                var canRfcParse = SyndicationDateTimeUtility
                    .TryParseRfc822DateTime(dateTimeString, out dateTime);

                if (!canRfcParse)
                {
                    var canNormalParse = DateTime.TryParse(dateTimeString, out dateTime);
                    if (!canNormalParse)
                    {
                        string canParseAny = new[] { "ddd, dd MMM yyyy HH:mm:ss ZK", "yyyy-MM-ddTHH:mm:ssK" }
                            .FirstOrDefault(o => DateTime.TryParseExact(dateTimeString, o, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));

                        if (canParseAny == null)
                            return Tuple.Create(false, DateTime.MinValue);
                    }
                }

                //if (dateTime.Kind == DateTimeKind.Local)
                //{
                //    var local = TimeZone.CurrentTimeZone;
                //    dateTime = local.ToUniversalTime(dateTime);
                //    //dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
                //}

                //else if (dateTime.Kind == DateTimeKind.Unspecified)
                //    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                //if (dateTime > DateTime.UtcNow)
                //    dateTime = DateTime.UtcNow;

                return Tuple.Create(true, dateTime);
            }
            catch (Exception)
            {
                return Tuple.Create(false, DateTime.MinValue);
            }
        }
    }
}
