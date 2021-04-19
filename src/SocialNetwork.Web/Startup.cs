using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Data.Migrations;
using SocialNetwork.Infrastructure.Configuration;
using SocialNetwork.Infrastructure.MySQL;
using SocialNetwork.Infrastructure.Repositories;
using SocialNetwork.Web.Authentication;

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

            services.AddHttpContextAccessor();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Login/Logout";
                });

            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddMySql5()
                    .WithGlobalConnectionString(Configuration.GetConnectionString("SocialNetworkDb"))
                    .ScanIn(typeof(AddUserTable).Assembly).For.Migrations());

            services.AddOptions<ConnectionStrings>()
                .Bind(Configuration.GetSection("ConnectionStrings"));

            services.AddScoped<ISignInManager, SignInManager>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<SqlConnectionFactory>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}