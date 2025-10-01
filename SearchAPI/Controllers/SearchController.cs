using Microsoft.AspNetCore.Mvc;
using Shared;

namespace SearchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchLogic _searchLogic;

        public SearchController()
        {
            _searchLogic = new SearchLogic(new DatabaseSqlite());
        }

        [HttpGet]
        public IActionResult Search([FromQuery] string query, [FromQuery] int maxResults = 20, [FromQuery] bool showFullPaths = false, [FromQuery] bool caseSensitive = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Query parameter is required");
                }

                // Split query into words
                var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Perform search
                var result = _searchLogic.Search(queryWords, maxResults, caseSensitive);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("ping")]
        public string? Ping()
        {
            return Environment.GetEnvironmentVariable("id");
        }
    }
}
