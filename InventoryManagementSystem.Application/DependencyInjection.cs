using Microsoft.Extensions.DependencyInjection;
using MediatR;
using InventoryManagementSystem.Application.Common.Behaviours;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;

namespace InventoryManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        return services;
    }
}
