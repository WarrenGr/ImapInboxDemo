using Microsoft.AspNetCore.SignalR;

namespace ImapInboxDemo.Hubs
{
    /// <summary>
    /// SignalR hub used to broadcast real-time IMAP sync telemetry
    /// to any connected dashboard clients.
    /// </summary>
    public class ImapSyncHub : Hub
    {
        // No server-invoked methods needed yet.
        // Telemetry service will call Clients.All.SendAsync(...)
    }
}