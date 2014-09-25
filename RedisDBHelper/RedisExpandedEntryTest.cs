﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache;

namespace RedisDBHelper
{
    class RedisExpandedEntryTest
    {
        readonly string id;

        public RedisExpandedEntryTest(string id)
        {
            this.id = id;
        }

        public async Task<string> Execute()
        {
            var cache = ExpandedEntryCacheFactory.CreateCache(0);
            var guid = Guid.Parse(id);
            var result = await cache.Get(new[] { guid });

            if (result.Results.Any())
                return result.Dump();
            else
                return "not found";
        }
    }
}