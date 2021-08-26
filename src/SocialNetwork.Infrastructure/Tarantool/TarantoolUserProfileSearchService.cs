using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static Box _tarantoolClient;
        private static SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        public TarantoolUserProfileSearchService(IOptions<TarantoolConnectionOptions> options, ILogger<TarantoolUserProfileSearchService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<ICollection<UserProfile>> SearchUserProfilesAsync(string query, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(query)) return new List<UserProfile>();

            var connectionString = $"{_options.Value.Host}:{_options.Value.Port}";
            var connectionOptions = new ClientOptions(connectionString, new StringWriterLog());

            await InitClientAsync(connectionOptions);

            //await tarantoolClient.Eval<string>("dofile('/usr/local/share/tarantool/init.lua')");
            var sw = new Stopwatch();
            sw.Start();

            try
            {
             
                
                var profileTuples = await _tarantoolClient.Call_1_6<
                    TarantoolTuple<string, int, int>,
                    TarantoolTuple<long, string, string, int, string, string>>("find_profiles", TarantoolTuple.Create(query, page * pageSize, pageSize));

                sw.Stop();
                _logger.LogInformation($"Request to tarantool took {sw.Elapsed}");

                return profileTuples.Data.Select(TarantoolModelExtensions.ToProfile).ToList();
            }
            catch (ArgumentException e)
            {
                _logger.LogError($"Error in search call: {e.Message}");

                sw.Stop();
                _logger.LogInformation($"Request to tarantool took {sw.Elapsed}");

                return new List<UserProfile>();
            }
        }

        private static async ValueTask InitClientAsync(ClientOptions options)
        {
            if (_tarantoolClient == null)
            {
                await _connectionLock.WaitAsync();
                
                if (_tarantoolClient == null)
                {
                    try
                    {
                        _tarantoolClient = new Box(options);
                        await _tarantoolClient.Connect();
                    }
                    catch (Exception e)
                    {
                      
                    }
                }

                _connectionLock.Release();
            }
        }
    }
}