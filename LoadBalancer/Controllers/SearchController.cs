using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LoadBalancer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SearchController> _logger;
        private readonly Random _random = new();
        
        private readonly List<string> _backendUrls;

        public SearchController(HttpClient httpClient, ILogger<SearchController> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Read backend URLs from configuration (supports both local and Docker)
            _backendUrls = configuration.GetSection("BackendUrls").Get<List<string>>() ?? new()
            {
                "http://localhost:5281",
                "http://localhost:5282", 
                "http://localhost:5283"
            };
            
            _logger.LogInformation("LoadBalancer configured with backends: {Backends}", string.Join(", ", _backendUrls));
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var instanceStatus = new List<object>();
            
            foreach (var url in _backendUrls)
            {
                var isHealthy = await CheckInstanceHealth(url);
                instanceStatus.Add(new
                {
                    url = url,
                    status = isHealthy ? "Active" : "Inactive"
                });
            }
            
            return Ok(new
            {
                loadBalancer = "LoadBalancer is running",
                backendInstances = instanceStatus
            });
        }
        
        private async Task<bool> CheckInstanceHealth(string baseUrl)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var response = await _httpClient.GetAsync($"{baseUrl}/api/Search/ping", cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<string?> GetRandomHealthyInstance()
        {
            var healthyInstances = new List<string>();
            
            foreach (var url in _backendUrls)
            {
                if (await CheckInstanceHealth(url))
                {
                    healthyInstances.Add(url);
                }
            }
            
            if (!healthyInstances.Any())
            {
                return null;
            }
            
            return healthyInstances[_random.Next(healthyInstances.Count)];
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int maxResults = 20, 
            [FromQuery] bool showFullPaths = false, [FromQuery] bool caseSensitive = false)
        {
            var selectedInstance = await GetRandomHealthyInstance();
            
            if (selectedInstance == null)
            {
                _logger.LogWarning("No healthy backend instances available");
                return StatusCode(503, new { error = "No healthy backend instances available" });
            }
            
            try
            {
                var queryString = $"?query={Uri.EscapeDataString(query)}&maxResults={maxResults}&showFullPaths={showFullPaths}&caseSensitive={caseSensitive}";
                var url = $"{selectedInstance}/api/Search{queryString}";
                
                _logger.LogInformation("Forwarding GET request to: {Url}", url);
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                // Add info about which backend was used
                Response.Headers["X-LoadBalancer-Backend"] = selectedInstance;
                Response.Headers["X-LoadBalancer-Strategy"] = "RandomHealthy";
                
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding request to backend: {Backend}", selectedInstance);
                return StatusCode(502, new { error = "Backend unavailable", backend = selectedInstance });
            }
        }
    }
}