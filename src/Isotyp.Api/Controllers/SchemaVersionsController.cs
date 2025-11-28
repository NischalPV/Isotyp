using Microsoft.AspNetCore.Mvc;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Enums;

namespace Isotyp.Api.Controllers;

/// <summary>
/// API controller for managing schema versions.
/// All schema changes require multi-layer approval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SchemaVersionsController : ControllerBase
{
    private readonly ISchemaVersionService _service;

    public SchemaVersionsController(ISchemaVersionService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SchemaVersionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("datasource/{dataSourceId:guid}")]
    public async Task<ActionResult<IEnumerable<SchemaVersionDto>>> GetByDataSource(
        Guid dataSourceId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByDataSourceIdAsync(dataSourceId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("datasource/{dataSourceId:guid}/latest")]
    public async Task<ActionResult<SchemaVersionDto>> GetLatestApplied(
        Guid dataSourceId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetLatestAppliedAsync(dataSourceId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        if (result.Value == null)
            return NotFound("No applied schema version found.");
        
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<SchemaVersionDto>> Create(
        [FromBody] CreateSchemaVersionDto dto,
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
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.SubmitForApprovalAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromQuery] ApprovalLayer layer,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.ApproveAsync(id, layer, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
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

    [HttpPost("{id:guid}/apply")]
    public async Task<IActionResult> Apply(
        Guid id,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.ApplyAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/rollback")]
    public async Task<IActionResult> Rollback(
        Guid id,
        [FromBody] SchemaRollbackDto dto,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.RollbackAsync(id, dto.Reason, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> ApplyLock(
        Guid id,
        [FromBody] ApplySchemaLockDto dto,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.ApplyLockAsync(id, dto.LockType, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpDelete("{id:guid}/lock")]
    public async Task<IActionResult> RemoveLock(
        Guid id,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.RemoveLockAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }
}
