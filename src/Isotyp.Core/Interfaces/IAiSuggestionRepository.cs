using Isotyp.Core.Entities;
using Isotyp.Core.Enums;

namespace Isotyp.Core.Interfaces;

/// <summary>
/// Repository for AI suggestions.
/// </summary>
public interface IAiSuggestionRepository : IRepository<AiSuggestion>
{
    Task<IReadOnlyList<AiSuggestion>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiSuggestion>> GetUnreviewedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiSuggestion>> GetAcceptedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiSuggestion>> GetByConfidenceThresholdAsync(int minConfidence, CancellationToken cancellationToken = default);
}
