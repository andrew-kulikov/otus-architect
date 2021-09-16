using System;
using System.Linq;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SocialNetwork.Infrastructure.Configuration;

namespace SocialNetwork.Infrastructure.MySQL
{
    public class SqlConnectionFactory<TSettings> where TSettings: class, IReplicationGroupConnectionStrings
    {
        private readonly IOptions<TSettings> _replicationGroupOptions;
        private readonly Random _random;
        private MySqlConnection _masterConnection;

        public SqlConnectionFactory(IOptions<TSettings> replicationGroupOptions)
        {
            _replicationGroupOptions = replicationGroupOptions;
            _random = new Random();
        }

        public MySqlConnection CreateMasterConnection() => 
            _masterConnection ??= new MySqlConnection(GetMasterConnectionString());

        public MySqlConnection CreateReadConnection() =>
            IsSingleNode()
                ? CreateMasterConnection() 
                : new MySqlConnection(GetRandomConnectionString());

        private bool IsSingleNode() =>
            _replicationGroupOptions.Value.ConnectionStrings.Count == 1;

        private string GetMasterConnectionString() => 
            _replicationGroupOptions.Value.ConnectionStrings.First(c => c.Type == "Master").ConnectionString;

        private string GetRandomConnectionString()
        {
            var randomConnectionId = _random.Next(_replicationGroupOptions.Value.ConnectionStrings.Count);

            return _replicationGroupOptions.Value.ConnectionStrings[randomConnectionId].ConnectionString;
        }
    }
}