using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ObituaryApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIController> _logger;

        public AIController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AIController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Rewrites rough obituary notes into a polished, formal tribute paragraph using AI
        /// </summary>
        [HttpPost("rewrite")]
        public async Task<ActionResult<string>> RewriteObituary([FromBody] RewriteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Text cannot be empty.");
            }

            try
            {
                // Get Ollama API URL from configuration (defaults to localhost)
                var ollamaUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
                var model = _configuration["Ollama:Model"] ?? "llama3:latest"; // Default model
                
                // Prepare the request to Ollama API
                var apiUrl = $"{ollamaUrl}/api/chat";
                
                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a professional obituary writer. Your ONLY job is to rewrite the provided text in a formal, respectful tone. STRICT RULES: 1) Use EXACTLY the same information provided - rewrite it in formal tone only. 2) Do NOT add ANY information: no dates, no names, no family members, no achievements, no community involvement, no personal details - NOTHING that is not in the original text. 3) If the original says 'he is a banker', you can only say 'he was a banker' (past tense) - do NOT add 'successful', 'dedicated', or any other adjectives. 4) Output ONLY the rewritten text - no introductions, no quotes, no explanations. 5) Write in past tense. 6) Keep it formal but use ONLY the facts provided."
                        },
                        new
                        {
                            role = "user",
                            content = $"Rewrite this text in a formal obituary tone. Use EXACTLY the same information - do NOT add anything. If it says 'great banker', keep it as 'great banker' (just change to past tense). If it says 'loved gardening', keep it as 'loved gardening'. Do NOT add family members, dates, achievements, or any other information. Output ONLY the rewritten paragraph:\n\n{request.Text}"
                        }
                    },
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Set headers for Ollama API
                _httpClient.DefaultRequestHeaders.Clear();

                // Send request
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ollama API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, $"AI rewrite failed: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                // Extract the rewritten text from Ollama's response format
                // Ollama returns: { "message": { "content": "..." } }
                if (jsonResponse.TryGetProperty("message", out var message))
                {
                    if (message.TryGetProperty("content", out var contentElement))
                    {
                        var rewrittenText = contentElement.GetString() ?? string.Empty;
                        
                        // Clean up the response: remove introductory text and quotes
                        rewrittenText = CleanObituaryText(rewrittenText);
                        
                        return Ok(rewrittenText);
                    }
                }

                _logger.LogError("Unexpected response structure: {Response}", responseContent);
                return StatusCode(500, "Failed to parse AI response.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI rewrite");
                return StatusCode(500, $"AI rewrite failed: {ex.Message}");
            }
        }

        private string CleanObituaryText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Remove all quotes first
            text = text.Replace("\"", "").Replace("'", "").Replace("`", "");

            // Split into lines
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var cleanedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Skip lines that are introductory text (case-insensitive)
                var lowerLine = trimmedLine.ToLowerInvariant();
                if (lowerLine.StartsWith("here is") ||
                    lowerLine.StartsWith("the rewritten") ||
                    lowerLine.StartsWith("here's") ||
                    lowerLine.StartsWith("below") ||
                    lowerLine.StartsWith("following") ||
                    lowerLine.StartsWith("this is") ||
                    lowerLine.StartsWith("i've") ||
                    lowerLine.Contains("rewritten version") ||
                    lowerLine.Contains("formal and respectful tone") ||
                    string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    cleanedLines.Add(trimmedLine);
                }
            }

            var result = string.Join(" ", cleanedLines).Trim();
            
            // Final cleanup - remove any remaining quotes
            result = result.Trim('"', '\'', '`');

            return result;
        }
    }

    public class RewriteRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}

