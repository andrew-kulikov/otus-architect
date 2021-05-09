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
            var connectionStrings = new ConnectionStrings
            {
                SocialNetworkDb = "Server=127.0.0.1;Database=SocialNetwork;Uid=zukk;Pwd=zukk;"
            };

            var connectionFactory = new SqlConnectionFactory(new OptionsWrapper<ConnectionStrings>(connectionStrings));
            var dbContext = new DbContext(connectionFactory);
            var unitOfWork = new UnitOfWork(dbContext);
            var userRepository = new UserRepository(dbContext);
            var userProfileRepository = new UserProfileRepository(dbContext);
            var authenticationService = new AuthenticationService(userRepository, new MockSignInManager(), unitOfWork);

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

            var sw = new Stopwatch();
            sw.Start();

            var users = fakeUser.Generate(1_000_000);

            sw.Stop();
            Console.WriteLine($"Generation took: {sw.Elapsed}");

            sw.Restart();

            var tasks = users.Batch(500)
                .Select(async usersBatch => await RegisterBatchAsync(usersBatch.ToList(), userProfileRepository, authenticationService, unitOfWork));

            await Task.WhenAll(tasks);

            dbContext.Dispose();

            sw.Stop();
            Console.WriteLine($"Elapsed: {sw.Elapsed}");
        }

        public static async Task RegisterBatchAsync(
            List<User> usersBatch,
            UserProfileRepository userProfileRepository,
            AuthenticationService authenticationService,
            IUnitOfWork unitOfWork)
        {
            var sw = new Stopwatch();
            sw.Start();

            foreach (var user in usersBatch)
            {
                if (user.Id % 1000 == 0)
                {
                    Console.WriteLine($"Processing user #{user.Id}");
                }
                
                await authenticationService.RegisterAsync(user, user.PasswordHash);
            }

            Console.WriteLine("Beginning transaction...");

            await unitOfWork.CommitAsync();

            Console.WriteLine($"Users committed {usersBatch.First().Id}. Elapsed: {sw.Elapsed}");

            foreach (var user in usersBatch)
            {
                if (user.Id % 1000 == 0)
                {
                    Console.WriteLine($"Processing UserProfile #{user.Id}");
                }

                await userProfileRepository.AddUserProfileAsync(user, user.Profile);
            }

            Console.WriteLine("Beginning transaction...");

            await unitOfWork.CommitAsync();

            sw.Stop();
            Console.WriteLine($"Batch {usersBatch.First().Id}. Elapsed: {sw.Elapsed}");
        }
    }
}