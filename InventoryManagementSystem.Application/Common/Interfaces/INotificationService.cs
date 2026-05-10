namespace InventoryManagementSystem.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = false,
        CancellationToken cancellationToken = default);

    Task SendSmsAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
