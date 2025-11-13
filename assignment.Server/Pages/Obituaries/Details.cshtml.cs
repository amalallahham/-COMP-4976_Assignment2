using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObituaryApplication.Data;
using ObituaryApplication.Models;
using System.Security.Claims;

namespace ObituaryApplication.Pages.Obituaries
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Obituary Obituary { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var obituary = await _context.Obituaries.FirstOrDefaultAsync(m => m.Id == id);
            
            if (obituary == null)
            {
                return NotFound();
            }
            
            Obituary = obituary;
            return Page();
        }
    }
}