using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObituaryApplication.Data;
using ObituaryApplication.Models;
using System.Security.Claims;

namespace ObituaryApplication.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class ObituariesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ObituariesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

    /// <summary>
    /// Get all obituaries with pagination and search
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResponse<ObituaryResponse>>>> GetObituaries(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var query = _context.Obituaries
                .Where(o => _context.Users.Any(u => u.Id == o.CreatorId)) // Only include obituaries with valid creators
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.FullName.ToLower().Contains(search.ToLower()));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var obituaries = await query
                .OrderByDescending(o => o.DOD)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new ObituaryResponse
                {
                    Id = o.Id,
                    FullName = o.FullName,
                    DOB = o.DOB,
                    DOD = o.DOD,
                    Biography = o.Biography,
                    PhotoPath = o.PhotoPath,
                    CreatorId = o.CreatorId,
                    CreatorEmail = _context.Users.Where(u => u.Id == o.CreatorId).Select(u => u.Email).FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            var pagedResponse = new PagedResponse<ObituaryResponse>
            {
                Data = obituaries,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return Ok(new ApiResponse<PagedResponse<ObituaryResponse>>
            {
                Success = true,
                Message = "Obituaries retrieved successfully",
                Data = pagedResponse
            });
        }

    /// <summary>
    /// Get a specific obituary by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ObituaryResponse>>> GetObituary(int id)
        {
            var obituary = await _context.Obituaries
                .Select(o => new ObituaryResponse
                {
                    Id = o.Id,
                    FullName = o.FullName,
                    DOB = o.DOB,
                    DOD = o.DOD,
                    Biography = o.Biography,
                    PhotoPath = o.PhotoPath,
                    CreatorId = o.CreatorId,
                    CreatorEmail = _context.Users.Where(u => u.Id == o.CreatorId).Select(u => u.Email).FirstOrDefault() ?? string.Empty
                })
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obituary == null)
            {
                return NotFound(new ApiResponse<ObituaryResponse>
                {
                    Success = false,
                    Message = "Obituary not found"
                });
            }

            return Ok(new ApiResponse<ObituaryResponse>
            {
                Success = true,
                Message = "Obituary retrieved successfully",
                Data = obituary
            });
        }

        /// <summary>
        /// Create a new obituary (authenticated users only)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ObituaryResponse>>> CreateObituary([FromForm] CreateObituaryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ObituaryResponse>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new ApiResponse<ObituaryResponse>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Handle photo upload
            string? photoPath = null;
            if (request.Photo != null)
            {
                photoPath = await SavePhotoAsync(request.Photo);
            }

            var obituary = new Obituary
            {
                FullName = request.FullName,
                DOB = request.DOB,
                DOD = request.DOD,
                Biography = request.Biography,
                PhotoPath = photoPath,
                CreatorId = userId
            };

            _context.Obituaries.Add(obituary);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            var response = new ObituaryResponse
            {
                Id = obituary.Id,
                FullName = obituary.FullName,
                DOB = obituary.DOB,
                DOD = obituary.DOD,
                Biography = obituary.Biography,
                PhotoPath = obituary.PhotoPath,
                CreatorId = obituary.CreatorId,
                CreatorEmail = user?.Email ?? string.Empty
            };

            return CreatedAtAction(nameof(GetObituary), new { id = obituary.Id }, new ApiResponse<ObituaryResponse>
            {
                Success = true,
                Message = "Obituary created successfully",
                Data = response
            });
        }

        /// <summary>
        /// Update an obituary (creator or admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ObituaryResponse>>> UpdateObituary(int id, [FromForm] UpdateObituaryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ObituaryResponse>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            // Find the obituary first
            var obituary = await _context.Obituaries.FindAsync(id);
            if (obituary == null)
            {
                return NotFound(new ApiResponse<ObituaryResponse>
                {
                    Success = false,
                    Message = "Obituary not found."
                });
            }

            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new ApiResponse<ObituaryResponse>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Check authorization: only creator or admin can update
            var isAdmin = User.IsInRole("admin");
            if (!isAdmin && obituary.CreatorId != userId)
            {
                return StatusCode(403, new ApiResponse<ObituaryResponse>
                {
                    Success = false,
                    Message = "Access denied. Only the creator or admin can update this obituary."
                });
            }

            // Handle photo upload
            if (request.Photo != null)
            {
                // Delete old photo if it exists
                if (!string.IsNullOrEmpty(obituary.PhotoPath))
                {
                    var oldPhotoPath = Path.Combine(_env.WebRootPath, obituary.PhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                }

                obituary.PhotoPath = await SavePhotoAsync(request.Photo);
            }

            obituary.FullName = request.FullName;
            obituary.DOB = request.DOB;
            obituary.DOD = request.DOD;
            obituary.Biography = request.Biography;

            _context.Update(obituary);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(obituary.CreatorId);
            var response = new ObituaryResponse
            {
                Id = obituary.Id,
                FullName = obituary.FullName,
                DOB = obituary.DOB,
                DOD = obituary.DOD,
                Biography = obituary.Biography,
                PhotoPath = obituary.PhotoPath,
                CreatorId = obituary.CreatorId,
                CreatorEmail = user?.Email ?? string.Empty
            };

            return Ok(new ApiResponse<ObituaryResponse>
            {
                Success = true,
                Message = "Obituary updated successfully",
                Data = response
            });
        }

        /// <summary>
        /// Delete an obituary (creator or admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteObituary(int id)
        {
            // Find the obituary first
            var obituary = await _context.Obituaries.FindAsync(id);
            if (obituary == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Obituary not found."
                });
            }

            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Check authorization: only creator or admin can delete
            var isAdmin = User.IsInRole("admin");
            if (!isAdmin && obituary.CreatorId != userId)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied. Only the creator or admin can delete this obituary."
                });
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

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Obituary deleted successfully"
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





