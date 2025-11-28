using Microsoft.AspNetCore.Mvc;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;

namespace Isotyp.Api.Controllers;

/// <summary>
/// API controller for AI suggestions.
/// AI may suggest schema/model evolution but never auto-applies changes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AiSuggestionsController : ControllerBase
{
    private readonly IAiSuggestionService _service;

    public AiSuggestionsController(IAiSuggestionService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AiSuggestionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("datasource/{dataSourceId:guid}")]
    public async Task<ActionResult<IEnumerable<AiSuggestionDto>>> GetByDataSource(
        Guid dataSourceId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByDataSourceIdAsync(dataSourceId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("unreviewed")]
    public async Task<ActionResult<IEnumerable<AiSuggestionDto>>> GetUnreviewed(CancellationToken cancellationToken)
    {
        var result = await _service.GetUnreviewedAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Record an AI suggestion. This is typically called by AI analysis processes.
    /// Note: AI suggestions are never auto-applied.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AiSuggestionDto>> Create(
        [FromBody] CreateAiSuggestionDto dto,
        CancellationToken cancellationToken)
    {
        // AI suggestions are created by the system
        var result = await _service.CreateAsync(dto, "ai-system", cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Human reviews an AI suggestion. Accepting creates a change request 
    /// that still requires multi-layer approval before being applied.
    /// </summary>
    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<SchemaChangeRequestDto>> Review(
        Guid id,
        [FromBody] ReviewAiSuggestionDto dto,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.ReviewAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        // If accepted and a change request was created, return it
        if (result.Value != null)
            return Ok(result.Value);
        
        return NoContent();
    }
}
