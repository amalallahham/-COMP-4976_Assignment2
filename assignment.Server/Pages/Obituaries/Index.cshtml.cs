using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObituaryApplication.Data;
using ObituaryApplication.Models;

namespace ObituaryApplication.Pages.Obituaries
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<Obituary> Obituaries { get; set; } = new List<Obituary>();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Obituaries
                .Where(o => _context.Users.Any(u => u.Id == o.CreatorId)) // Only include obituaries with valid creators
                .AsQueryable();

            // Trim whitespace and check if search term is not empty
            var searchTerm = Search?.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Case-insensitive search that handles extra whitespace
                query = query.Where(o => o.FullName.ToLower().Contains(searchTerm.ToLower()));
            }

            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            Obituaries = await query
                .OrderByDescending(o => o.DOD)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
