using Microsoft.EntityFrameworkCore;
using Isotyp.Core.Entities;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure.Data;

namespace Isotyp.Infrastructure.Repositories;

public class AiSuggestionRepository : Repository<AiSuggestion>, IAiSuggestionRepository
{
    public AiSuggestionRepository(IsotypDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AiSuggestion>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.DataSourceId == dataSourceId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiSuggestion>> GetUnreviewedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => !s.IsReviewed)
            .OrderByDescending(s => s.ConfidenceScore)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiSuggestion>> GetAcceptedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsAccepted == true)
            .OrderByDescending(s => s.ReviewedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiSuggestion>> GetByConfidenceThresholdAsync(int minConfidence, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ConfidenceScore >= minConfidence)
            .OrderByDescending(s => s.ConfidenceScore)
            .ToListAsync(cancellationToken);
    }
}
