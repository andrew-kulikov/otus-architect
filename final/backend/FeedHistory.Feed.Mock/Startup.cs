using System;
using FeedHistory.Feed.Mock.HostedServices;
using FeedHistory.Feed.Mock.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace FeedHistory.Feed.Mock
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "FeedHistory.Feed.Mock", Version = "v1"}); });

            services.AddSignalR().AddJsonProtocol();

            services.AddHostedService<TickSenderService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FeedHistory.Feed.Mock v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<FeedHub>("feedHub");
            });
        }
    }
}