using Asp.Versioning;
using Microsoft.OpenApi.Models;
using InventoryManagementSystem.Api.Infrastructure;
using InventoryManagementSystem.Api.Services;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Features.Units;
using InventoryManagementSystem.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace InventoryManagementSystem.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors.Select(error =>
                        Error.Validation(
                            code: string.IsNullOrWhiteSpace(entry.Key) ? "Validation" : entry.Key,
                            description: error.ErrorMessage)))
                    .ToList();

                Result<object?> response = errors;
                return new BadRequestObjectResult(response);
            };
        });
        services.AddAppOutputCaching();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? [];

        string[] defaultAllowedOrigins =
        [
            "https://inentory-system-front.vercel.app",
            "https://inventory-system-front.vercel.app"
        ];

        var corsAllowedOrigins = allowedOrigins
            .Concat(defaultAllowedOrigins)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy("ApiCors", policy =>
            {
                policy
                    .SetIsOriginAllowed(origin =>
                    {
                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        {
                            return false;
                        }

                        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                               || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                               || corsAllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Inventory Management System API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter a valid JWT access token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddAppOutputCaching(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100 mb
            options.AddPolicy("Units", policy =>
                policy.Tag(UnitsCacheKeys.UnitsTag));
        });

        return services;
    }
}
