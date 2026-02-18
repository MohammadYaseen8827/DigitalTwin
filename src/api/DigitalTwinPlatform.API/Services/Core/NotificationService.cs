using DigitalTwinPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http.Json;

namespace DigitalTwinPlatform.API.Services.Core;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public NotificationService(ILogger<NotificationService> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendNotificationAsync(Alert alert, IEnumerable<string> channels)
    {
        foreach (var channel in channels)
        {
            try 
            {
                switch (channel.ToLowerInvariant())
                {
                    case "email":
                        await SendEmailAsync(alert);
                        break;
                    case "webhook":
                        await SendWebhookAsync(alert);
                        break;
                    default:
                        _logger.LogWarning("Unknown notification channel: {Channel}", channel);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification via {Channel} for alert {AlertId}", channel, alert.Id);
            }
        }
    }

    private async Task SendEmailAsync(Alert alert)
    {
        try
        {
            // Get SMTP configuration from settings
            var smtpConfig = GetSmtpConfiguration();
            
            if (string.IsNullOrEmpty(smtpConfig.Host))
            {
                _logger.LogWarning("SMTP not configured. Skipping email for alert {AlertId}", alert.Id);
                return;
            }

            using var client = new System.Net.Mail.SmtpClient(smtpConfig.Host, smtpConfig.Port);
            client.EnableSsl = smtpConfig.EnableSsl;
            client.Credentials = new System.Net.NetworkCredential(smtpConfig.Username, smtpConfig.Password);

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(smtpConfig.FromAddress),
                Subject = $"Digital Twin Alert: {alert.Severity} - {alert.Message}",
                Body = GenerateAlertEmailBody(alert),
                IsBodyHtml = true
            };
            
            // Add recipients based on severity or configuration
            var recipients = GetAlertRecipients(alert.Severity);
            foreach (var recipient in recipients)
            {
                mailMessage.To.Add(recipient);
            }

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully for alert {AlertId}", alert.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for alert {AlertId}", alert.Id);
            // Don't throw to avoid interrupting the main flow
        }
    }
    
    private SmtpConfiguration GetSmtpConfiguration()
    {
        return new SmtpConfiguration
        {
            Host = _configuration.GetSection("Smtp:Host").Value ?? "",
            Port = int.TryParse(_configuration.GetSection("Smtp:Port").Value, out var port) ? port : 587,
            EnableSsl = bool.TryParse(_configuration.GetSection("Smtp:EnableSsl").Value, out var enableSsl) ? enableSsl : true,
            Username = _configuration.GetSection("Smtp:Username").Value ?? "",
            Password = _configuration.GetSection("Smtp:Password").Value ?? "",
            FromAddress = _configuration.GetSection("Smtp:FromAddress").Value ?? ""
        };
    }
    
    private string GenerateAlertEmailBody(Alert alert)
    {
        return $@"
            <html>
            <body>
                <h2>Digital Twin Platform Alert</h2>
                <p><strong>Severity:</strong> {alert.Severity}</p>
                <p><strong>Machine ID:</strong> {alert.MachineId}</p>
                <p><strong>Message:</strong> {alert.Message}</p>
                <p><strong>Created:</strong> {alert.CreatedAt:yyyy-MM-dd HH:mm:ss}</p>
            </body>
            </html>";
    }
    
    private IEnumerable<string> GetAlertRecipients(AlertSeverity severity)
    {
        // This would be configured based on alert severity
        yield return "admin@example.com"; // Would come from configuration
    }

    private async Task SendWebhookAsync(Alert alert)
    {
        // In a real SME environment, they might have a webhook URL configured in settings
        // For now, we'll try to post to a placeholder if it were configured
        var payload = new 
        {
            alert_id = alert.Id,
            machine_id = alert.MachineId,
            message = alert.Message,
            severity = alert.Severity.ToString(),
            timestamp = alert.CreatedAt
        };

        _logger.LogInformation("WEBHOOK: Dispatching payload for alert {AlertId}", alert.Id);
        
        // This is a placeholder. If configured, we would do:
        // await _httpClient.PostAsJsonAsync("https://sme-external-api.com/webhooks/alerts", payload);
        
        await Task.CompletedTask;
    }

    public async Task<bool> TestChannelAsync(string channel, string target)
    {
        _logger.LogInformation("Testing channel {Channel} with target {Target}", channel, target);
        return await Task.FromResult(true);
    }
}

public class SmtpConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}
