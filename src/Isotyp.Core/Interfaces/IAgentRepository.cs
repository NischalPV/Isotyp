using Isotyp.Core.Entities;

namespace Isotyp.Core.Interfaces;

/// <summary>
/// Repository for agents.
/// </summary>
public interface IAgentRepository : IRepository<Agent>
{
    Task<Agent?> GetByAgentKeyAsync(string agentKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agent>> GetConnectedAgentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agent>> GetAgentsByDataSourceAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
}
