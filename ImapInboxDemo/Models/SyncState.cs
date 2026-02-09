namespace ImapInboxDemo.Models
{
    public class SyncState
    {

        public int Id { get; set; }

        // Backfill progress
        public long LowestSyncedUid { get; set; }

        // Control flags
        public bool PauseBackfill { get; set; }

        // Metrics
        public int LastRunNewMessages { get; set; }
        public DateTime? LastRunTime { get; set; }
    }
}




