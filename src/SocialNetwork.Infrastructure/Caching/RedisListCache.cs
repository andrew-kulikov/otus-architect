using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CachingFramework.Redis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
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

        public bool Any(string key)
        {
            try
            {
                var context = new RedisContext(_redisCacheClient.GetDbFromConfiguration().Database.Multiplexer);

                var list = context.Collections.GetRedisList<T>(key);

                return list.Count != 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Redis exception: {e.Message}");

                return false;
            }
        }

        public async Task<List<T>> GetAsync(string key)
        {
            try
            {
                var context = new RedisContext(_redisCacheClient.GetDbFromConfiguration().Database.Multiplexer);

                var list = context.Collections.GetRedisList<T>(key);

                return list.GetRange().ToList();
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
                var context = new RedisContext(_redisCacheClient.GetDbFromConfiguration().Database.Multiplexer);

                var list = context.Collections.GetRedisList<T>(key);

                await list.ClearAsync();
                list.AddRange(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Redis exception: {e.Message}");
            }
        }

        public async Task AddAsync(string key, T data)
        {
            try
            {
                var context = new RedisContext(_redisCacheClient.GetDbFromConfiguration().Database.Multiplexer);

                var list = context.Collections.GetRedisList<T>(key);
                await list.AddAsync(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Redis exception: {e.Message}");
            }
        }
    }
}