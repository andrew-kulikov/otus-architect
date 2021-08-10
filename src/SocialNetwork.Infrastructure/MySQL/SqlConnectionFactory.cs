using System;
using System.Linq;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SocialNetwork.Infrastructure.Configuration;

namespace SocialNetwork.Infrastructure.MySQL
{
    public class SqlConnectionFactory
    {
        private readonly IOptions<ConnectionStrings> _options;
        private readonly IOptions<ReplicationGroupConnectionStrings> _replicationGroupOptions;
        private readonly Random _random;

        public SqlConnectionFactory(IOptions<ConnectionStrings> options, IOptions<ReplicationGroupConnectionStrings> replicationGroupOptions)
        {
            _options = options;
            _replicationGroupOptions = replicationGroupOptions;
            _random = new Random();
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_options.Value.SocialNetworkDb);
        }

        public MySqlConnection CreateMasterConnection()
        {
            var masterConnectionString = _replicationGroupOptions.Value.ConnectionStrings.First(c => c.Type == "Master");

            return new MySqlConnection(masterConnectionString.ConnectionString);
        }

        public MySqlConnection CreateReadConnection()
        {
            var randomConnectionId = _random.Next(_replicationGroupOptions.Value.ConnectionStrings.Count);
            var randomConnectionString = _replicationGroupOptions.Value.ConnectionStrings[randomConnectionId];

            return new MySqlConnection(randomConnectionString.ConnectionString);
        }
    }
}