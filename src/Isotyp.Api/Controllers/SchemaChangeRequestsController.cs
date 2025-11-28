using Microsoft.AspNetCore.Mvc;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;

namespace Isotyp.Api.Controllers;

/// <summary>
/// API controller for managing schema change requests.
/// All changes require explicit multi-layer human approval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SchemaChangeRequestsController : ControllerBase
{
    private readonly ISchemaChangeRequestService _service;

    public SchemaChangeRequestsController(ISchemaChangeRequestService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SchemaChangeRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("schemaversion/{schemaVersionId:guid}")]
    public async Task<ActionResult<IEnumerable<SchemaChangeRequestDto>>> GetBySchemaVersion(
        Guid schemaVersionId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetBySchemaVersionIdAsync(schemaVersionId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<SchemaChangeRequestDto>>> GetPending(CancellationToken cancellationToken)
    {
        var result = await _service.GetPendingApprovalsAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<SchemaChangeRequestDto>> Create(
        [FromBody] CreateSchemaChangeRequestDto dto,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.CreateAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(
        Guid id,
        [FromBody] SubmitChangeRequestDto dto,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.SubmitAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveChangeRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.ApproveAsync(dto, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/apply")]
    public async Task<ActionResult<SchemaVersionDto>> Apply(
        Guid id,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.ApplyAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] string reason,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.RejectAsync(id, reason, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }
}
