using System;
using System.Collections.Generic;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Web.ViewModels
{
    public class UserProfilesViewModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public ICollection<UserProfile> Profiles { get; set; }
    }

    public class UserProfilesSearchViewModel
    {
        public string Query { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public ICollection<UserProfile> Profiles { get; set; }
    }
}