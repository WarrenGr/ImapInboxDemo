using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ImapInboxDemo.Data;

namespace ImapInboxDemo.Pages
{
    public class BackfillHealthModel : PageModel
    {
        private readonly AppDbContext _db;

        public BackfillHealthModel(AppDbContext db)
        {
            _db = db;
        }

        // UI properties
        public long LowestSyncedUid { get; set; }
        public int TotalMessages { get; set; }
        public int SyncedMessages { get; set; }
        public int RemainingMessages => TotalMessages - SyncedMessages;

        public async Task OnGetAsync()
        {
            var state = await _db.SyncState.FirstOrDefaultAsync();
            if (state != null)
                LowestSyncedUid = state.LowestSyncedUid;

            TotalMessages = await _db.EmailMessages.CountAsync();
            SyncedMessages = await _db.EmailMessages
                .Where(e => e.Uid >= LowestSyncedUid)
                .CountAsync();
        }
    }
}