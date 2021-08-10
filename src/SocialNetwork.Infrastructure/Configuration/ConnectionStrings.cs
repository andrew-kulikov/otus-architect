﻿using System.Collections.Generic;

namespace SocialNetwork.Infrastructure.Configuration
{
    public class ConnectionStrings
    {
        public string SocialNetworkDb { get; set; }
    }

    public class ReplicationGroupConnectionStrings
    {
        public List<ReplicationGroupConnectionString> ConnectionStrings { get; set; }
    }

    public class ReplicationGroupConnectionString
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string Type { get; set; }
    }
}