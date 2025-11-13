namespace assignment.Client.Models
{
    public class HttpValidationProblemDetails
    {
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}
