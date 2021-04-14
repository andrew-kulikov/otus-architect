using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Data.Migrations;
using SocialNetwork.Infrastructure.Configuration;
using SocialNetwork.Infrastructure.MySQL;
using SocialNetwork.Infrastructure.Repositories;

namespace SocialNetwork.Web
{
    public class Startup
    {
        protected readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddMySql5()
                    .WithGlobalConnectionString(Configuration.GetConnectionString("SocialNetworkDb"))
                    .ScanIn(typeof(AddUserTable).Assembly).For.Migrations());

            services.AddOptions<ConnectionStrings>()
                .Bind(Configuration.GetSection("ConnectionStrings"));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<SqlConnectionFactory>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}