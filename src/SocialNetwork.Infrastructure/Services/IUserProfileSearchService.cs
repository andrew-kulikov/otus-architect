using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Infrastructure.Services
{
    public interface IUserProfileSearchService
    {
        Task<ICollection<UserProfile>> SearchUserProfilesAsync(string query, int page, int pageSize);
    }
}
