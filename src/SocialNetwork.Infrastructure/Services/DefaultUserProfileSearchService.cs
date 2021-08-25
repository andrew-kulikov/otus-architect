using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Infrastructure.Services
{
    public class DefaultUserProfileSearchService : IUserProfileSearchService
    {
        private readonly IUserProfileRepository _userProfileRepository;

        public DefaultUserProfileSearchService(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public async Task<ICollection<UserProfile>> SearchUserProfilesAsync(string query, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(query)) return new List<UserProfile>();
            return await _userProfileRepository.SearchUserProfilesAsync(query, page, pageSize);
        }
    }
}