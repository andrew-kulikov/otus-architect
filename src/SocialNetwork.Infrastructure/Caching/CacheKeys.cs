namespace SocialNetwork.Infrastructure.Caching
{
    public static class CacheKeys
    {
        public static class Feed
        {
            private const string FeedKey = "feed";
            public static string ForUser(long userid) => $"{FeedKey}-{userid}";
        }
    }
}