using ImapInboxDemo.Data;
using ImapInboxDemo.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ImapInboxDemo.Pages
{
    public class InboxModel : PageModel
    {
        private readonly AppDbContext _db;

        public InboxModel(AppDbContext db)
        {
            _db = db;
        }

        public List<EmailMessage> Messages { get; set; } = new();
        public EmailMessage? SelectedMessage { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalMessages { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }

        public async Task OnGet(
            int pageNumber = 1,
            string? searchTerm = null,
            int? id = null)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            SearchTerm = searchTerm;

            var query = _db.EmailMessages.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim();
                query = query.Where(e =>
                    e.Subject.Contains(term) ||
                    e.From.Contains(term) ||
                    e.To.Contains(term) ||
                    e.Body.Contains(term));
            }

            TotalMessages = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalMessages / (double)PageSize);

            Messages = await query
                .OrderByDescending(e => e.Date)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // ? Load selected message for preview
            if (id.HasValue)
            {
                SelectedMessage = await _db.EmailMessages
                    .FirstOrDefaultAsync(e => e.Id == id.Value);
            }
        }
    }
}