namespace assignment.Client.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
