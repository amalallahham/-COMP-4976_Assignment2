using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ObituaryApplication.Data;
using ObituaryApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace ObituaryApplication.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name of Deceased")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            public DateTime DOB { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Death")]
            public DateTime DOD { get; set; }

            [Required]
            [Display(Name = "Biography / Tribute")]
            [MinLength(10, ErrorMessage = "Biography must be at least 10 characters long")]
            public string Biography { get; set; } = string.Empty;

            public IFormFile? Photo { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                // Create user account
                var user = new IdentityUser 
                { 
                    UserName = Input.Email, 
                    Email = Input.Email,
                    EmailConfirmed = true // Auto-confirm for simplicity
                };
                
                var result = await _userManager.CreateAsync(user, Input.Password);
                
                if (result.Succeeded)
                {
                    // Add user to "user" role
                    await _userManager.AddToRoleAsync(user, "user");

                    // Handle photo upload
                    string? photoPath = null;
                    if (Input.Photo != null)
                    {
                        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = $"{Guid.NewGuid()}_{Input.Photo.FileName}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using var fs = new FileStream(filePath, FileMode.Create);
                        await Input.Photo.CopyToAsync(fs);

                        photoPath = $"/uploads/{fileName}";
                    }

                    // Create obituary record
                    var obituary = new Obituary
                    {
                        FullName = Input.FullName,
                        DOB = Input.DOB,
                        DOD = Input.DOD,
                        Biography = Input.Biography,
                        PhotoPath = photoPath,
                        CreatorId = user.Id
                    };

                    _context.Obituaries.Add(obituary);
                    await _context.SaveChangesAsync();

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    TempData["Success"] = $"Obituary for {Input.FullName} has been successfully created.";
                    return RedirectToPage("/Obituaries/Index");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}