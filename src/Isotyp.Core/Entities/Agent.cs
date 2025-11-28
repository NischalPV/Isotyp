using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents a local agent that processes data. Agents run locally and only send metadata to cloud.
/// </summary>
public class Agent : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// Unique identifier for this agent instance.
    /// </summary>
    public string AgentKey { get; private set; } = string.Empty;
    
    /// <summary>
    /// Version of the agent software.
    /// </summary>
    public string AgentVersion { get; private set; } = string.Empty;
    
    /// <summary>
    /// Hostname of the machine running the agent.
    /// </summary>
    public string HostName { get; private set; } = string.Empty;
    
    /// <summary>
    /// Whether the agent is currently connected.
    /// </summary>
    public bool IsConnected { get; private set; }
    
    /// <summary>
    /// Last time the agent sent a heartbeat.
    /// </summary>
    public DateTime? LastHeartbeat { get; private set; }
    
    /// <summary>
    /// Data sources this agent is authorized to process.
    /// </summary>
    public string AuthorizedDataSourceIds { get; private set; } = string.Empty;
    
    /// <summary>
    /// Configuration settings for the agent (non-sensitive only).
    /// </summary>
    public string Configuration { get; private set; } = string.Empty;

    private Agent() { }

    public static Agent Create(
        string name,
        string description,
        string agentKey,
        string agentVersion,
        string hostName,
        string createdBy)
    {
        var agent = new Agent
        {
            Name = name ?? throw new ArgumentNullException(nameof(name)),
            Description = description ?? string.Empty,
            AgentKey = agentKey ?? throw new ArgumentNullException(nameof(agentKey)),
            AgentVersion = agentVersion ?? throw new ArgumentNullException(nameof(agentVersion)),
            HostName = hostName ?? throw new ArgumentNullException(nameof(hostName)),
            IsConnected = false,
            AuthorizedDataSourceIds = "[]",
            Configuration = "{}"
        };
        agent.SetAuditFields(createdBy);
        return agent;
    }

    public void Connect()
    {
        IsConnected = true;
        LastHeartbeat = DateTime.UtcNow;
    }

    public void Disconnect()
    {
        IsConnected = false;
    }

    public void RecordHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

    public void UpdateVersion(string version, string updatedBy)
    {
        AgentVersion = version ?? throw new ArgumentNullException(nameof(version));
        UpdateAuditFields(updatedBy);
    }

    public void UpdateConfiguration(string configuration, string updatedBy)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        UpdateAuditFields(updatedBy);
    }

    public void AuthorizeDataSource(Guid dataSourceId, string authorizedBy)
    {
        // Parse existing authorized sources, add new one, serialize back
        var sources = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(AuthorizedDataSourceIds) ?? new List<Guid>();
        if (!sources.Contains(dataSourceId))
        {
            sources.Add(dataSourceId);
            AuthorizedDataSourceIds = System.Text.Json.JsonSerializer.Serialize(sources);
            UpdateAuditFields(authorizedBy);
        }
    }

    public void RevokeDataSourceAuthorization(Guid dataSourceId, string revokedBy)
    {
        var sources = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(AuthorizedDataSourceIds) ?? new List<Guid>();
        if (sources.Remove(dataSourceId))
        {
            AuthorizedDataSourceIds = System.Text.Json.JsonSerializer.Serialize(sources);
            UpdateAuditFields(revokedBy);
        }
    }

    public bool IsAuthorizedForDataSource(Guid dataSourceId)
    {
        var sources = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(AuthorizedDataSourceIds) ?? new List<Guid>();
        return sources.Contains(dataSourceId);
    }
}
