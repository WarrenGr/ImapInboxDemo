using System;
using System.Threading;
using System.Threading.Tasks;
using ImapInboxDemo.Data;
using ImapInboxDemo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ImapInboxDemo.Telemetry
{
    /// <summary>
    /// Central service responsible for updating the SyncDashboard table
    /// and broadcasting real-time telemetry to SignalR clients.
    /// </summary>
    public class SyncTelemetryService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<ImapSyncHub> _hub;

        public SyncTelemetryService(AppDbContext db, IHubContext<ImapSyncHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        /// <summary>
        /// Updates both the database and SignalR clients with the latest
        /// sync progress snapshot.
        /// </summary>
        public async Task UpdateAsync(SyncProgress progress, CancellationToken token)
        {
            // Ensure dashboard row exists
            var dashboard = await _db.SyncDashboard.FirstOrDefaultAsync(token);
            if (dashboard == null)
            {
                dashboard = new SyncDashboard();
                _db.SyncDashboard.Add(dashboard);
            }

            // Update DB fields
            dashboard.Phase = progress.Phase;
            dashboard.TotalMessages = progress.TotalMessages;
            dashboard.Processed = progress.Processed;
            dashboard.Inserted = progress.Inserted;
            dashboard.CurrentUid = progress.CurrentUid;
            dashboard.MessagesPerSecond = progress.MessagesPerSecond;
            dashboard.EstimatedSecondsRemaining = progress.EstimatedSecondsRemaining;
            dashboard.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync(token);

            // Broadcast to SignalR clients
            await _hub.Clients.All.SendAsync("syncProgress", new
            {
                phase = progress.Phase,
                totalMessages = progress.TotalMessages,
                processed = progress.Processed,
                inserted = progress.Inserted,
                currentUid = progress.CurrentUid,
                messagesPerSecond = progress.MessagesPerSecond,
                etaSeconds = progress.EstimatedSecondsRemaining,
                lastUpdated = progress.LastUpdated
            }, token);
        }
    }
}