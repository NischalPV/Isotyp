using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;

namespace Isotyp.Application.Services;

/// <summary>
/// Service for managing AI suggestions. AI may suggest schema/model evolution but never auto-applies changes.
/// </summary>
public class AiSuggestionService : IAiSuggestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ISchemaChangeRequestService _changeRequestService;

    public AiSuggestionService(
        IUnitOfWork unitOfWork, 
        IAuditService auditService,
        ISchemaChangeRequestService changeRequestService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _changeRequestService = changeRequestService ?? throw new ArgumentNullException(nameof(changeRequestService));
    }

    public async Task<Result<AiSuggestionDto>> CreateAsync(CreateAiSuggestionDto dto, string systemUserId, CancellationToken cancellationToken = default)
    {
        // Verify data source exists
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(dto.DataSourceId, cancellationToken);
        if (dataSource == null)
            return Result<AiSuggestionDto>.Failure("Data source not found.");

        // Verify schema version if provided
        if (dto.SchemaVersionId.HasValue)
        {
            var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(dto.SchemaVersionId.Value, cancellationToken);
            if (schemaVersion == null)
                return Result<AiSuggestionDto>.Failure("Schema version not found.");
        }

        var suggestion = AiSuggestion.Create(
            dto.DataSourceId,
            dto.SchemaVersionId,
            dto.SuggestedChangeType,
            dto.SuggestionDetails,
            dto.Reasoning,
            dto.ConfidenceScore,
            dto.TriggeringPatterns,
            systemUserId);

        await _unitOfWork.AiSuggestions.AddAsync(suggestion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.AiSuggestionGenerated,
            nameof(AiSuggestion),
            suggestion.Id,
            systemUserId,
            "AI System",
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(suggestion)),
            $"Confidence: {dto.ConfidenceScore}%, Type: {dto.SuggestedChangeType}",
            null,
            null,
            cancellationToken);

        return Result<AiSuggestionDto>.Success(MapToDto(suggestion));
    }

    public async Task<Result<AiSuggestionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var suggestion = await _unitOfWork.AiSuggestions.GetByIdAsync(id, cancellationToken);
        if (suggestion == null)
            return Result<AiSuggestionDto>.Failure("AI suggestion not found.");

        return Result<AiSuggestionDto>.Success(MapToDto(suggestion));
    }

    public async Task<Result<IReadOnlyList<AiSuggestionDto>>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        var suggestions = await _unitOfWork.AiSuggestions.GetByDataSourceIdAsync(dataSourceId, cancellationToken);
        return Result<IReadOnlyList<AiSuggestionDto>>.Success(suggestions.Select(MapToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<AiSuggestionDto>>> GetUnreviewedAsync(CancellationToken cancellationToken = default)
    {
        var suggestions = await _unitOfWork.AiSuggestions.GetUnreviewedAsync(cancellationToken);
        return Result<IReadOnlyList<AiSuggestionDto>>.Success(suggestions.Select(MapToDto).ToList());
    }

    public async Task<Result<SchemaChangeRequestDto?>> ReviewAsync(ReviewAiSuggestionDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var suggestion = await _unitOfWork.AiSuggestions.GetByIdAsync(dto.SuggestionId, cancellationToken);
        if (suggestion == null)
            return Result<SchemaChangeRequestDto?>.Failure("AI suggestion not found.");

        if (suggestion.IsReviewed)
            return Result<SchemaChangeRequestDto?>.Failure("AI suggestion has already been reviewed.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(suggestion));

        if (dto.Accept)
        {
            suggestion.Accept(userId, dto.Comments);
            await _unitOfWork.AiSuggestions.UpdateAsync(suggestion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create a change request from the accepted AI suggestion
            // The change request still requires multi-layer human approval before being applied
            if (suggestion.SchemaVersionId.HasValue)
            {
                var changeRequestResult = await _changeRequestService.CreateAsync(
                    new CreateSchemaChangeRequestDto(
                        suggestion.SchemaVersionId.Value,
                        suggestion.SuggestedChangeType,
                        suggestion.SuggestionDetails,
                        $"[AI Suggested] {suggestion.Reasoning}",
                        $"AI-suggested change with confidence {suggestion.ConfidenceScore}%. Triggering patterns: {suggestion.TriggeringPatterns}",
                        true,
                        suggestion.ConfidenceScore),
                    userId,
                    cancellationToken);

                if (changeRequestResult.IsSuccess && changeRequestResult.Value != null)
                {
                    suggestion.LinkToChangeRequest(changeRequestResult.Value.Id, userId);
                    await _unitOfWork.AiSuggestions.UpdateAsync(suggestion, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _auditService.LogAsync(
                        AuditAction.AiSuggestionGenerated,
                        nameof(AiSuggestion),
                        suggestion.Id,
                        userId,
                        userId,
                        stateBefore,
                        System.Text.Json.JsonSerializer.Serialize(MapToDto(suggestion)),
                        $"Accepted and created change request: {changeRequestResult.Value.Id}",
                        null,
                        null,
                        cancellationToken);

                    return Result<SchemaChangeRequestDto?>.Success(changeRequestResult.Value);
                }
            }

            await _auditService.LogAsync(
                AuditAction.AiSuggestionGenerated,
                nameof(AiSuggestion),
                suggestion.Id,
                userId,
                userId,
                stateBefore,
                System.Text.Json.JsonSerializer.Serialize(MapToDto(suggestion)),
                "Accepted but no change request created (no schema version associated)",
                null,
                null,
                cancellationToken);

            return Result<SchemaChangeRequestDto?>.Success(null);
        }
        else
        {
            suggestion.Reject(userId, dto.Comments);
            await _unitOfWork.AiSuggestions.UpdateAsync(suggestion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.AiSuggestionGenerated,
                nameof(AiSuggestion),
                suggestion.Id,
                userId,
                userId,
                stateBefore,
                System.Text.Json.JsonSerializer.Serialize(MapToDto(suggestion)),
                $"Rejected. Comments: {dto.Comments}",
                null,
                null,
                cancellationToken);

            return Result<SchemaChangeRequestDto?>.Success(null);
        }
    }

    private static AiSuggestionDto MapToDto(AiSuggestion entity)
    {
        return new AiSuggestionDto(
            entity.Id,
            entity.DataSourceId,
            entity.SchemaVersionId,
            entity.SuggestedChangeType,
            entity.SuggestionDetails,
            entity.Reasoning,
            entity.ConfidenceScore,
            entity.TriggeringPatterns,
            entity.IsReviewed,
            entity.IsAccepted,
            entity.ReviewedBy,
            entity.ReviewedAt,
            entity.ReviewComments,
            entity.CreatedChangeRequestId,
            entity.CreatedAt);
    }
}
