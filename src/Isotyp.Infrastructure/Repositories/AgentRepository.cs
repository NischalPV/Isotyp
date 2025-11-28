using Microsoft.EntityFrameworkCore;
using Isotyp.Core.Entities;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure.Data;

namespace Isotyp.Infrastructure.Repositories;

public class AgentRepository : Repository<Agent>, IAgentRepository
{
    public AgentRepository(IsotypDbContext context) : base(context)
    {
    }

    public async Task<Agent?> GetByAgentKeyAsync(string agentKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.AgentKey == agentKey, cancellationToken);
    }

    public async Task<IReadOnlyList<Agent>> GetConnectedAgentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IsConnected)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Agent>> GetAgentsByDataSourceAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        // Filter by authorized data source IDs (stored as JSON array)
        var agents = await _dbSet.ToListAsync(cancellationToken);
        return agents.Where(a => a.IsAuthorizedForDataSource(dataSourceId)).ToList();
    }
}
