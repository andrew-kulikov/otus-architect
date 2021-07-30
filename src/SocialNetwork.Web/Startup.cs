using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Core.Utils;
using SocialNetwork.Infrastructure.Caching;
using SocialNetwork.Infrastructure.Configuration;
using SocialNetwork.Infrastructure.Consumers;
using SocialNetwork.Infrastructure.MySQL;
using SocialNetwork.Infrastructure.Repositories;
using SocialNetwork.Infrastructure.Services;
using SocialNetwork.Web.Authentication;
using SocialNetwork.Web.Extensions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.MsgPack;

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
            services.AddRabbitMq(Configuration);

            services.AddStackExchangeRedisExtensions<MsgPackObjectSerializer>(new RedisConfiguration
            {
                ConnectionString = Configuration.GetValue<string>("Redis:ConnectionString")
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"{Configuration.GetValue<string>("Redis:Server")}:{Configuration.GetValue<int>("Redis:Port")}";
            });

            services.AddControllersWithViews();

            services.AddHttpContextAccessor();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Login/Logout";
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = PrincipalValidator.ValidateAsync
                    };
                });

            services.AddOptions<ConnectionStrings>().Bind(Configuration.GetSection("ConnectionStrings"));

            services.AddAutoMapper(typeof(Startup).Assembly);

            services.AddScoped<ISignInManager, SignInManager>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserContext, UserContext>();

            services.AddScoped<DbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<SqlConnectionFactory>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IUserPostRepository, UserPostRepository>();

            services.AddScoped<IFriendshipService, FriendshipService>();
            services.AddScoped<IUserPostService, UserPostService>();

            services.AddScoped(typeof(IListCache<>), typeof(RedisListCache<>));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot")),
                RequestPath = new PathString("/wwwroot")
            });

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