using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents an AI suggestion for schema evolution.
/// AI can suggest changes but never auto-applies them.
/// </summary>
public class AiSuggestion : EntityBase
{
    public Guid DataSourceId { get; private set; }
    public Guid? SchemaVersionId { get; private set; }
    
    /// <summary>
    /// Type of change suggested.
    /// </summary>
    public ChangeType SuggestedChangeType { get; private set; }
    
    /// <summary>
    /// JSON representation of the suggested change.
    /// </summary>
    public string SuggestionDetails { get; private set; } = string.Empty;
    
    /// <summary>
    /// AI reasoning for the suggestion.
    /// </summary>
    public string Reasoning { get; private set; } = string.Empty;
    
    /// <summary>
    /// Confidence score (0-100).
    /// </summary>
    public int ConfidenceScore { get; private set; }
    
    /// <summary>
    /// Data patterns that triggered this suggestion.
    /// </summary>
    public string TriggeringPatterns { get; private set; } = string.Empty;
    
    /// <summary>
    /// Whether this suggestion was reviewed by a human.
    /// </summary>
    public bool IsReviewed { get; private set; }
    
    /// <summary>
    /// Whether this suggestion was accepted.
    /// </summary>
    public bool? IsAccepted { get; private set; }
    
    /// <summary>
    /// User who reviewed the suggestion.
    /// </summary>
    public string? ReviewedBy { get; private set; }
    
    /// <summary>
    /// When the suggestion was reviewed.
    /// </summary>
    public DateTime? ReviewedAt { get; private set; }
    
    /// <summary>
    /// Review comments.
    /// </summary>
    public string? ReviewComments { get; private set; }
    
    /// <summary>
    /// ID of the change request created from this suggestion (if accepted).
    /// </summary>
    public Guid? CreatedChangeRequestId { get; private set; }

    private AiSuggestion() { }

    public static AiSuggestion Create(
        Guid dataSourceId,
        Guid? schemaVersionId,
        ChangeType suggestedChangeType,
        string suggestionDetails,
        string reasoning,
        int confidenceScore,
        string triggeringPatterns,
        string createdBy)
    {
        var suggestion = new AiSuggestion
        {
            DataSourceId = dataSourceId,
            SchemaVersionId = schemaVersionId,
            SuggestedChangeType = suggestedChangeType,
            SuggestionDetails = suggestionDetails ?? throw new ArgumentNullException(nameof(suggestionDetails)),
            Reasoning = reasoning ?? string.Empty,
            ConfidenceScore = Math.Clamp(confidenceScore, 0, 100),
            TriggeringPatterns = triggeringPatterns ?? string.Empty,
            IsReviewed = false,
            IsAccepted = null
        };
        suggestion.SetAuditFields(createdBy);
        return suggestion;
    }

    public void Accept(string reviewedBy, string? comments = null)
    {
        if (IsReviewed)
            throw new InvalidOperationException("Suggestion has already been reviewed.");
        
        IsReviewed = true;
        IsAccepted = true;
        ReviewedBy = reviewedBy ?? throw new ArgumentNullException(nameof(reviewedBy));
        ReviewedAt = DateTime.UtcNow;
        ReviewComments = comments;
        UpdateAuditFields(reviewedBy);
    }

    public void Reject(string reviewedBy, string? comments = null)
    {
        if (IsReviewed)
            throw new InvalidOperationException("Suggestion has already been reviewed.");
        
        IsReviewed = true;
        IsAccepted = false;
        ReviewedBy = reviewedBy ?? throw new ArgumentNullException(nameof(reviewedBy));
        ReviewedAt = DateTime.UtcNow;
        ReviewComments = comments;
        UpdateAuditFields(reviewedBy);
    }

    public void LinkToChangeRequest(Guid changeRequestId, string updatedBy)
    {
        if (!IsAccepted.HasValue || !IsAccepted.Value)
            throw new InvalidOperationException("Can only link accepted suggestions to change requests.");
        
        CreatedChangeRequestId = changeRequestId;
        UpdateAuditFields(updatedBy);
    }

    public bool RequiresHighConfidenceForAutoSuggestion()
    {
        return SuggestedChangeType == ChangeType.RemoveTable ||
               SuggestedChangeType == ChangeType.RemoveColumn ||
               SuggestedChangeType == ChangeType.ModifyColumn;
    }
}
