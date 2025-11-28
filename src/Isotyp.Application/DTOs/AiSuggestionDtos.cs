using Isotyp.Core.Enums;

namespace Isotyp.Application.DTOs;

/// <summary>
/// DTO for creating an AI suggestion.
/// </summary>
public record CreateAiSuggestionDto(
    Guid DataSourceId,
    Guid? SchemaVersionId,
    ChangeType SuggestedChangeType,
    string SuggestionDetails,
    string Reasoning,
    int ConfidenceScore,
    string TriggeringPatterns);

/// <summary>
/// DTO for reviewing an AI suggestion.
/// </summary>
public record ReviewAiSuggestionDto(
    Guid SuggestionId,
    bool Accept,
    string? Comments);

/// <summary>
/// DTO for AI suggestion response.
/// </summary>
public record AiSuggestionDto(
    Guid Id,
    Guid DataSourceId,
    Guid? SchemaVersionId,
    ChangeType SuggestedChangeType,
    string SuggestionDetails,
    string Reasoning,
    int ConfidenceScore,
    string TriggeringPatterns,
    bool IsReviewed,
    bool? IsAccepted,
    string? ReviewedBy,
    DateTime? ReviewedAt,
    string? ReviewComments,
    Guid? CreatedChangeRequestId,
    DateTime CreatedAt);
