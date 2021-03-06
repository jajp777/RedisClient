﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceComparison.Tests.SimpleTests
{
    public class StackExchangeRedisSimpleTest : ITest
    {
        StackExchange.Redis.ConnectionMultiplexer _connection;

        public async Task Init(IPEndPoint endpoint, System.Threading.CancellationToken cancel)
        {
            _connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(endpoint.ToString()).ConfigureAwait(false);
            await _connection.GetDatabase().StringIncrementAsync("whatever").ConfigureAwait(false);
        }

        public async Task RunClient(Int32 id, System.Threading.CancellationToken cancel)
        {
            var key = id.ToString();
            var db = _connection.GetDatabase();
            await db.StringIncrementAsync(key).ConfigureAwait(false);
        }

        public void ClearData()
        {
            //var ep = _connection.GetEndPoints();
            //await _connection.GetServer( "localhost:6379, allowAdmin=true").FlushDatabaseAsync().ConfigureAwait(false);
            _connection.Dispose();
        }
    }
}
