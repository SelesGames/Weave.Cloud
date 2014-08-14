using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis.Serializers;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    class StandardStringCache<TKey, TResult>
    {
        readonly IDatabaseAsync db;
        readonly Func<TKey, RedisKey> keyMap;
        readonly Func<TResult, TKey> objToKeyMap;
        readonly RedisValueSerializer<TResult> serializer;

        public StandardStringCache(IDatabaseAsync db, Func<TKey, RedisKey> keyMap, Func<TResult, TKey> objToKeyMap, RedisValueSerializer<TResult> serializer)
        {
            this.db = db;
            this.keyMap = keyMap;
            this.objToKeyMap = objToKeyMap;
            this.serializer = serializer;
        }

        public async Task<RedisCacheResult<TResult>> Get(TKey key)
        {
            var redisKey = keyMap(key);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var value = await db.StringGetAsync(redisKey, CommandFlags.None);
            sw.Stop();
            DebugEx.WriteLine("the actual getting of the user index took {0} ms", sw.ElapsedMilliseconds);

            sw.Restart();
            var cacheResult = serializer.Read(value);
            sw.Stop();
            DebugEx.WriteLine("deserializing the user index took {0} ms", sw.ElapsedMilliseconds);

            return cacheResult;
        }

        public Task<bool> Save(TResult obj)
        {
            var key = objToKeyMap(obj);
            var redisKey = keyMap(key);
            var val = serializer.Write(obj);

            return db.StringSetAsync(
                key: redisKey,
                value: val,
                expiry: TimeSpan.FromDays(7),
                when: When.Always,
                flags: CommandFlags.HighPriority);
        }
    }

    class StandardStringCache2<TKey, TResult>
    {
        readonly StringGet<TKey, TResult> get;
        readonly StringSet<TKey, TResult> set;

        public StandardStringCache2(StringGet<TKey, TResult> get, StringSet<TKey, TResult> set)
        {
            this.get = get;
            this.set = set;
        }

        public Task<RedisCacheResult<TResult>> Get(TKey key)
        {
            return get.Get(key);
        }

        public Task<bool> Save(TResult obj)
        {
            return set.Save(obj);
        }
    }

    class StringGet<TKey, TResult>
    {
        readonly IDatabaseAsync db;
        readonly Func<TKey, RedisKey> keyMap;
        readonly RedisValueSerializer<TResult> serializer;

        public CommandFlags Flags { get; set; }

        public StringGet(IDatabaseAsync db, Func<TKey, RedisKey> keyMap, RedisValueSerializer<TResult> serializer)
        {
            this.db = db;
            this.keyMap = keyMap;
            this.serializer = serializer;

            Flags = CommandFlags.None;
        }

        public async Task<RedisCacheResult<TResult>> Get(TKey key)
        {
            var redisKey = keyMap(key);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var value = await db.StringGetAsync(redisKey, Flags);
            sw.Stop();
            DebugEx.WriteLine("the actual getting of the user index took {0} ms", sw.ElapsedMilliseconds);

            sw.Restart();
            var cacheResult = serializer.Read(value);
            sw.Stop();
            DebugEx.WriteLine("deserializing the user index took {0} ms", sw.ElapsedMilliseconds);

            return cacheResult;
        }
    }

    class StringSet<TKey, TResult>
    {
        readonly IDatabaseAsync db;
        readonly Func<TKey, RedisKey> keyMap;
        readonly Func<TResult, TKey> objToKeyMap;
        readonly RedisValueSerializer<TResult> serializer;

        public TimeSpan? Expiry { get; set; }
        public When When { get; set; }
        public CommandFlags Flags { get; set; }

        public StringSet(IDatabaseAsync db, Func<TKey, RedisKey> keyMap, Func<TResult, TKey> objToKeyMap, RedisValueSerializer<TResult> serializer)
        {
            this.db = db;
            this.keyMap = keyMap;
            this.objToKeyMap = objToKeyMap;
            this.serializer = serializer;

            Expiry = null;
            When = When.Always;
            Flags = CommandFlags.None;
        }

        public Task<bool> Save(TResult obj)
        {
            var key = objToKeyMap(obj);
            var redisKey = keyMap(key);
            var val = serializer.Write(obj);

            return db.StringSetAsync(
                key: redisKey,
                value: val,
                expiry: Expiry,
                when: When,
                flags: Flags);
        }
    }

    class TestUserIndexCache : StandardStringCache2<Guid, UserIndex>
    {
        public TestUserIndexCache(IDatabaseAsync db)
            : base(CreateGet(db), CreateSet(db))
        { }

        static StringGet<Guid, UserIndex> CreateGet(IDatabaseAsync db)
        {
            return new StringGet<Guid, UserIndex>(db, id => id.ToByteArray(), new UserIndexBinarySerializer())
            {
                Flags = CommandFlags.None,
            };
        }

        static StringSet<Guid, UserIndex> CreateSet(IDatabaseAsync db)
        {
            return new StringSet<Guid, UserIndex>(db, id => id.ToByteArray(), user => user.Id, new UserIndexBinarySerializer())
            {
                Expiry = TimeSpan.FromDays(7),
                When = When.Always,
                Flags = CommandFlags.HighPriority,
            };
        }
    }
}