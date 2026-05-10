using System.Security.Claims;
using InventoryManagementSystem.Application.Common.Security;

namespace InventoryManagementSystem.Infrastructure.Identity;

public sealed class IdentityClaimsFactory
{
    public List<Claim> Create(IList<string> roles, IEnumerable<Claim> storedClaims)
    {
        var effectiveClaims = new List<Claim>();

        AddMissingClaims(effectiveClaims, storedClaims);

        foreach (var permission in roles
                     .SelectMany(Permissions.GetForRole)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            AddMissingClaim(effectiveClaims, new Claim(PermissionClaimTypes.Permission, permission));
        }

        return effectiveClaims;
    }

    public void AddMissingClaims(ClaimsIdentity identity, IEnumerable<Claim> claims)
    {
        foreach (var claim in claims)
        {
            if (!identity.Claims.Any(existing =>
                    existing.Type == claim.Type &&
                    string.Equals(existing.Value, claim.Value, StringComparison.OrdinalIgnoreCase)))
            {
                identity.AddClaim(claim);
            }
        }
    }

    private static void AddMissingClaims(List<Claim> targetClaims, IEnumerable<Claim> claims)
    {
        foreach (var claim in claims)
        {
            AddMissingClaim(targetClaims, claim);
        }
    }

    private static void AddMissingClaim(List<Claim> targetClaims, Claim claim)
    {
        if (targetClaims.Any(existing =>
                existing.Type == claim.Type &&
                string.Equals(existing.Value, claim.Value, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        targetClaims.Add(claim);
    }
}
