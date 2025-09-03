using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Model;

namespace ConsoleSearch
{
    public class ApiSearchLogic
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiSearchLogic(string apiBaseUrl = "http://localhost:5281")
        {
            _httpClient = new HttpClient();
            _apiBaseUrl = apiBaseUrl;
        }

        public async Task<SearchResult> SearchAsync(string[] query, int maxAmount)
        {
            try
            {
                var searchRequest = new
                {
                    Query = query,
                    MaxResults = maxAmount
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
                    Console.WriteLine($"API call failed: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                Console.WriteLine("Make sure the SearchAPI is running on https://localhost:7216");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling API: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
