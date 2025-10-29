using Microsoft.AspNetCore.Mvc;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace SearchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchLogic _searchLogic;

        public SearchController()
        {
            // Y-scaling: Auto-detect database shards
            var shardPaths = Paths.GetDatabaseShards();
            
            if (shardPaths.Count > 0)
            {
                // Multiple shards found - use MultiDatabaseWrapper
                var databases = shardPaths.Select(path => new DatabaseSqlite(path) as IDatabase).ToList();
                _searchLogic = new SearchLogic(new MultiDatabaseWrapper(databases));
            }
            else
            {
                // No shards - use single database
                _searchLogic = new SearchLogic(new DatabaseSqlite());
            }
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
