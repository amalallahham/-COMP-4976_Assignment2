using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ObituaryApplication.Data;
using ObituaryApplication.Models;
using ObituaryApplication.Services;
using System.ComponentModel.DataAnnotations;

namespace ObituaryApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            JwtTokenService jwtTokenService,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Login with email and password to get JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            var token = await _jwtTokenService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = new AuthResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60), // Should match JWT settings
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                }
            });
        }

        /// <summary>
        /// Register a new user and create an obituary
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromForm] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "User with this email already exists"
                });
            }

            // Create user
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Failed to create user",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Add user to "user" role
            await _userManager.AddToRoleAsync(user, "user");

            // Handle photo upload
            string? photoPath = null;
            if (request.Photo != null)
            {
                photoPath = await SavePhotoAsync(request.Photo);
            }

            // Create obituary
            var obituary = new Obituary
            {
                FullName = request.FullName,
                DOB = request.DOB,
                DOD = request.DOD,
                Biography = request.Biography,
                PhotoPath = photoPath,
                CreatorId = user.Id
            };

            _context.Obituaries.Add(obituary);
            await _context.SaveChangesAsync();

            // Generate token
            var token = await _jwtTokenService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Registration successful",
                Data = new AuthResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60),
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                }
            });
        }

        private async Task<string> SavePhotoAsync(IFormFile photo)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var fileName = $"{Guid.NewGuid()}_{photo.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var fs = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(fs);

            return $"/uploads/{fileName}";
        }
    }
}





