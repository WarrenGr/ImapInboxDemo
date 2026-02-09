using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ImapInboxDemo.Data;
using ImapInboxDemo.Models;

namespace ImapInboxDemo.Pages
{
    public class SyncStatusModel : PageModel
    {
        private readonly AppDbContext _db;

        public SyncStatusModel(AppDbContext db)
        {
            _db = db;
        }

        // UI properties
        public long LowestSyncedUid { get; set; }
        public int LastRunNewMessages { get; set; }
        public DateTime? LastRunTime { get; set; }
        public bool PauseBackfill { get; set; }

        public async Task OnGetAsync()
        {
            var state = await _db.SyncState.FirstOrDefaultAsync();

            if (state != null)
            {
                LowestSyncedUid = state.LowestSyncedUid;
                LastRunNewMessages = state.LastRunNewMessages;
                LastRunTime = state.LastRunTime;
                PauseBackfill = state.PauseBackfill;
            }
        }
    }
}