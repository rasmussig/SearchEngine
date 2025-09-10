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

        [HttpPost]
        public IActionResult SearchPost([FromBody] SearchRequest request)
        {
            try
            {
                if (request == null || request.Query == null || request.Query.Length == 0)
                {
                    return BadRequest("Query is required");
                }

                var result = _searchLogic.Search(request.Query, request.MaxResults ?? 20, request.CaseSensitive);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class SearchRequest
    {
        public string[] Query { get; set; } = Array.Empty<string>();
        public int? MaxResults { get; set; }
        public bool ShowFullPaths { get; set; } = false;
        public bool CaseSensitive { get; set; } = false;
    }
}
