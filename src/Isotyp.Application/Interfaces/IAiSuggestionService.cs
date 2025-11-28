using Isotyp.Application.Common;
using Isotyp.Application.DTOs;

namespace Isotyp.Application.Interfaces;

/// <summary>
/// Service for managing AI suggestions. AI suggests but never auto-applies changes.
/// </summary>
public interface IAiSuggestionService
{
    /// <summary>
    /// Record a new AI suggestion. This is called by AI analysis processes.
    /// AI suggestions are never auto-applied.
    /// </summary>
    Task<Result<AiSuggestionDto>> CreateAsync(CreateAiSuggestionDto dto, string systemUserId, CancellationToken cancellationToken = default);
    
    Task<Result<AiSuggestionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AiSuggestionDto>>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AiSuggestionDto>>> GetUnreviewedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Human reviews an AI suggestion. Accepting creates a change request (not auto-applied).
    /// </summary>
    Task<Result<SchemaChangeRequestDto?>> ReviewAsync(ReviewAiSuggestionDto dto, string userId, CancellationToken cancellationToken = default);
}
