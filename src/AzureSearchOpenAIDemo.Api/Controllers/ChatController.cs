using AzureSearchOpenAIDemo.Api.Models;
using AzureSearchOpenAIDemo.Api.Orchestration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureSearchOpenAIDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatOrchestrator _orchestrator;

    public ChatController(ChatOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostAsync([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        var response = await _orchestrator.GetResponseAsync(request, cancellationToken);
        return Ok(response);
    }
}
