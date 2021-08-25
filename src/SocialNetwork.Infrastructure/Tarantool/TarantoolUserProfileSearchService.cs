using System;
using System.Collections.Generic;
using System.Linq;
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

            using (var tarantoolClient = new Box(connectionOptions))
            {
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
    }
}