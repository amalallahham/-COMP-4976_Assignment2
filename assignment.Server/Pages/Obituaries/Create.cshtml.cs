using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ObituaryApplication.Data;
using ObituaryApplication.Models;

namespace ObituaryApplication.Pages.Obituaries
{
    [Authorize] 
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        [BindProperty]
        public Obituary Obituary { get; set; } = new Obituary();

        [BindProperty]
        public IFormFile? Photo { get; set; }

        public void OnGet()
        {

                Obituary.DOD = DateTime.Today;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid(); // Same as 403
            }

            Obituary.CreatorId = userId;

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

                Obituary.PhotoPath = $"/uploads/{fileName}";
            }

            _context.Obituaries.Add(Obituary);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Obituary for {Obituary.FullName} has been created successfully!";
            return RedirectToPage("Index");
        }
    }
}
