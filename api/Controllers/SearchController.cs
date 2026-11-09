using business_layer.Dtos;
using business_layer.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> searchLogger)
        {
            _searchService = searchService;
            _logger = searchLogger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentSearchResultDto>>> Search([FromQuery] string q)
        {
            _logger.LogInformation("HTTP GET /api/Search?q={Query} empfangen.", q);

            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Der Suchparameter 'q' darf nicht leer sein.");
            }

            var results = await _searchService.SearchDocumentsAsync(q);
            return Ok(results);
        }
    }
}