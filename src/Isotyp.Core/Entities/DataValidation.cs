using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents a data validation result against the canonical schema.
/// System re-validates data over time to ensure conformance.
/// </summary>
public class DataValidation : EntityBase
{
    public Guid DataSourceId { get; private set; }
    public Guid SchemaVersionId { get; private set; }
    public Guid? AgentId { get; private set; }
    
    public ValidationStatus Status { get; private set; }
    
    /// <summary>
    /// When the validation was executed.
    /// </summary>
    public DateTime ExecutedAt { get; private set; }
    
    /// <summary>
    /// Duration of the validation in milliseconds.
    /// </summary>
    public long DurationMs { get; private set; }
    
    /// <summary>
    /// Total records validated.
    /// </summary>
    public long TotalRecords { get; private set; }
    
    /// <summary>
    /// Records that passed validation.
    /// </summary>
    public long PassedRecords { get; private set; }
    
    /// <summary>
    /// Records that failed validation.
    /// </summary>
    public long FailedRecords { get; private set; }
    
    /// <summary>
    /// Records with warnings.
    /// </summary>
    public long WarningRecords { get; private set; }
    
    /// <summary>
    /// JSON array of validation errors.
    /// </summary>
    public string Errors { get; private set; } = string.Empty;
    
    /// <summary>
    /// JSON array of validation warnings.
    /// </summary>
    public string Warnings { get; private set; } = string.Empty;
    
    /// <summary>
    /// Summary of the validation results.
    /// </summary>
    public string Summary { get; private set; } = string.Empty;

    private DataValidation() { }

    public static DataValidation Create(
        Guid dataSourceId,
        Guid schemaVersionId,
        Guid? agentId,
        string createdBy)
    {
        var validation = new DataValidation
        {
            DataSourceId = dataSourceId,
            SchemaVersionId = schemaVersionId,
            AgentId = agentId,
            Status = ValidationStatus.Pending,
            ExecutedAt = DateTime.UtcNow,
            Errors = "[]",
            Warnings = "[]"
        };
        validation.SetAuditFields(createdBy);
        return validation;
    }

    public void Start()
    {
        Status = ValidationStatus.InProgress;
        ExecutedAt = DateTime.UtcNow;
    }

    public void Complete(
        long totalRecords,
        long passedRecords,
        long failedRecords,
        long warningRecords,
        string errors,
        string warnings,
        string summary,
        long durationMs)
    {
        TotalRecords = totalRecords;
        PassedRecords = passedRecords;
        FailedRecords = failedRecords;
        WarningRecords = warningRecords;
        Errors = errors ?? "[]";
        Warnings = warnings ?? "[]";
        Summary = summary ?? string.Empty;
        DurationMs = durationMs;

        Status = failedRecords > 0 
            ? ValidationStatus.Failed 
            : warningRecords > 0 
                ? ValidationStatus.PassedWithWarnings 
                : ValidationStatus.Passed;
    }

    public void Fail(string errorMessage, long durationMs)
    {
        Status = ValidationStatus.Failed;
        Errors = System.Text.Json.JsonSerializer.Serialize(new[] { errorMessage });
        DurationMs = durationMs;
    }

    public double GetPassRate()
    {
        return TotalRecords > 0 ? (double)PassedRecords / TotalRecords * 100 : 0;
    }
}
