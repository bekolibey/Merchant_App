using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace eAppointmentServer.application;

public static class dependencyinjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(dependencyinjection).Assembly);
        });
        return services;
    }
}