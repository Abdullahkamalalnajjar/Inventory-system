using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using InventoryManagementSystem.Application.Common.Interfaces;
using InventoryManagementSystem.Application.Common.Security;
using InventoryManagementSystem.Domain.Common.Results;
using InventoryManagementSystem.Domain.Identity;
using InventoryManagementSystem.Infrastructure.Data;
using InventoryManagementSystem.Infrastructure.Data.Interceptors;
using InventoryManagementSystem.Infrastructure.Data.Seed;
using InventoryManagementSystem.Infrastructure.Identity;
using InventoryManagementSystem.Infrastructure.Notifications;
using InventoryManagementSystem.Infrastructure.Payments;

namespace InventoryManagementSystem.Infrastructure;

public static class DependencyInjection
{
    private const string DeletedAccountErrorCode = "Account_Deleted";
    private const string DeletedAccountErrorDescription = "The user account has been deleted. Restore the account before signing in again.";
    private const string ForceLogoutHeaderName = "X-Force-Logout";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<IdentityClaimsFactory>();
        services.AddScoped<AppDbContextSeeder>();
        services.Configure<SeedDataOptions>(configuration.GetSection(SeedDataOptions.SectionName));
        services.Configure<PaymobOptions>(configuration.GetSection(PaymobOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            options
                .UseSqlServer(connectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is missing.");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is missing.");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is missing.");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            context.Fail("The access token does not contain a valid user identifier.");
                            return;
                        }

                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                        var user = await userManager.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == userId);

                        if (user is null || user.IsDeleted)
                        {
                            context.HttpContext.Items[nameof(DeletedAccountErrorCode)] = DeletedAccountErrorCode;
                            context.HttpContext.Items[nameof(DeletedAccountErrorDescription)] = DeletedAccountErrorDescription;
                            context.Fail("The user account is no longer active.");
                        }
                    },
                    OnChallenge = async context =>
                    {
                        if (!context.HttpContext.Items.TryGetValue(nameof(DeletedAccountErrorCode), out var errorCode) ||
                            errorCode is not string code ||
                            !context.HttpContext.Items.TryGetValue(nameof(DeletedAccountErrorDescription), out var errorDescription) ||
                            errorDescription is not string description)
                        {
                            return;
                        }

                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Headers[ForceLogoutHeaderName] = "true";

                        Result<object?> response = Error.Unauthorized(code, description);
                        await context.Response.WriteAsJsonAsync(response);
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(AuthorizationPolicies.CurrentUserRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Users.ReadSelf));

            options.AddPolicy(AuthorizationPolicies.UsersRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Users.Read));

            options.AddPolicy(AuthorizationPolicies.UsersWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Users.Write));

            options.AddPolicy(AuthorizationPolicies.PaymentsCheckout, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Payments.Checkout));

            options.AddPolicy(AuthorizationPolicies.CategoriesRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Categories.Read));

            options.AddPolicy(AuthorizationPolicies.CategoriesWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Categories.Write));

            options.AddPolicy(AuthorizationPolicies.UnitsRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Units.Read));

            options.AddPolicy(AuthorizationPolicies.UnitsWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Units.Write));

            options.AddPolicy(AuthorizationPolicies.ProductsRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Products.Read));

            options.AddPolicy(AuthorizationPolicies.ProductsWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Products.Write));

            options.AddPolicy(AuthorizationPolicies.WarehousesRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Warehouses.Read));

            options.AddPolicy(AuthorizationPolicies.WarehousesWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Warehouses.Write));

            options.AddPolicy(AuthorizationPolicies.StockRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Stock.Read));

            options.AddPolicy(AuthorizationPolicies.StockWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.Stock.Write));

            options.AddPolicy(AuthorizationPolicies.PurchaseInvoicesRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.PurchaseInvoices.Read));

            options.AddPolicy(AuthorizationPolicies.PurchaseInvoicesWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.PurchaseInvoices.Write));

            options.AddPolicy(AuthorizationPolicies.SalesInvoicesRead, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.SalesInvoices.Read));

            options.AddPolicy(AuthorizationPolicies.SalesInvoicesWrite, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(PermissionClaimTypes.Permission, Permissions.SalesInvoices.Write));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<INotificationService, SmtpNotificationService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IPaymobWebhookService, PaymobWebhookService>();
        services.AddHttpClient<IPaymentCheckoutService, PaymobCheckoutService>();

        return services;
    }
}
