using System;

namespace ImapInboxDemo.Telemetry
{
    /// <summary>
    /// Database entity that stores the most recent real-time
    /// IMAP sync telemetry snapshot. This powers the dashboard
    /// and persists the latest sync state across restarts.
    /// </summary>
    public class SyncDashboard
    {
        public int Id { get; set; }

        /// <summary>
        /// Current phase of the sync:
        /// "initial", "forward", or "backfill".
        /// </summary>
        public string Phase { get; set; } = string.Empty;

        /// <summary>
        /// Total number of messages expected to be processed
        /// in the current phase.
        /// </summary>
        public int TotalMessages { get; set; }

        /// <summary>
        /// Number of messages processed so far (including skipped).
        /// </summary>
        public int Processed { get; set; }

        /// <summary>
        /// Number of new messages inserted into the database.
        /// </summary>
        public int Inserted { get; set; }

        /// <summary>
        /// The UID of the message currently being processed.
        /// </summary>
        public long CurrentUid { get; set; }

        /// <summary>
        /// Messages processed per second (throughput).
        /// </summary>
        public double MessagesPerSecond { get; set; }

        /// <summary>
        /// Estimated seconds remaining in the current phase.
        /// </summary>
        public double EstimatedSecondsRemaining { get; set; }

        /// <summary>
        /// Timestamp of the last telemetry update.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}