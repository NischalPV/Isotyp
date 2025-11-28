using Microsoft.AspNetCore.Mvc;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;

namespace Isotyp.Api.Controllers;

/// <summary>
/// API controller for managing data sources.
/// Note: Connection details are never exposed via API - only metadata.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataSourcesController : ControllerBase
{
    private readonly IDataSourceService _service;

    public DataSourcesController(IDataSourceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSourceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DataSourceDto>>> GetActive(CancellationToken cancellationToken)
    {
        var result = await _service.GetActiveAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DataSourceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<DataSourceDto>> Create(
        [FromBody] CreateDataSourceDto dto, 
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DataSourceDto>> Update(
        Guid id, 
        [FromBody] UpdateDataSourceDto dto,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.UpdateAsync(id, dto, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.ActivateAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.DeactivateAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.DeleteAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }
}
