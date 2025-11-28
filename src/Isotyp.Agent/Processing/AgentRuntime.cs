using Isotyp.Application.DTOs;
using Isotyp.Core.Enums;

namespace Isotyp.Agent.Processing;

/// <summary>
/// Main agent runtime that coordinates local data processing and cloud communication.
/// All data processing runs locally - cloud sees metadata only.
/// </summary>
public class AgentRuntime
{
    private readonly AgentConfiguration _configuration;
    private readonly ILocalDataProcessor _dataProcessor;
    private readonly ICloudApiClient _cloudApiClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;

    public AgentRuntime(
        AgentConfiguration configuration,
        ILocalDataProcessor dataProcessor,
        ICloudApiClient cloudApiClient)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dataProcessor = dataProcessor ?? throw new ArgumentNullException(nameof(dataProcessor));
        _cloudApiClient = cloudApiClient ?? throw new ArgumentNullException(nameof(cloudApiClient));
    }

    /// <summary>
    /// Start the agent runtime.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            throw new InvalidOperationException("Agent is already running.");

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isRunning = true;

        // Connect to cloud
        var connected = await _cloudApiClient.ConnectAsync(_configuration.AgentKey, _cancellationTokenSource.Token);
        if (!connected)
            throw new InvalidOperationException("Failed to connect to cloud API.");

        // Start background tasks
        _ = HeartbeatLoopAsync(_cancellationTokenSource.Token);
        _ = ValidationLoopAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stop the agent runtime.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
            return;

        _cancellationTokenSource?.Cancel();
        
        // Disconnect from cloud
        await _cloudApiClient.DisconnectAsync(_configuration.AgentKey);
        
        _isRunning = false;
    }

    /// <summary>
    /// Heartbeat loop - sends periodic heartbeats to cloud.
    /// </summary>
    private async Task HeartbeatLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _cloudApiClient.SendHeartbeatAsync(
                    new AgentHeartbeatDto(
                        _configuration.AgentKey, 
                        _configuration.AgentVersion, 
                        DateTime.UtcNow),
                    cancellationToken);
            }
            catch (Exception)
            {
                // Log error but continue
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_configuration.HeartbeatIntervalSeconds), 
                cancellationToken);
        }
    }

    /// <summary>
    /// Validation loop - periodically validates data against schema.
    /// </summary>
    private async Task ValidationLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RunValidationsAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Log error but continue
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_configuration.ValidationIntervalSeconds), 
                cancellationToken);
        }
    }

    /// <summary>
    /// Run validations for all authorized data sources.
    /// </summary>
    public async Task RunValidationsAsync(CancellationToken cancellationToken = default)
    {
        var dataSources = await _cloudApiClient.GetAuthorizedDataSourcesAsync(
            _configuration.AgentKey, cancellationToken);

        foreach (var dataSource in dataSources)
        {
            await ValidateDataSourceAsync(dataSource.Id, cancellationToken);
        }
    }

    /// <summary>
    /// Validate a specific data source against its schema.
    /// Data is processed locally - only validation results go to cloud.
    /// </summary>
    public async Task ValidateDataSourceAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        // Get the latest schema version from cloud
        var schemaVersion = await _cloudApiClient.GetLatestSchemaVersionAsync(dataSourceId, cancellationToken);
        if (schemaVersion == null)
            return;

        // Create validation record in cloud
        var validation = await _cloudApiClient.CreateValidationAsync(
            new CreateDataValidationDto(dataSourceId, schemaVersion.Id, null),
            "agent-system",
            cancellationToken);

        if (validation == null)
            return;

        // Perform local validation (data never leaves local machine)
        var result = await _dataProcessor.ValidateDataAsync(
            dataSourceId, 
            schemaVersion.SchemaDefinition, 
            cancellationToken);

        // Send only the results to cloud (no actual data)
        await _cloudApiClient.CompleteValidationAsync(
            new CompleteDataValidationDto(
                validation.Id,
                result.TotalRecords,
                result.PassedRecords,
                result.FailedRecords,
                result.WarningRecords,
                System.Text.Json.JsonSerializer.Serialize(result.Errors),
                System.Text.Json.JsonSerializer.Serialize(result.Warnings),
                result.Summary,
                result.DurationMs),
            cancellationToken);
    }

    /// <summary>
    /// Analyze data patterns and report AI suggestions.
    /// Note: AI suggestions are never auto-applied - they require human approval.
    /// </summary>
    public async Task AnalyzeAndSuggestAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _cloudApiClient.GetLatestSchemaVersionAsync(dataSourceId, cancellationToken);
        if (schemaVersion == null)
            return;

        // Analyze patterns locally (data never leaves local machine)
        var analysis = await _dataProcessor.AnalyzePatternsAsync(
            dataSourceId,
            schemaVersion.SchemaDefinition,
            cancellationToken);

        // Report suggestions to cloud (they will require human approval)
        foreach (var suggestion in analysis.Suggestions)
        {
            await _cloudApiClient.ReportAiSuggestionAsync(
                new CreateAiSuggestionDto(
                    dataSourceId,
                    schemaVersion.Id,
                    ParseChangeType(suggestion.SuggestionType),
                    suggestion.Details,
                    suggestion.Description,
                    suggestion.ConfidenceScore,
                    "Pattern analysis"),
                cancellationToken);
        }
    }

    private static ChangeType ParseChangeType(string suggestionType)
    {
        return suggestionType.ToLowerInvariant() switch
        {
            "add_column" => ChangeType.AddColumn,
            "add_index" => ChangeType.AddIndex,
            "modify_column" => ChangeType.ModifyColumn,
            "add_relationship" => ChangeType.AddRelationship,
            _ => ChangeType.AddColumn
        };
    }
}
