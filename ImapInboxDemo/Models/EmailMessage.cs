using System;

namespace ImapInboxDemo.Models
{
    public class EmailMessage
    {
        public int Id { get; set; }
        public long Uid { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        // Add these:
        public string Flags { get; set; } = string.Empty;
        public int Size { get; set; }

        // Optional: body, preview, etc.
        public string Body { get; set; } = string.Empty;
        public string PreviewText { get; set; } = string.Empty;
    }
}