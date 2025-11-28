using Microsoft.AspNetCore.Mvc;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;

namespace Isotyp.Api.Controllers;

/// <summary>
/// API controller for managing local agents.
/// Agents process data locally; cloud sees metadata only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _service;

    public AgentsController(IAgentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("connected")]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetConnected(CancellationToken cancellationToken)
    {
        var result = await _service.GetConnectedAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AgentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpGet("key/{agentKey}")]
    public async Task<ActionResult<AgentDto>> GetByKey(string agentKey, CancellationToken cancellationToken)
    {
        var result = await _service.GetByAgentKeyAsync(agentKey, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<AgentDto>> Register(
        [FromBody] RegisterAgentDto dto,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.RegisterAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("connect/{agentKey}")]
    public async Task<IActionResult> Connect(string agentKey, CancellationToken cancellationToken)
    {
        var result = await _service.ConnectAsync(agentKey, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("disconnect/{agentKey}")]
    public async Task<IActionResult> Disconnect(string agentKey, CancellationToken cancellationToken)
    {
        var result = await _service.DisconnectAsync(agentKey, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat(
        [FromBody] AgentHeartbeatDto dto, 
        CancellationToken cancellationToken)
    {
        var result = await _service.HeartbeatAsync(dto, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/authorize")]
    public async Task<IActionResult> AuthorizeDataSource(
        Guid id,
        [FromBody] Guid dataSourceId,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.AuthorizeForDataSourceAsync(
            new AuthorizeAgentDto(id, dataSourceId), userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }

    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> RevokeDataSource(
        Guid id,
        [FromBody] Guid dataSourceId,
        [FromHeader(Name = "X-User-Id")] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is required in X-User-Id header.");

        var result = await _service.RevokeDataSourceAuthorizationAsync(
            new AuthorizeAgentDto(id, dataSourceId), userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }
}
