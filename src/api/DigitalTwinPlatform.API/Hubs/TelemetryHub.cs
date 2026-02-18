using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Hubs;

/// <summary>
/// SignalR hub for real-time telemetry streaming.
/// Provides methods for subscribing to machine telemetry updates.
/// </summary>
public class TelemetryHub : Hub<ITelemetryClient>
{
    private readonly ILogger<TelemetryHub> _logger;
    private static readonly ConcurrentDictionary<string, HashSet<string>> _machineSubscriptions = new();
    private static readonly ConcurrentDictionary<string, string> _connectionToMachine = new();

    public TelemetryHub(ILogger<TelemetryHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the user ID from the connection context.
    /// </summary>
    private string? GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? Context.User?.FindFirst("sub")?.Value;

    /// <summary>
    /// Subscribe to telemetry updates for a specific machine.
    /// </summary>
    /// <param name="machineId">The machine ID to subscribe to.</param>
    public async Task SubscribeToMachine(string machineId)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} subscribing to machine {MachineId} (Connection: {ConnectionId})",
            userId ?? "anonymous", machineId, connectionId);

        // Add connection to machine-specific group
        await Groups.AddToGroupAsync(connectionId, $"telemetry-{machineId}");

        // Track subscription for cleanup
        _machineSubscriptions.AddOrUpdate(
            machineId,
            new HashSet<string> { connectionId },
            (_, existing) => { existing.Add(connectionId); return existing; }
        );

        _connectionToMachine.AddOrUpdate(connectionId, machineId, (_, _) => machineId);

        // Confirm subscription to client
        await Clients.Caller.TelemetryUpdate(new TelemetryData
        {
            MachineId = machineId,
            Timestamp = DateTime.UtcNow,
            Message = $"Successfully subscribed to telemetry for machine {machineId}"
        });

        _logger.LogDebug("Client {ConnectionId} subscribed to machine {MachineId}", connectionId, machineId);
    }

    /// <summary>
    /// Unsubscribe from telemetry updates for a specific machine.
    /// </summary>
    /// <param name="machineId">The machine ID to unsubscribe from.</param>
    public async Task UnsubscribeFromMachine(string machineId)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} unsubscribing from machine {MachineId} (Connection: {ConnectionId})",
            userId ?? "anonymous", machineId, connectionId);

        // Remove connection from machine-specific group
        await Groups.RemoveFromGroupAsync(connectionId, $"telemetry-{machineId}");

        // Clean up subscription tracking
        if (_machineSubscriptions.TryGetValue(machineId, out var subscriptions))
        {
            lock (subscriptions)
            {
                subscriptions.Remove(connectionId);
                if (subscriptions.Count == 0)
                {
                    _machineSubscriptions.TryRemove(machineId, out _);
                }
            }
        }

        _connectionToMachine.TryRemove(connectionId, out _);

        // Confirm unsubscription to client
        await Clients.Caller.TelemetryUpdate(new TelemetryData
        {
            MachineId = machineId,
            Timestamp = DateTime.UtcNow,
            Message = $"Successfully unsubscribed from telemetry for machine {machineId}"
        });

        _logger.LogDebug("Client {ConnectionId} unsubscribed from machine {MachineId}", connectionId, machineId);
    }

    /// <summary>
    /// Send telemetry data to the server (for testing/demo purposes).
    /// </summary>
    /// <param name="telemetry">The telemetry data to broadcast.</param>
    public async Task SendTelemetry(TelemetryData telemetry)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogDebug("Telemetry received from {ConnectionId}: {MachineId}", connectionId, telemetry.MachineId);

        // Broadcast to all subscribers of this machine
        await Clients.Group($"telemetry-{telemetry.MachineId}").TelemetryUpdate(telemetry);
    }

    /// <summary>
    /// Subscribe to all machines the user has access to.
    /// </summary>
    public async Task SubscribeToAllMachines()
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} subscribing to all machine telemetry", userId ?? "anonymous");
        await Clients.Caller.TelemetryUpdate(new TelemetryData
        {
            MachineId = "all",
            Timestamp = DateTime.UtcNow,
            Message = "Subscribed to all machine telemetry (use SubscribeToMachine for specific machines)"
        });
    }

    /// <summary>
    /// Handle client connection.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation("Client connected: {ConnectionId} (User: {UserId})",
            Context.ConnectionId, userId ?? "anonymous");
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle client disconnection with proper cleanup.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        if (exception != null)
        {
            _logger.LogWarning(exception, "Client {ConnectionId} disconnected with error (User: {UserId})",
                connectionId, userId ?? "anonymous");
        }
        else
        {
            _logger.LogInformation("Client disconnected: {ConnectionId} (User: {UserId})",
                connectionId, userId ?? "anonymous");
        }

        // Clean up subscription tracking
        if (_connectionToMachine.TryRemove(connectionId, out var machineId))
        {
            if (_machineSubscriptions.TryGetValue(machineId, out var subscriptions))
            {
                lock (subscriptions)
                {
                    subscriptions.Remove(connectionId);
                    if (subscriptions.Count == 0)
                    {
                        _machineSubscriptions.TryRemove(machineId, out _);
                    }
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Client interface for TelemetryHub methods.
/// </summary>
public interface ITelemetryClient
{
    /// <summary>
    /// Receive telemetry update from the server.
    /// </summary>
    Task TelemetryUpdate(TelemetryData data);
}

/// <summary>
/// Telemetry data model for SignalR communication.
/// </summary>
public class TelemetryData
{
    public string MachineId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double? Temperature { get; set; }
    public double? Pressure { get; set; }
    public double? Vibration { get; set; }
    public double? RulPrediction { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, double>? Metrics { get; set; }
}
