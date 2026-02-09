using System;
using System.Numerics;

namespace ImapInboxDemo.Dtos
{
    public class SyncStatusDto
    {
        public int TotalSyncedMessages { get; set; }
        public DateTime? OldestSyncedDate { get; set; }
        public DateTime? NewestSyncedDate { get; set; }
        public long LowestSyncedUid { get; set; }

        // New fields
        public int TotalInboxMessages { get; set; }
        public double PercentComplete { get; set; }
        public int RemainingMessages { get; set; }

        // For chart
        public List<DateTime> SyncDates { get; set; } = new();
        public List<int> SyncCounts { get; set; } = new();

        public double SyncSpeedMsgsPerMin { get; set; }
        public string? BackfillEta { get; set; }
        public bool PauseBackfill { get; set; }
    }
}
