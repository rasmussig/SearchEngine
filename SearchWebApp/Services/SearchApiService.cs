using System.Text;
using System.Text.Json;
using Shared.Model;

namespace SearchWebApp.Services
{
    public class SearchApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public SearchApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["SearchApi:BaseUrl"] ?? "http://localhost:5280";
        }

        public async Task<SearchResult?> SearchAsync(string[] query, int maxResults = 20, bool caseSensitive = false)
        {
            try
            {
                // Convert query array to single string
                var queryString = string.Join(" ", query);
                
                // Build query parameters for GET request
                var url = $"{_apiBaseUrl}/api/Search?query={Uri.EscapeDataString(queryString)}&maxResults={maxResults}&caseSensitive={caseSensitive}";

                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        IncludeFields = true
                    };
                    return JsonSerializer.Deserialize<SearchResult>(responseContent, options);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Search?query=test&maxResults=1", cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
