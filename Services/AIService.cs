using BillingAutomation.Models;
using System.Text.Json;
using System.Text;

namespace BillingAutomation.Services;

public class AIService : IAIService
{
    private readonly string? _apiKey;
    private readonly HttpClient _httpClient;
    private const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

    public AIService(IConfiguration configuration)
    {
        _apiKey = configuration["Gemini:ApiKey"];
        _httpClient = new HttpClient();
    }

    public object AnalyzeData(List<ProcessedCustomerDto> processed, List<RejectedCustomerDto> rejected)
    {
        // 1. Prepare data for AI
        var analysisPayload = new
        {
            Processed = processed.Select(p => new { p.CustomerId, p.PlanName, p.Usage, p.CityRegion }),
            Rejected = rejected.Select(r => new { r.CustomerId, r.Errors, Original = r.OriginalData })
        };

        string jsonPayload = JsonSerializer.Serialize(analysisPayload);

        // 2. Mock Mode
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return new
            {
                Note = "AI Service is in MOCK mode (no 'Gemini:ApiKey' configured).",
                Summary = $"Processed {processed.Count} customers. Rejected {rejected.Count}. (Mock Gemini Analysis)"
            };
        }

        // 3. Real Mode (Gemini REST API)
        try
        {
            var prompt = @"You are an AI assistant supporting a billing automation system.
Analyze the provided customer usage data and:
1. Flag suspicious records (unknown plans, extreme usage, missing city).
2. Suggest standardized plan categories (Basic, Standard, Premium).
3. Generate a concise operational summary.
Rules:
- Do not recalculate or modify billing values.
- Do not alter customer IDs or usage.
- Respond in structured JSON only, without markdown formatting.

Data:
" + jsonPayload;

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var url = $"{GeminiEndpoint}?key={_apiKey}";

            // Synchronous call for simplicity in this synchronous method context, or blocking async
            var response = _httpClient.PostAsync(url, content).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                return new { Error = "Gemini API call failed", StatusCode = response.StatusCode, Details = responseString };
            }

            // Parse Gemini Response
            using var doc = JsonDocument.Parse(responseString);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Clean up markdown block if present (```json ... ```) 
            if (text != null && text.StartsWith("```"))
            {
                 var lines = text.Split('\n');
                 if (lines.Length > 2)
                 {
                     text = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
                 }
            }

            try
            {
                return JsonSerializer.Deserialize<object>(text ?? "{}") ?? new { Raw = text };
            }
            catch
            {
                return new { RawResponse = text, Error = "Failed to parse JSON from AI response" };
            }
        }
        catch (Exception ex)
        {
            return new
            {
                Error = "AI Service call failed.",
                Details = ex.Message
            };
        }
    }
}
