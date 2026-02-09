using ImapInboxDemo.Data;
using ImapInboxDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/sync")]
public class SyncControlController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ImapSyncBackgroundService _worker;

    public SyncControlController(AppDbContext db, ImapSyncBackgroundService worker)
    {
        _db = db;
        _worker = worker;
    }

    [HttpPost("force")]
    public IActionResult ForceSync()
    {
        _worker.TriggerImmediateSync();
        return Ok(new { message = "Sync triggered" });
    }

    [HttpPost("pause")]
    public async Task<IActionResult> PauseBackfill([FromBody] bool pause)
    {
        var state = await _db.SyncState.FirstAsync();
        state.PauseBackfill = pause;
        await _db.SaveChangesAsync();
        return Ok(new { paused = pause });
    }
}