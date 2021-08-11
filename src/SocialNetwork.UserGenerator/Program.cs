using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Options;
using MoreLinq;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Infrastructure.Configuration;
using SocialNetwork.Infrastructure.MySQL;
using SocialNetwork.Infrastructure.Repositories;

namespace SocialNetwork.UserGenerator
{
    public class MockSignInManager : ISignInManager
    {
        public Task SignInAsync(User user)
        {
            return Task.CompletedTask;
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var replicationGroup = new ReplicationGroupConnectionStrings
            {
                ConnectionStrings = new List<ReplicationGroupConnectionString>
                {
                    new ReplicationGroupConnectionString
                    {
                        ConnectionString = "Server=127.0.0.1;Database=SocialNetwork;Uid=root;Pwd=admin;",
                        Name = "Master",
                        Type = "Master"
                    }
                }
            };

            var connectionFactory = new SqlConnectionFactory(new OptionsWrapper<ReplicationGroupConnectionStrings>(replicationGroup));

            var fakeProfile = new Faker<UserProfile>()
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.Age, (f, u) => f.Random.Int(12, 95))
                .RuleFor(u => u.City, (f, u) => f.Address.City())
                .RuleFor(u => u.Interests, (f, u) => f.Lorem.Sentence());

            var fakeUser = new Faker<User>()
                .CustomInstantiator(f => new User {Id = f.IndexFaker + 1})
                .RuleFor(u => u.Profile, (f, u) =>
                {
                    var profile = fakeProfile.Generate();
                    profile.UserId = u.Id;
                    return profile;
                })
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.Profile.FirstName, u.Profile.LastName))
                .RuleFor(u => u.PasswordHash, (f, u) => f.Internet.Password())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Profile.FirstName, u.Profile.LastName))
                .RuleFor(u => u.RegisteredAt, (f, u) => f.Date.Past());

            var users = fakeUser.Generate(1000000);

            Parallel.ForEach(
                users.Batch(500),
                async usersBatch => await RegisterUsersBatchAsync(usersBatch.ToList(), connectionFactory));

            Console.WriteLine("Start users relink...");
            users = await RelinkUsers(users, connectionFactory);
            Console.WriteLine("End users relink...");

            Parallel.ForEach(
                users.Batch(500),
                async usersBatch => await RegisterProfilesBatchAsync(usersBatch.ToList(), connectionFactory));
        }

        private static async Task<List<User>> RelinkUsers(List<User> users, SqlConnectionFactory connectionFactory)
        {
            var dbContext = new DbContext(connectionFactory);
            var userRepository = new UserRepository(dbContext);

            var savedUsers = await userRepository.GetAllUsersAsync();
            var generatedUsersByUsername = users.ToDictionary(u => (u.Username, u.Email, u.PasswordHash), u => u);

            return savedUsers.Select(user =>
            {
                var generatedUser = generatedUsersByUsername[(user.Username, user.Email, user.PasswordHash)];
                user.Profile = generatedUser.Profile;
                user.Profile.UserId = user.Id;
                if (user.Id % 1000 == 0) Console.WriteLine($"Relinking user {user.Id}");
                return user;
            }).ToList();
        }

        public static async Task RegisterUsersBatchAsync(List<User> usersBatch, SqlConnectionFactory connectionFactory)
        {
            var dbContext = new DbContext(connectionFactory);
            var unitOfWork = new UnitOfWork(dbContext);
            var userRepository = new UserRepository(dbContext);

            var sw = new Stopwatch();
            sw.Start();

            foreach (var user in usersBatch) await userRepository.AddUserAsync(user);
            
            Console.WriteLine($"Commiting users {usersBatch.First().Id}...");

            await CommitWithRetriesAsync(unitOfWork);

            sw.Stop();
            Console.WriteLine($"Batch {usersBatch.First().Id}. Elapsed: {sw.Elapsed}");
        }

        public static async Task RegisterProfilesBatchAsync(List<User> usersBatch, SqlConnectionFactory connectionFactory)
        {
            var dbContext = new DbContext(connectionFactory);
            var unitOfWork = new UnitOfWork(dbContext);
            var userProfileRepository = new UserProfileRepository(dbContext);

            var sw = new Stopwatch();
            sw.Start();

            foreach (var user in usersBatch) await userProfileRepository.AddUserProfileAsync(user, user.Profile);

            Console.WriteLine($"Commiting profiles {usersBatch.First().Id}...");

            await CommitWithRetriesAsync(unitOfWork);

            sw.Stop();
            Console.WriteLine($"Batch {usersBatch.First().Id}. Elapsed: {sw.Elapsed}");
        }

        private static async Task CommitWithRetriesAsync(UnitOfWork unitOfWork, int retryCount = 3)
        {
            try
            {
                await unitOfWork.CommitAsync();
                await unitOfWork.DisposeAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Retries: {retryCount}, Error: {e}");
                if (retryCount <= 0) throw;

                await Task.Delay(1000);
                await CommitWithRetriesAsync(unitOfWork, retryCount);
            }
        }
    }
}