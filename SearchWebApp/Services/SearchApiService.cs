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

        public async Task<SearchResult?> SearchAsync(string[] query, int maxResults = 20)
        {
            try
            {
                var searchRequest = new
                {
                    Query = query,
                    MaxResults = maxResults
                };

                var json = JsonSerializer.Serialize(searchRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Search", content);
                
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
