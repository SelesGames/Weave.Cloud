using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;

namespace Weave.User.Service.Redis
{
    class NewsItemScore
    {
        public Guid Id { get; set; }
        public double Score { get; set; }
    }

    public static class NewsItemScoringExtensions
    {
        public static NewsItemScore CreateScore(NewsItem o)
        {
            return new NewsItemScore
            {
                Id = o.Id,
                Score = ScoreHelper.CalculateScore(o.UtcPublishDateTime.Ticks),
            };
        }
    }

    public class SortedNewsItemsSetCache
    {
        readonly ConnectionMultiplexer connection;
        readonly IDatabase db;

        public SortedNewsItemsSetCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            db = connection.GetDatabase(0);
        }

        public async Task AddNewsItemsToSet(Guid feedId, IEnumerable<NewsItemScore> scores)
        {
            var key = feedId.ToByteArray();
            var vals = scores.Select(o => new SortedSetEntry(o.Id.ToByteArray(), o.Score)).ToArray();

            var numAdded = await db.SortedSetAddAsync(key, vals, flags: CommandFlags.None);
        }

        // BEWARE - THIS KEY THAT IS CREATED, IS ONLY VALID UNTIL THE NEXT TIME THE USER REFRESHES THIS PARTICULAR
        // SET OF FEEDS (I.E. in a category, all news, etc.).  So it needs to be deleted
        // because it takes up space, and because the data will be invalid on the next refresh
        // Consider storing this value somewhere on the User
        /// <summary>
        /// In Redis, create a sorted set that represents the Union of sorted articles of multiple Feed Ids
        /// </summary>
        /// <returns>The Guid, as a byte array, of the key for the newly created set</returns>
        public async Task<byte[]> CreateUnionSet(IEnumerable<Guid> feedIds)
        {
            var destinationKey = Guid.NewGuid().ToByteArray();
            var setKeys = feedIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();

            await db.SortedSetCombineAndStoreAsync(SetOperation.Union, destinationKey, setKeys,
                flags: CommandFlags.None);

            return destinationKey;
        }

        /// <summary>
        /// Get the News Item Ids for a particular sorted list (either an 
        /// individual feed or a temporarily list representing the union of 
        /// multiple feeds), denoting at what index you want to start and how 
        /// many ids you want to take
        /// </summary>
        public async Task<IEnumerable<Guid>> GetNewsItemIds(byte[] key, int skip, int take)
        {
            var vals = await db.SortedSetRangeByRankAsync(key, 
                start: skip, 
                stop: skip + take, 
                order: Order.Ascending, 
                flags: CommandFlags.None);

            return vals.Select(o => new Guid((byte[])o));
        }

        public async Task TrimList(byte[] key, int trimTo)
        {
            var numRemoved = await db.SortedSetRemoveRangeByRankAsync(key, trimTo, -1, flags: CommandFlags.None);
        }
    }



    static class ScoreHelper
    {
        // Number of 100ns ticks per time unit
        const long TicksPerMillisecond = 10000;
        const long TicksPerSecond = TicksPerMillisecond * 1000;
        const long TicksPerMinute = TicksPerSecond * 60;
        const long TicksPerHour = TicksPerMinute * 60;
        const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        const int MillisPerSecond = 1000;
        const int MillisPerMinute = MillisPerSecond * 60;
        const int MillisPerHour = MillisPerMinute * 60;
        const int MillisPerDay = MillisPerHour * 24;

        // Number of days in a non-leap year
        const int DaysPerYear = 365;
        // Number of days in 4 years
        const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Number of days in 100 years
        const int DaysPer100Years = DaysPer4Years * 25 - 1;
        // Number of days in 400 years
        const int DaysPer400Years = DaysPer100Years * 4 + 1;

        // Number of days from 1/1/0001 to 12/30/1899
        const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/9999
        const long DoubleDateOffset = DaysTo1899 * TicksPerDay;

        const long DaysTo3000 = DaysPer400Years * 7 + DaysPer100Years * 2;
        const long SuperDateOffset = DaysTo3000 * TicksPerDay;

        // The minimum OA date is 0100/01/01 (Note it's year 100).
        // The maximum OA date is 9999/12/31
        const long OADateMinAsTicks = (DaysPer100Years - DaysPerYear) * TicksPerDay;

        public static double CalculateScore(long value)
        {
            if (value == 0)
                return 0.0;  // Returns OleAut's zero'ed date value.

            if (value < TicksPerDay) // This is a fix for VB. They want the default day to be 1/1/0001 rathar then 12/30/1899.
                value += DoubleDateOffset; // We could have moved this fix down but we would like to keep the bounds check.
            
            //if (value < OADateMinAsTicks)
            //    throw new OverflowException("modified date score effup");

            // Currently, our max date == OA's max date (12/31/9999), so we don't
            // need an overflow check in that direction.
            //long millis = (value - DoubleDateOffset) / TicksPerMillisecond;
            long millis = (SuperDateOffset - value) / TicksPerMillisecond;
            if (millis < 0)
            {
                long frac = millis % MillisPerDay;
                if (frac != 0) millis -= (MillisPerDay + frac) * 2;
            }
            return (double)millis / MillisPerDay;
        }
    }
}
