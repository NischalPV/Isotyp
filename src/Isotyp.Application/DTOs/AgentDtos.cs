namespace Isotyp.Application.DTOs;

/// <summary>
/// DTO for registering a new agent.
/// </summary>
public record RegisterAgentDto(
    string Name,
    string Description,
    string AgentKey,
    string AgentVersion,
    string HostName);

/// <summary>
/// DTO for agent response.
/// </summary>
public record AgentDto(
    Guid Id,
    string Name,
    string Description,
    string AgentKey,
    string AgentVersion,
    string HostName,
    bool IsConnected,
    DateTime? LastHeartbeat,
    DateTime CreatedAt);

/// <summary>
/// DTO for authorizing agent to a data source.
/// </summary>
public record AuthorizeAgentDto(
    Guid AgentId,
    Guid DataSourceId);

/// <summary>
/// DTO for agent heartbeat.
/// </summary>
public record AgentHeartbeatDto(
    string AgentKey,
    string AgentVersion,
    DateTime Timestamp);
