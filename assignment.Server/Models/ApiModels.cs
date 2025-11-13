using System.ComponentModel.DataAnnotations;

namespace ObituaryApplication.Models
{
    // Authentication DTOs
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public DateTime DOD { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Biography must be at least 10 characters long")]
        public string Biography { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    // Obituary DTOs
    public class ObituaryResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public DateTime DOD { get; set; }
        public string Biography { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public string CreatorId { get; set; } = string.Empty;
        public string CreatorEmail { get; set; } = string.Empty;
    }

    public class CreateObituaryRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public DateTime DOD { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Biography must be at least 10 characters long")]
        public string Biography { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }
    }

    public class UpdateObituaryRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public DateTime DOD { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Biography must be at least 10 characters long")]
        public string Biography { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }
    }

    // API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    // Pagination
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}





