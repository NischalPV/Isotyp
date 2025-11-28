using Isotyp.Application.Common;
using Isotyp.Application.DTOs;

namespace Isotyp.Application.Interfaces;

/// <summary>
/// Service for managing local agents. Agents process data locally; cloud sees metadata only.
/// </summary>
public interface IAgentService
{
    Task<Result<AgentDto>> RegisterAsync(RegisterAgentDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<AgentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AgentDto>> GetByAgentKeyAsync(string agentKey, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AgentDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AgentDto>>> GetConnectedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Agent connects and starts heartbeat.
    /// </summary>
    Task<Result> ConnectAsync(string agentKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Agent disconnects.
    /// </summary>
    Task<Result> DisconnectAsync(string agentKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process agent heartbeat.
    /// </summary>
    Task<Result> HeartbeatAsync(AgentHeartbeatDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Authorize an agent to process a specific data source.
    /// </summary>
    Task<Result> AuthorizeForDataSourceAsync(AuthorizeAgentDto dto, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoke agent authorization for a data source.
    /// </summary>
    Task<Result> RevokeDataSourceAuthorizationAsync(AuthorizeAgentDto dto, string userId, CancellationToken cancellationToken = default);
}
