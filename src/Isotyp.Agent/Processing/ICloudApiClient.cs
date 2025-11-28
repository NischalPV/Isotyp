using Isotyp.Application.DTOs;

namespace Isotyp.Agent.Processing;

/// <summary>
/// Interface for communicating with the cloud API.
/// Only metadata is transmitted - never actual data.
/// </summary>
public interface ICloudApiClient
{
    /// <summary>
    /// Register agent with cloud.
    /// </summary>
    Task<AgentDto?> RegisterAgentAsync(RegisterAgentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send heartbeat to cloud.
    /// </summary>
    Task<bool> SendHeartbeatAsync(AgentHeartbeatDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Connect agent to cloud.
    /// </summary>
    Task<bool> ConnectAsync(string agentKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect agent from cloud.
    /// </summary>
    Task<bool> DisconnectAsync(string agentKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get data sources authorized for this agent.
    /// </summary>
    Task<IReadOnlyList<DataSourceDto>> GetAuthorizedDataSourcesAsync(string agentKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest applied schema version for a data source.
    /// </summary>
    Task<SchemaVersionDto?> GetLatestSchemaVersionAsync(Guid dataSourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a data validation record in cloud.
    /// </summary>
    Task<DataValidationDto?> CreateValidationAsync(CreateDataValidationDto dto, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete a data validation with results.
    /// </summary>
    Task<bool> CompleteValidationAsync(CompleteDataValidationDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Report AI suggestion based on local analysis.
    /// Note: AI suggestions are never auto-applied.
    /// </summary>
    Task<AiSuggestionDto?> ReportAiSuggestionAsync(CreateAiSuggestionDto dto, CancellationToken cancellationToken = default);
}
