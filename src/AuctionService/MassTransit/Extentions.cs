using AuctionService.Data;
using MassTransit;
using System.Reflection;

namespace AuctionService.MassTransit;
public static class Extentions
{
    public static IServiceCollection AddMessageBroker
        (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        services.AddMassTransit(config =>
        {
            if (assembly != null)
                config.AddConsumers(assembly);

            config.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(10);

                o.UsePostgres();
                o.UseBusOutbox();
            });

            config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

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
