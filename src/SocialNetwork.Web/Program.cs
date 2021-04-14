using System;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SocialNetwork.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            MigrateDatabase(host.Services);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }

        private static void MigrateDatabase(IServiceProvider servicePriProvider)
        {
            using var scope = servicePriProvider.CreateScope();
            
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                
            runner.MigrateUp();
        }
    }
}