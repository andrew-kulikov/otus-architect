using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace SocialNetwork.Infrastructure.Caching
{
    // TODO: Error handling if redis is unavailable at start
    public class RedisListCache<T> : IListCache<T>
    {
        private readonly ILogger<RedisListCache<T>> _logger;
        private readonly IRedisCacheClient _redisCacheClient;

        public RedisListCache(IRedisCacheClient redisCacheClient, ILogger<RedisListCache<T>> logger)
        {
            _redisCacheClient = redisCacheClient;
            _logger = logger;
        }

        public async Task<List<T>> GetAsync(string key)
        {
            try
            {
                return await _redisCacheClient.GetDbFromConfiguration().GetAsync<List<T>>(key);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Redis exception: {e.Message}");

                return new List<T>();
            }
        }

        public async Task SetAsync(string key, List<T> data)
        {
            try
            {
                await _redisCacheClient.GetDbFromConfiguration().AddAsync(key, data, DateTimeOffset.UtcNow.AddHours(6));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Redis exception: {e.Message}");
            }
        }
    }
}