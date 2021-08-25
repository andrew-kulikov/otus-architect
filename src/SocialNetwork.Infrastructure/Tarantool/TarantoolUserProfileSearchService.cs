using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProGaudi.Tarantool.Client;
using SocialNetwork.Core.Entities;
using SocialNetwork.Infrastructure.Services;

namespace SocialNetwork.Infrastructure.Tarantool
{
    public class TarantoolUserProfileSearchService: IUserProfileSearchService
    {
        private readonly IOptions<TarantoolConnectionOptions> _options;

        public TarantoolUserProfileSearchService(IOptions<TarantoolConnectionOptions> options)
        {
            _options = options;
        }

        public async Task<ICollection<UserProfile>> SearchUserProfilesAsync(string query, int page, int pageSize)
        {
            using (var tarantoolClient = await Box.Connect(_options.Value.Host, _options.Value.Port))
            {

            }
        }
    }
}
