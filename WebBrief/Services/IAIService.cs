using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebBrief.Services
{
    public interface IAIService
    {
        Task<string> SummarizeAsync(string website, string personality);
    }

    public class GroqAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private readonly Dictionary<string, string> _personalityInstructions =
            new()
            {
                ["Friendly"] = "Use a friendly, conversational voice.",
                ["Expert"] = "Use a precise, knowledgeable voice and preserve important nuance.",
                ["Executive"] = "Lead with the key takeaway, then give concise decision-relevant points.",
                ["Explain simply"] = "Explain the ideas in plain language for a general audience."
            };

        public GroqAIService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> SummarizeAsync(
            string website,
            string personality)
        {
            var apiKey = _configuration["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "GROQ_API_KEY is not configured.";
            }

            var systemPrompt = """
            You analyze the contents of a website and
            ignore navigation menus and return a useful summary in markdown.
            """;

            var personalityInstruction =
                _personalityInstructions.GetValueOrDefault(
                    personality,
                    _personalityInstructions["Friendly"]);

            var requestBody = new
            {
                model = "openai/gpt-oss-120b",
                messages = new[]
                {
                new
                {
                    role = "system",
                    content = $"{systemPrompt}\n{personalityInstruction}"
                },
                new
                {
                    role = "user",
                    content = $"Summarize this website:\n\n{website}"
                }
            }
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(responseJson);

            return document
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
    }
}
