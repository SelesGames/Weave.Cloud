﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.Testing
{
    class Program
    {
        static async void Main(string[] args)
        {
            var multiplexer = await ConnectionMultiplexer.ConnectAsync("serverUrl");
            var db = multiplexer.GetDatabase(0);

            SortedSetEntry[] values = new SortedSetEntry[] 
            { 
                new SortedSetEntry("hello hi", 67),
                new SortedSetEntry("hello hi", 99),
            };
            await db.SortedSetAddAsync("key", values);


        }
    }
}
