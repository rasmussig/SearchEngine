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
            _apiBaseUrl = configuration["SearchApi:BaseUrl"] ?? "http://localhost:5281";
        }

        public async Task<SearchResult?> SearchAsync(string[] query, int maxResults = 20, bool caseSensitive = false)
        {
            try
            {
                var queryString = string.Join(" ", query);
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Search?query=test&maxResults=1");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
