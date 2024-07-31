using MassTransit;
using System.Reflection;

namespace SearchService.MassTransit;
public static class Extentions
{
    public static IServiceCollection AddMessageBroker
        (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        services.AddMassTransit(config =>
        {
            if (assembly != null)
                config.AddConsumers(assembly);

            /*config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();*/

            config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.UseMessageRetry(r =>
                {
                    r.Handle<RabbitMqConnectionException>();
                    r.Interval(5, TimeSpan.FromSeconds(10));
                });
                configurator.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                {
                    host.Username(configuration["MessageBroker:UserName"]!);
                    host.Password(configuration["MessageBroker:Password"]!);
                });
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
