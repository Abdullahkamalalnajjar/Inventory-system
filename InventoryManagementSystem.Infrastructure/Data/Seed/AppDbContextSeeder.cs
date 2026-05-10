using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using InventoryManagementSystem.Domain.Identity;
using InventoryManagementSystem.Infrastructure.Identity;

namespace InventoryManagementSystem.Infrastructure.Data.Seed;

public sealed class AppDbContextSeeder(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<SeedDataOptions> options)
{
    private readonly SeedDataOptions _options = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        foreach (var role in Enum.GetNames<Role>())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (string.IsNullOrWhiteSpace(_options.AdminEmail) || string.IsNullOrWhiteSpace(_options.AdminPassword))
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(_options.AdminEmail);
        if (existingUser is not null)
        {
            return;
        }

        var adminUser = new AppUser
        {
            UserName = _options.AdminEmail,
            Email = _options.AdminEmail,
            EmailConfirmed = true,
            City = _options.AdminCity,
            PhoneNumber = _options.AdminPhoneNumber
        };

        var createResult = await userManager.CreateAsync(adminUser, _options.AdminPassword);
        if (!createResult.Succeeded)
        {
            return;
        }

        var roleName = string.IsNullOrWhiteSpace(_options.AdminRole)
            ? Role.Manager.ToString()
            : _options.AdminRole;

        await userManager.AddToRoleAsync(adminUser, roleName);
    }
}
