namespace ImapInboxDemo.Dtos
{
    public class BackfillHealthDto
    {
        public int TotalInboxMessages { get; set; }
        public int TotalSyncedMessages { get; set; }
        public int RemainingMessages { get; set; }

        public uint MinUid { get; set; }
        public uint MaxUid { get; set; }
        public long LowestSyncedUid { get; set; }

        public bool PauseBackfill { get; set; }
        public bool HasUidGaps { get; set; }
        public bool BackfillComplete { get; set; }
        public bool WatermarkInvalid { get; set; }
        public bool Stalled { get; set; }
        public string? StalledReason { get; set; }

        public int LastRunNewMessages { get; set; }
        public DateTime? LastRunTime { get; set; }

        public static BackfillHealthDto Error(string message)
        {
            return new BackfillHealthDto
            {
                Stalled = true,
                StalledReason = message
            };
        }
    }
}
