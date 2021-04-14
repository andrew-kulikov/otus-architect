using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SocialNetwork.Infrastructure.Configuration;

namespace SocialNetwork.Infrastructure.MySQL
{
    public class SqlConnectionFactory
    {
        private readonly IOptions<ConnectionStrings> _options;

        public SqlConnectionFactory(IOptions<ConnectionStrings> options)
        {
            _options = options;
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_options.Value.SocialNetworkDb);
        }
    }
}