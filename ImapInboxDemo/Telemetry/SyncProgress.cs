using System;

namespace ImapInboxDemo.Telemetry
{
    /// <summary>
    /// Represents real-time sync telemetry sent to both the database
    /// and SignalR clients. This powers the live dashboard.
    /// </summary>
    public class SyncProgress
    {
        /// <summary>
        /// Current phase of the sync:
        /// "initial", "forward", or "backfill".
        ///<summary>
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