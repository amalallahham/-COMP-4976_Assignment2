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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment env, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        [BindProperty]
        public Obituary Obituary { get; set; } = default!;

        [BindProperty]
        public IFormFile? Photo { get; set; }

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

            // Check if user is authorized to edit (creator or admin)
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("admin") && obituary.CreatorId != currentUserId)
            {
                return Forbid();
            }

            Obituary = obituary;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if user is authorized to edit (creator or admin)
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("admin") && Obituary.CreatorId != currentUserId)
            {
                return Forbid();
            }

            // Handle photo upload
            if (Photo != null)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var fileName = $"{Guid.NewGuid()}_{Photo.FileName}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                await Photo.CopyToAsync(fs);

                // Delete old photo if it exists
                if (!string.IsNullOrEmpty(Obituary.PhotoPath))
                {
                    var oldPhotoPath = Path.Combine(_env.WebRootPath, Obituary.PhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                }

                Obituary.PhotoPath = $"/uploads/{fileName}";
            }

            _context.Attach(Obituary).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObituaryExists(Obituary.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Details", new { id = Obituary.Id });
        }

        private bool ObituaryExists(int id)
        {
            return _context.Obituaries.Any(e => e.Id == id);
        }
    }
}