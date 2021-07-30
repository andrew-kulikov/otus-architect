using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace SocialNetwork.Infrastructure.Caching
{
    public class RedisListCache<T>: IListCache<T>
    {
        private readonly IRedisCacheClient _redisCacheClient;

        public RedisListCache(IRedisCacheClient redisCacheClient)
        {
            _redisCacheClient = redisCacheClient;
        }

        public async Task<List<T>> GetAsync(string key) => 
            await _redisCacheClient.GetDbFromConfiguration().GetAsync<List<T>>(key);

        public async Task SetAsync(string key, List<T> data) => 
            await _redisCacheClient.GetDbFromConfiguration().AddAsync(key, data, DateTimeOffset.UtcNow.AddHours(6));
    }
}