using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProGaudi.Tarantool.Client;
using ProGaudi.Tarantool.Client.Model;
using SocialNetwork.Core.Entities;
using SocialNetwork.Infrastructure.Services;

namespace SocialNetwork.Infrastructure.Tarantool
{
    public class TarantoolUserProfileSearchService : IUserProfileSearchService
    {
        private readonly IOptions<TarantoolConnectionOptions> _options;
        private readonly ILogger<TarantoolUserProfileSearchService> _logger;
        private readonly TarantoolClientPool _tarantoolClientPool;

        public TarantoolUserProfileSearchService(
            IOptions<TarantoolConnectionOptions> options, 
            ILogger<TarantoolUserProfileSearchService> logger, 
            TarantoolClientPool tarantoolClientPool)
        {
            _options = options;
            _logger = logger;
            _tarantoolClientPool = tarantoolClientPool;
        }

        public async Task<ICollection<UserProfile>> SearchUserProfilesAsync(string query, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(query)) return new List<UserProfile>();

            var tarantoolClient = await _tarantoolClientPool.GetConnectedClientAsync(_options.Value);
        
            //await tarantoolClient.Eval<string>("dofile('/usr/local/share/tarantool/init.lua')");

            try
            {
                var profileTuples = await tarantoolClient.Call_1_6<
                    TarantoolTuple<string, int, int>,
                    TarantoolTuple<long, string, string, int, string, string>>("find_profiles", TarantoolTuple.Create(query, page * pageSize, pageSize));

                return profileTuples.Data.Select(TarantoolModelExtensions.ToProfile).ToList();
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Error in search call");

                return new List<UserProfile>();
            }
        }
    }

    public class TarantoolClientPool: IDisposable
    {
        private const int PoolSize = 5;

        private readonly Box[] _clientPool;
        private readonly Random _random;
        private readonly SemaphoreSlim[] _connectionLocks;
        private readonly ILogger<TarantoolClientPool> _logger;

        public TarantoolClientPool(ILogger<TarantoolClientPool> logger)
        {
            _logger = logger;
            _clientPool = new Box[PoolSize];
            _connectionLocks = new SemaphoreSlim[PoolSize];
            _random = new Random(42);
    

            InitializeLocks();
        }

        private void InitializeLocks()
        {
            for (int i = 0; i < PoolSize; i++)
            {
                _connectionLocks[i] = new SemaphoreSlim(1, 1);
            }
        }

        public async ValueTask<Box> GetConnectedClientAsync(TarantoolConnectionOptions options)
        {
            var currentId = _random.Next(PoolSize);
            var client = await GetClient(options, currentId);

            await ConnectClient(client, currentId);

            return client;
        }

        private async ValueTask<Box> GetClient(TarantoolConnectionOptions options, int currentId)
        {
            if (_clientPool[currentId] != null) return _clientPool[currentId];

            await ExecuteInLock(currentId, () =>
            {
                if (_clientPool[currentId] == null)
                {
                    _clientPool[currentId] = new Box(CreateTarantoolOptions(options));

                    _logger.LogInformation($"Created tarantool client with id {currentId}");
                }

                return Task.CompletedTask;
            });

            return _clientPool[currentId];
        }

        private async ValueTask ConnectClient(Box client, int currentId)
        {
            if (client.IsConnected) return;

            await ExecuteInLock(currentId, async () => await client.Connect());
        }

        private async Task ExecuteInLock(int currentId, Func<Task> action)
        {
            var currentLock = _connectionLocks[currentId];
            await currentLock.WaitAsync();

            await action.Invoke();
            
            currentLock.Release();
        }

        private ClientOptions CreateTarantoolOptions(TarantoolConnectionOptions options)
        {
            var connectionString = $"{options.Host}:{options.Port}";
            return new ClientOptions(connectionString, new StringWriterLog());
        }

        public void Dispose()
        {
            foreach (var client in _clientPool)
            {
                client.Dispose();
            }

            foreach (var connectionLock in _connectionLocks)
            {
                connectionLock.Dispose();
            }
        }
    }
}