using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RestaurantPOS.AI.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GenerateContentAsync(object requestBody)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-3.7-flash";

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API Key is not configured in appsettings.json.");
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Log lỗi chi tiết ở backend, nhưng chỉ ném ra thông báo lỗi HTTP cơ bản
                    throw new HttpRequestException($"Gemini API returned error: {response.StatusCode}. Details: {jsonResponse}");
                }

                return jsonResponse;
            }
            catch (Exception ex)
            {
                // Bảo mật: Không để lộ API Key trong exception message
                throw new Exception("Error communicating with Gemini AI Service.", ex);
            }
        }
    }
}
