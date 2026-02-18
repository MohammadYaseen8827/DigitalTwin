using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.API.Services.Core;

/// <summary>
/// Service for dispatching notifications via various channels (Email, Webhook, SMS).
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to the specified channels.
    /// </summary>
    /// <param name="alert">The alert that triggered the notification.</param>
    /// <param name="channels">List of channels (e.g., "email", "webhook").</param>
    Task SendNotificationAsync(Alert alert, IEnumerable<string> channels);

    /// <summary>
    /// Tests the connectivity of a specific channel.
    /// </summary>
    Task<bool> TestChannelAsync(string channel, string target);
}
