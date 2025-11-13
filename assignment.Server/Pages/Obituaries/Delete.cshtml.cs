using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObituaryApplication.Data;
using ObituaryApplication.Models;
using System.Security.Claims;

namespace ObituaryApplication.Pages.Obituaries
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<IdentityUser> _userManager;

        public DeleteModel(ApplicationDbContext context, IWebHostEnvironment env, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        [BindProperty]
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

            // Check if user is authorized to delete (creator or admin)
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("admin") && obituary.CreatorId != currentUserId)
            {
                return Forbid();
            }

            Obituary = obituary;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var obituary = await _context.Obituaries.FindAsync(id);
            if (obituary != null)
            {
                // Check if user is authorized to delete (creator or admin)
                var currentUserId = _userManager.GetUserId(User);
                if (!User.IsInRole("admin") && obituary.CreatorId != currentUserId)
                {
                    return Forbid();
                }

                // Delete associated photo file if it exists
                if (!string.IsNullOrEmpty(obituary.PhotoPath))
                {
                    var photoPath = Path.Combine(_env.WebRootPath, obituary.PhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(photoPath))
                    {
                        System.IO.File.Delete(photoPath);
                    }
                }

                _context.Obituaries.Remove(obituary);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}