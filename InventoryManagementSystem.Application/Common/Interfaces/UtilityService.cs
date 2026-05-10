namespace InventoryManagementSystem.Application.Common.Interfaces;

public static class UtilityService
{
    public static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
        {
            return $"****{email.AsSpan(Math.Max(atIndex, 0))}";
        }

        return email[0] + "****" + email[atIndex - 1] + email[atIndex..];
    }
}
