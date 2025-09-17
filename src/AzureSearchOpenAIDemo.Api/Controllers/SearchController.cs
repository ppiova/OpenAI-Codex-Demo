using AzureSearchOpenAIDemo.Api.Models;
using AzureSearchOpenAIDemo.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureSearchOpenAIDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IAzureSearchService _searchService;

    public SearchController(IAzureSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyList<SearchDocumentResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostAsync([FromBody] SearchRequest request, CancellationToken cancellationToken)
    {
        var results = await _searchService.SearchAsync(request, cancellationToken);
        return Ok(results);
    }
}
