using System.ComponentModel.DataAnnotations;

namespace assignment.Client.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }
        public T? Data { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class ObituaryResponse
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public DateTime? DOB { get; set; }
        public DateTime? DOD { get; set; }

        public string? Biography { get; set; }

        public string? PhotoPath { get; set; }

        public string? CreatorId { get; set; }
        public string CreatorEmail { get; set; } = string.Empty;
    }

    public class CreateObituaryRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DOB { get; set; }
        public DateTime? DOD { get; set; }

        public string? Biography { get; set; }
        // Photo is sent separately as IBrowserFile in the service, so no IFormFile here
    }

    public class UpdateObituaryRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DOB { get; set; }
        public DateTime? DOD { get; set; }

        public string? Biography { get; set; }
        // Photo also handled separately
    }
}
