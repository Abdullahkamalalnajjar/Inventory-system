using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InventoryManagementSystem.Application.Common.Interfaces;

namespace InventoryManagementSystem.Infrastructure.Notifications;

public sealed class SmtpNotificationService(
    IOptions<SmtpOptions> options,
    ILogger<SmtpNotificationService> logger) : INotificationService
{
    private readonly SmtpOptions smtpOptions = options.Value;

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        if (!smtpOptions.Enabled)
        {
            logger.LogInformation("Skipping email to {Recipient} because SMTP delivery is disabled.", to);
            return;
        }

        ValidateConfiguration();

        using var message = new MailMessage
        {
            From = new MailAddress(smtpOptions.FromEmail, smtpOptions.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };

        message.To.Add(to);

        using var client = new SmtpClient(smtpOptions.Host, smtpOptions.Port)
        {
            EnableSsl = smtpOptions.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(smtpOptions.UserName))
        {
            client.Credentials = new NetworkCredential(smtpOptions.UserName, smtpOptions.Password);
        }
        else
        {
            client.UseDefaultCredentials = true;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message);

        logger.LogInformation("Sent email to {Recipient} with subject {Subject}.", to, subject);
    }

    public Task SendSmsAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Skipping SMS to {PhoneNumber} because SMS delivery is not implemented.", phoneNumber);
        return Task.CompletedTask;
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(smtpOptions.Host))
        {
            throw new InvalidOperationException("Smtp:Host is required when SMTP delivery is enabled.");
        }

        if (string.IsNullOrWhiteSpace(smtpOptions.FromEmail))
        {
            throw new InvalidOperationException("Smtp:FromEmail is required when SMTP delivery is enabled.");
        }

        if (smtpOptions.Port <= 0)
        {
            throw new InvalidOperationException("Smtp:Port must be greater than zero when SMTP delivery is enabled.");
        }
    }
}
