using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ImapInboxDemo.Data;
using ImapInboxDemo.Models;

namespace ImapInboxDemo.Services
{
    public class ImapSyncService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ImapSyncService> _logger;
        private readonly ImapSettings _settings;

        private const int SaveBatchSize = 100;

        public ImapSyncService(
            AppDbContext db,
            ILogger<ImapSyncService> logger,
            IOptions<ImapSettings> settings)
        {
            _db = db;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task SyncInboxAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(">>> ENTERED SyncInboxAsync <<<");

            try
            {
                // Ensure SyncState exists
                var state = await _db.SyncState.FirstOrDefaultAsync(cancellationToken);
                if (state == null)
                {
                    _logger.LogWarning("SyncState missing - creating new row.");

                    state = new SyncState
                    {
                        LastRunNewMessages = 0,
                        LastRunTime = DateTime.UtcNow,
                        LowestSyncedUid = 0,
                        PauseBackfill = false
                    };

                    _db.SyncState.Add(state);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                using var client = new ImapClient();
                _logger.LogInformation("Connecting to IMAP {Host}:{Port} SSL={UseSsl}",
                    _settings.Host, _settings.Port, _settings.UseSsl);

                await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, cancellationToken);
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

                _logger.LogInformation("Connected. Inbox message count: {Count}", inbox.Count);

                // Fetch all UIDs
                var allUids = await inbox.SearchAsync(SearchQuery.All, cancellationToken);
                _logger.LogInformation("Server returned {Count} UIDs.", allUids.Count);

                if (allUids.Count == 0)
                {
                    _logger.LogInformation("No messages on server.");
                    return;
                }

                long newestUid = allUids.Max().Id;
                _logger.LogInformation("Highest UID on server: {Uid}", newestUid);

                if (state.LowestSyncedUid == 0)
                {
                    state.LowestSyncedUid = newestUid;
                    await _db.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Initialized LowestSyncedUid to {Uid}", newestUid);
                }

                // Determine max stored UID in DB
                long maxStoredUid = await _db.EmailMessages.AnyAsync(cancellationToken)
                    ? await _db.EmailMessages.MaxAsync(e => e.Uid, cancellationToken)
                    : 0;

                _logger.LogInformation("Max stored UID in DB: {Uid}", maxStoredUid);

                var newUids = allUids
                    .Where(u => (long)u.Id > maxStoredUid)
                    .OrderBy(u => u.Id)
                    .ToList();

                _logger.LogInformation("New UIDs to process: {Count}", newUids.Count);

                if (newUids.Count > 0)
                {
                    await SyncNewMessagesAsync(inbox, newUids, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("No new messages to sync.");
                }

                state.LastRunTime = DateTime.UtcNow;
                state.LastRunNewMessages = newUids.Count;

                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("SyncInboxAsync completed. New messages this run: {Count}", newUids.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FATAL ERROR inside SyncInboxAsync.");
                throw;
            }
        }

        private async Task SyncNewMessagesAsync(
            IMailFolder inbox,
            System.Collections.Generic.List<UniqueId> newUids,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching summaries for {Count} new UIDs.", newUids.Count);

            var summaries = await inbox.FetchAsync(
                newUids,
                MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.InternalDate,
                cancellationToken);

            int inserted = 0;

            foreach (var summary in summaries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                long uid = (long)summary.UniqueId.Id;

                bool exists = await _db.EmailMessages.AnyAsync(e => e.Uid == uid, cancellationToken);
                if (exists)
                {
                    _logger.LogDebug("Skipping UID {Uid} - already exists.", uid);
                    continue;
                }

                var message = await inbox.GetMessageAsync(summary.UniqueId, cancellationToken);

                // Extract body (HTML preferred, fallback to text, then multipart)
                string body = "";

                if (!string.IsNullOrEmpty(message.HtmlBody))
                {
                    body = message.HtmlBody;
                }
                else if (!string.IsNullOrEmpty(message.TextBody))
                {
                    body = message.TextBody;
                }
                else
                {
                    var html = message.GetTextBody(MimeKit.Text.TextFormat.Html);
                    var text = message.GetTextBody(MimeKit.Text.TextFormat.Plain);
                    body = html ?? text ?? "";
                }

                var entity = new EmailMessage
                {
                    Uid = uid,
                    MessageId = message.MessageId ?? "",
                    From = message.From?.ToString() ?? "",
                    To = message.To?.ToString() ?? "",
                    Subject = message.Subject ?? "",
                    Date = message.Date.UtcDateTime,
                    Flags = summary.Flags?.ToString() ?? "",
                    Size = body.Length,
                    Body = body
                };

                _db.EmailMessages.Add(entity);
                inserted++;

                if (inserted % SaveBatchSize == 0)
                {
                    _logger.LogInformation("Saving batch of {BatchSize} messages...", SaveBatchSize);
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            _logger.LogInformation("Final save. Total inserted this run: {Count}", inserted);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}