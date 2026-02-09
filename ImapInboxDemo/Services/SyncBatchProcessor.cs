using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImapInboxDemo.Data;
using ImapInboxDemo.Models;
using ImapInboxDemo.Telemetry;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;

namespace ImapInboxDemo.Services
{
    /// <summary>
    /// Handles all batch-level IMAP message processing:
    /// - Fetching
    /// - Processing
    /// - Attachment extraction
    /// - Batch saving
    /// - Real-time telemetry updates
    /// </summary>
    public class SyncBatchProcessor
    {
        private readonly AppDbContext _db;
        private readonly SyncTelemetryService _telemetry;

        private const int FetchBatchSize = 50;
        private const int SaveBatchSize = 50;

        public SyncBatchProcessor(AppDbContext db, SyncTelemetryService telemetry)
        {
            _db = db;
            _telemetry = telemetry;
        }

        /// <summary>
        /// Processes a list of UIDs in batches, updating telemetry as it goes.
        /// </summary>
        public async Task<int> ProcessAsync(
            IMailFolder inbox,
            IList<UniqueId> uids,
            string phase,
            CancellationToken token)
        {
            int total = uids.Count;
            int processed = 0;
            int inserted = 0;

            var start = DateTime.UtcNow;

            for (int i = 0; i < total; i += FetchBatchSize)
            {
                var batch = uids
                    .Skip(i)
                    .Take(FetchBatchSize)
                    .ToList();

                var summaries = await inbox.FetchAsync(
                    batch,
                    MessageSummaryItems.UniqueId |
                    MessageSummaryItems.Envelope |
                    MessageSummaryItems.Flags |
                    MessageSummaryItems.Size,
                    token);

                foreach (var summary in summaries)
                {
                    processed++;

                    long uid = summary.UniqueId.Id;

                    // Skip if already in DB
                    bool exists = await _db.EmailMessages
                        .AnyAsync(e => e.Uid == uid, token);

                    if (!exists)
                    {
                        var message = await inbox.GetMessageAsync(summary.UniqueId, token);

                        string body = message.HtmlBody
                            ?? message.TextBody
                            ?? message.GetTextBody(TextFormat.Html)
                            ?? message.GetTextBody(TextFormat.Plain)
                            ?? string.Empty;

                        var entity = new EmailMessage
                        {
                            Uid = uid,
                            MessageId = message.MessageId ?? string.Empty,
                            From = string.Join(", ", message.From.Select(f => f.ToString())),
                            To = string.Join(", ", message.To.Select(t => t.ToString())),
                            Subject = message.Subject ?? string.Empty,
                            Date = message.Date.UtcDateTime,
                            Flags = summary.Flags?.ToString() ?? string.Empty,
                            Size = summary.Size.HasValue ? (int)summary.Size.Value : 0,
                            Body = body
                        };

                        _db.EmailMessages.Add(entity);

                        AddAttachments(message, uid);

                        inserted++;
                    }

                    // Save in batches
                    if (processed % SaveBatchSize == 0)
                        await _db.SaveChangesAsync(token);

                    // Update telemetry
                    await UpdateTelemetryAsync(
                        phase,
                        total,
                        processed,
                        inserted,
                        uid,
                        start,
                        token);
                }
            }

            await _db.SaveChangesAsync(token);
            return inserted;
        }

        private async Task UpdateTelemetryAsync(
            string phase,
            int total,
            int processed,
            int inserted,
            long currentUid,
            DateTime start,
            CancellationToken token)
        {
            var elapsed = (DateTime.UtcNow - start).TotalSeconds;
            var mps = processed / Math.Max(1, elapsed);
            var remaining = total - processed;
            var eta = remaining / Math.Max(0.1, mps);

            var progress = new SyncProgress
            {
                Phase = phase,
                TotalMessages = total,
                Processed = processed,
                Inserted = inserted,
                CurrentUid = currentUid,
                MessagesPerSecond = mps,
                EstimatedSecondsRemaining = eta,
                LastUpdated = DateTime.UtcNow
            };

            await _telemetry.UpdateAsync(progress, token);
        }

        private void AddAttachments(MimeMessage message, long uid)
        {
            int index = 0;

            foreach (var attachment in message.Attachments)
            {
                var fileName =
                    attachment.ContentDisposition?.FileName ??
                    attachment.ContentType.Name ??
                    "attachment";

                var mimeType = attachment.ContentType.MimeType;

                long size = 0;
                if (attachment is MimePart mp)
                {
                    size = mp.ContentDisposition?.Size
                           ?? mp.Content?.Stream?.Length
                           ?? 0;
                }

                _db.EmailAttachments.Add(new EmailAttachment
                {
                    EmailMessageUid = uid,
                    FileName = fileName,
                    MimeType = mimeType,
                    Size = size,
                    AttachmentIndex = index
                });

                index++;
            }
        }
    }
}