namespace Isotyp.Agent.Processing;

/// <summary>
/// Interface for local data processing operations.
/// All data processing runs locally - only metadata is sent to cloud.
/// </summary>
public interface ILocalDataProcessor
{
    /// <summary>
    /// Connect to a local data source using locally-stored connection string.
    /// </summary>
    Task<bool> ConnectToDataSourceAsync(Guid dataSourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate data against the schema definition.
    /// Data never leaves the local machine - only validation results are sent to cloud.
    /// </summary>
    Task<DataValidationResult> ValidateDataAsync(
        Guid dataSourceId, 
        string schemaDefinition, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract schema metadata from a data source.
    /// Only metadata is sent to cloud - actual data stays local.
    /// </summary>
    Task<SchemaMetadata> ExtractSchemaMetadataAsync(
        Guid dataSourceId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze data patterns for AI suggestions.
    /// Only patterns and statistics are sent - no actual data.
    /// </summary>
    Task<DataPatternAnalysis> AnalyzePatternsAsync(
        Guid dataSourceId,
        string schemaDefinition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply a migration script locally.
    /// </summary>
    Task<MigrationResult> ApplyMigrationAsync(
        Guid dataSourceId,
        string migrationScript,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback a migration locally.
    /// </summary>
    Task<MigrationResult> RollbackMigrationAsync(
        Guid dataSourceId,
        string rollbackScript,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a data validation operation.
/// </summary>
public class DataValidationResult
{
    public bool Success { get; set; }
    public long TotalRecords { get; set; }
    public long PassedRecords { get; set; }
    public long FailedRecords { get; set; }
    public long WarningRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public long DurationMs { get; set; }
}

/// <summary>
/// Extracted schema metadata (no actual data).
/// </summary>
public class SchemaMetadata
{
    public List<TableMetadata> Tables { get; set; } = new();
    public DateTime ExtractedAt { get; set; }
}

/// <summary>
/// Table metadata.
/// </summary>
public class TableMetadata
{
    public string Name { get; set; } = string.Empty;
    public List<ColumnMetadata> Columns { get; set; } = new();
    public List<string> PrimaryKeys { get; set; } = new();
    public long EstimatedRowCount { get; set; }
}

/// <summary>
/// Column metadata.
/// </summary>
public class ColumnMetadata
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
}

/// <summary>
/// Data pattern analysis for AI suggestions (no actual data).
/// </summary>
public class DataPatternAnalysis
{
    public List<PatternSuggestion> Suggestions { get; set; } = new();
    public Dictionary<string, DataTypeDistribution> TypeDistributions { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Pattern-based suggestion.
/// </summary>
public class PatternSuggestion
{
    public string SuggestionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ConfidenceScore { get; set; }
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Distribution of data types/values (statistical only - no actual data).
/// </summary>
public class DataTypeDistribution
{
    public long TotalValues { get; set; }
    public long NullValues { get; set; }
    public long UniqueValues { get; set; }
    public double NullPercentage { get; set; }
}

/// <summary>
/// Result of a migration operation.
/// </summary>
public class MigrationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public DateTime ExecutedAt { get; set; }
}
