using AutoMapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Profiles
{
    public class SocialNetworkProfile : Profile
    {
        public SocialNetworkProfile()
        {
            CreateMap<UserProfile, UserProfileViewModel>();
        }
    }
}