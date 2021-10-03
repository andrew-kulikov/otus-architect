using System;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.Core.Messages;
using SocialNetwork.Infrastructure.Consumers;
using SocialNetwork.Infrastructure.Publishers;

namespace SocialNetwork.Web.Extensions
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        // TODO: Replace with options pattern
        public static RabbitMqOptions FromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection(SectionName);

            return new RabbitMqOptions
            {
                Uri = section.GetValue<string>(nameof(Uri)),
                Username = section.GetValue<string>(nameof(Username)),
                Password = section.GetValue<string>(nameof(Password))
            };
        }

        public string Uri { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
    }

    public static class DependencyInjection
    {
        public static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(massTransitOptions =>
            {
                massTransitOptions.AddConsumer<PostCreatedConsumer>();
                massTransitOptions.AddConsumer<FeedUpdateConsumer>();
                massTransitOptions.AddConsumer<PushFeedUpdateConsumer>();

                massTransitOptions.UsingRabbitMq((provider, rabbitMqOptions) =>
                {
                    var connectionInfo = RabbitMqOptions.FromConfiguration(configuration);

                    rabbitMqOptions.Host(new Uri(connectionInfo.Uri), h =>
                    {
                        h.Username(connectionInfo.Username);
                        h.Password(connectionInfo.Password);
                    });

                    rabbitMqOptions.ConfigureEndpoints(provider);
                });
            });

            services.AddMassTransitHostedService();

            services.AddTransient<IMessagePublisher<PostCreatedMessage>, PostCreatedPublisher>();
        }
    }
}