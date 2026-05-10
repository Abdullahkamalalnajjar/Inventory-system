using InventoryManagementSystem.Domain.Identity;

namespace InventoryManagementSystem.Application.Common.Security;

public static class Permissions
{
    public static class Users
    {
        public const string ReadSelf = "users:self:read";
        public const string Read = "users:read";
        public const string Write = "users:write";
    }

    public static class Payments
    {
        public const string Checkout = "payments:checkout";
    }

    public static IReadOnlyCollection<string> GetForRole(string role)
    {
        if (string.Equals(role, Role.Manager.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                Users.ReadSelf,
                Users.Read,
                Users.Write,
                Payments.Checkout
            ];
        }

        if (string.Equals(role, Role.Member.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                Users.ReadSelf,
                Payments.Checkout
            ];
        }

        return [];
    }
}
