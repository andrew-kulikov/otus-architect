using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProGaudi.Tarantool.Client;
using SocialNetwork.Core.Entities;
using SocialNetwork.Infrastructure.Configuration;
using SocialNetwork.Infrastructure.Extensions;
using SocialNetwork.Infrastructure.MySQL;
using SocialNetwork.Infrastructure.Repositories;
using SocialNetwork.Infrastructure.Tarantool;

namespace SocialNetwork.TarantoolReplicator
{
    public class Program
    {
        private const string UserProfilesSpaceName = "user_profiles";
        private const string MetadataSpaceName = "meta";

        private const string UserProfilesPrimaryIndex = "primary";
        private const string MetadataPrimaryIndex = "primary";

        private static Box TarantoolClient;

        public static async Task Main(string[] args)
        {
            TarantoolClient = await Box.Connect("localhost", 3301);

            await CreateSchemaAsync();

            while (true)
            {
                var lastSavedId = await GetLastSavedIdAsync();
                var newProfiles = await LoadUserProfiles(lastSavedId, 50000);

                if (!newProfiles.Any()) break;

                var saveTasks = newProfiles.Batch(1000).Select(SaveProfilesInTrantoolAsync).ToList();
                await Task.WhenAll(saveTasks);

                Console.WriteLine($"Processed batch with last user {lastSavedId}");
            }

            TarantoolClient.Dispose();

            Console.WriteLine("End!");
        }

        private static async Task SaveProfilesInTrantoolAsync(IEnumerable<UserProfile> profiles)
        {
            var index = TarantoolClient.Schema[UserProfilesSpaceName][UserProfilesPrimaryIndex];

            Console.WriteLine($"Start processing batch {profiles.First().UserId}");

            foreach (var userProfile in profiles) await index.Insert(userProfile.ToTuple());

            var lastInsertedId = profiles.Select(p => p.UserId).Max();

            var metadataIndex = TarantoolClient.Schema[MetadataSpaceName][MetadataPrimaryIndex];

            await metadataIndex.Insert(new ValueTuple<long, string>(
                lastInsertedId,
                DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)));

            Console.WriteLine($"Saved user profile {lastInsertedId}");
        }

        private static async Task<long> GetLastSavedIdAsync()
        {
            var index = TarantoolClient.Schema[MetadataSpaceName][MetadataPrimaryIndex];

            var max = await index.Max<ValueTuple<long, string>>();

            return max.Item1;
        }

        private static SqlConnectionFactory BuildConnectionFactory()
        {
            var replicationGroup = new ReplicationGroupConnectionStrings
            {
                ConnectionStrings = new List<ReplicationGroupConnectionString>
                {
                    new ReplicationGroupConnectionString
                    {
                        ConnectionString = "Server=localhost;Port=3306;Database=SocialNetwork;Uid=root;Pwd=admin;",
                        Name = "Master",
                        Type = "Master"
                    }
                }
            };

            return new SqlConnectionFactory(new OptionsWrapper<ReplicationGroupConnectionStrings>(replicationGroup));
        }

        private static async Task<ICollection<UserProfile>> LoadUserProfiles(long startId, int count)
        {
            var connectionFactory = BuildConnectionFactory();
            var dbContext = new DbContext(connectionFactory);

            await using var unitOfWork = new UnitOfWork(dbContext);

            var repo = new UserProfileRepository(dbContext);

            return await repo.GetNewUserProfilesAsync(startId, count);
        }

        private static async Task CreateSchemaAsync()
        {
            await TarantoolClient.Eval<string>($"box.schema.create_space('{UserProfilesSpaceName}', {{if_not_exists = true}})");
            await TarantoolClient.Eval<string>($"box.schema.create_space('{MetadataSpaceName}', {{if_not_exists = true}})");

            var primaryParts = "{{field = 1, type = 'unsigned'}}";
            var metaPrimaryParts = "{{field = 1, type = 'unsigned'}}";
            var nameParts =
                "{{field = 2, type = 'string', collation = 'unicode_ci'}, {field = 3, type = 'string', collation = 'unicode_ci'}}";

            await TarantoolClient.Eval<string>(
                $"box.space.{UserProfilesSpaceName}:create_index('primary', {{unique = true, if_not_exists = true, parts = {primaryParts}}})");
            await TarantoolClient.Eval<string>(
                $"box.space.{UserProfilesSpaceName}:create_index('name', {{unique = false, if_not_exists = true, parts = {nameParts}}})");
            await TarantoolClient.Eval<string>(
                $"box.space.{MetadataSpaceName}:create_index('primary', {{unique = true, if_not_exists = true, parts = {metaPrimaryParts}}})");

            await TarantoolClient.Schema.Reload();
        }
    }
}