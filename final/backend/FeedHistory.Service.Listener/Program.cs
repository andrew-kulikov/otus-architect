using FeedHistory.Service.Listener.Cache;
using FeedHistory.Service.Listener.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FeedHistory.Service.Listener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddSingleton<IBarsRepository, BarsRepository>();
                    services.AddSingleton<IDbInitializer, MongoDbInitializer>();
                    services.AddSingleton<ICacheInitializer, CacheInitializer>();
                });
    }
}