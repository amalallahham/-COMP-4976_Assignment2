namespace assignment.Client.Models
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string[]> FieldErrors { get; set; } = new();
    }
}
