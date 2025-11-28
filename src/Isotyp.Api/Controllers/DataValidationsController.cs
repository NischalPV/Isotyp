using Microsoft.AspNetCore.Mvc;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;

namespace Isotyp.Api.Controllers;

/// <summary>
/// API controller for data validation operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataValidationsController : ControllerBase
{
    private readonly IDataValidationService _service;

    public DataValidationsController(IDataValidationService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DataValidationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("datasource/{dataSourceId:guid}")]
    public async Task<ActionResult<IEnumerable<DataValidationDto>>> GetByDataSource(
        Guid dataSourceId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByDataSourceIdAsync(dataSourceId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("datasource/{dataSourceId:guid}/latest")]
    public async Task<ActionResult<DataValidationDto>> GetLatest(
        Guid dataSourceId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetLatestByDataSourceIdAsync(dataSourceId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        if (result.Value == null)
            return NotFound("No validation found for this data source.");
        
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<DataValidationDto>> Create(
        [FromBody] CreateDataValidationDto dto,
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

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.StartAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteDataValidationDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CompleteAsync(dto, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/fail")]
    public async Task<IActionResult> Fail(
        Guid id,
        [FromBody] FailValidationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.FailAsync(id, request.ErrorMessage, request.DurationMs, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }
}

public record FailValidationRequest(string ErrorMessage, long DurationMs);
