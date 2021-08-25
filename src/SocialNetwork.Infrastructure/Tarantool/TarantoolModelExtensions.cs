using System;
using ProGaudi.Tarantool.Client.Model;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Infrastructure.Tarantool
{
    public static class TarantoolModelExtensions
    {
        public static ValueTuple<long, string, string, int, string, string> ToTuple(this UserProfile profile) =>
            (profile.UserId, profile.FirstName, profile.LastName, profile.Age, profile.Interests, profile.City);

        public static UserProfile ToProfile(this ValueTuple<long, string, string, int, string, string> profileTuple) =>
            new UserProfile
            {
                UserId = profileTuple.Item1,
                FirstName = profileTuple.Item2,
                LastName = profileTuple.Item3,
                Age = profileTuple.Item4,
                Interests = profileTuple.Item5,
                City = profileTuple.Item6
            };

        public static UserProfile ToProfile(this TarantoolTuple<long, string, string, int, string, string> profileTuple) =>
            new UserProfile
            {
                UserId = profileTuple.Item1,
                FirstName = profileTuple.Item2,
                LastName = profileTuple.Item3,
                Age = profileTuple.Item4,
                Interests = profileTuple.Item5,
                City = profileTuple.Item6
            };
    }
}