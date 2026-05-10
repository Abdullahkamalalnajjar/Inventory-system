namespace InventoryManagementSystem.Application.Common.Security;

public static class AuthorizationPolicies
{
    public const string AuthenticatedUser = "auth.user";
    public const string CurrentUserRead = "current-user.read";
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string PaymentsCheckout = "payments.checkout";
}
