using Microsoft.AspNetCore.Mvc;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;

namespace Isotyp.Api.Controllers;

/// <summary>
/// API controller for audit log queries.
/// All actions are fully auditable and traceable.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _service;

    public AuditLogsController(IAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> Query(
        [FromQuery] AuditLogQueryDto query, 
        CancellationToken cancellationToken)
    {
        var result = await _service.QueryAsync(query, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditLogDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetByEntity(
        string entityType, 
        Guid entityId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByEntityAsync(entityType, entityId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("correlation/{correlationId:guid}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetByCorrelation(
        Guid correlationId, 
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByCorrelationIdAsync(correlationId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
}
