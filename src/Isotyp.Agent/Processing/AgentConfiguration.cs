namespace Isotyp.Agent.Processing;

/// <summary>
/// Configuration for the local agent.
/// Connection strings are stored and managed locally - never sent to cloud.
/// </summary>
public class AgentConfiguration
{
    /// <summary>
    /// Unique identifier for this agent.
    /// </summary>
    public string AgentKey { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this agent.
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Version of the agent software.
    /// </summary>
    public string AgentVersion { get; set; } = "1.0.0";

    /// <summary>
    /// API endpoint for the cloud metadata service.
    /// </summary>
    public string CloudApiEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Local storage path for connection strings and sensitive data.
    /// Never transmitted to cloud.
    /// </summary>
    public string LocalSecretStorePath { get; set; } = "./secrets";

    /// <summary>
    /// Interval for sending heartbeats to cloud (in seconds).
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Interval for validation checks (in seconds).
    /// </summary>
    public int ValidationIntervalSeconds { get; set; } = 3600;
}
