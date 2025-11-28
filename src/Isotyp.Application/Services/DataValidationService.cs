using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;

namespace Isotyp.Application.Services;

/// <summary>
/// Service for data validation operations. Validates data against canonical schema.
/// </summary>
public class DataValidationService : IDataValidationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public DataValidationService(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Result<DataValidationDto>> CreateAsync(CreateDataValidationDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(dto.DataSourceId, cancellationToken);
        if (dataSource == null)
            return Result<DataValidationDto>.Failure("Data source not found.");

        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(dto.SchemaVersionId, cancellationToken);
        if (schemaVersion == null)
            return Result<DataValidationDto>.Failure("Schema version not found.");

        if (dto.AgentId.HasValue)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(dto.AgentId.Value, cancellationToken);
            if (agent == null)
                return Result<DataValidationDto>.Failure("Agent not found.");
        }

        var validation = DataValidation.Create(
            dto.DataSourceId,
            dto.SchemaVersionId,
            dto.AgentId,
            userId);

        await _unitOfWork.DataValidations.AddAsync(validation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.DataValidationExecuted,
            nameof(DataValidation),
            validation.Id,
            userId,
            userId,
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(validation)),
            "Validation created",
            null,
            null,
            cancellationToken);

        return Result<DataValidationDto>.Success(MapToDto(validation));
    }

    public async Task<Result<DataValidationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var validation = await _unitOfWork.DataValidations.GetByIdAsync(id, cancellationToken);
        if (validation == null)
            return Result<DataValidationDto>.Failure("Data validation not found.");

        return Result<DataValidationDto>.Success(MapToDto(validation));
    }

    public async Task<Result<IReadOnlyList<DataValidationDto>>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        var validations = await _unitOfWork.DataValidations.GetByDataSourceIdAsync(dataSourceId, cancellationToken);
        return Result<IReadOnlyList<DataValidationDto>>.Success(validations.Select(MapToDto).ToList());
    }

    public async Task<Result<DataValidationDto?>> GetLatestByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        var validation = await _unitOfWork.DataValidations.GetLatestByDataSourceIdAsync(dataSourceId, cancellationToken);
        return Result<DataValidationDto?>.Success(validation != null ? MapToDto(validation) : null);
    }

    public async Task<Result> StartAsync(Guid validationId, CancellationToken cancellationToken = default)
    {
        var validation = await _unitOfWork.DataValidations.GetByIdAsync(validationId, cancellationToken);
        if (validation == null)
            return Result.Failure("Data validation not found.");

        validation.Start();
        await _unitOfWork.DataValidations.UpdateAsync(validation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CompleteAsync(CompleteDataValidationDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _unitOfWork.DataValidations.GetByIdAsync(dto.ValidationId, cancellationToken);
        if (validation == null)
            return Result.Failure("Data validation not found.");

        validation.Complete(
            dto.TotalRecords,
            dto.PassedRecords,
            dto.FailedRecords,
            dto.WarningRecords,
            dto.Errors,
            dto.Warnings,
            dto.Summary,
            dto.DurationMs);

        await _unitOfWork.DataValidations.UpdateAsync(validation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update data source last validated time
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(validation.DataSourceId, cancellationToken);
        if (dataSource != null)
        {
            dataSource.RecordValidation(DateTime.UtcNow);
            await _unitOfWork.DataSources.UpdateAsync(dataSource, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await _auditService.LogAsync(
            AuditAction.DataValidationExecuted,
            nameof(DataValidation),
            validation.Id,
            "system",
            "Validation System",
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(validation)),
            $"Completed: {validation.Status}, Pass rate: {validation.GetPassRate():F2}%",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> FailAsync(Guid validationId, string errorMessage, long durationMs, CancellationToken cancellationToken = default)
    {
        var validation = await _unitOfWork.DataValidations.GetByIdAsync(validationId, cancellationToken);
        if (validation == null)
            return Result.Failure("Data validation not found.");

        validation.Fail(errorMessage, durationMs);
        await _unitOfWork.DataValidations.UpdateAsync(validation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.DataValidationExecuted,
            nameof(DataValidation),
            validation.Id,
            "system",
            "Validation System",
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(validation)),
            $"Failed: {errorMessage}",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    private static DataValidationDto MapToDto(DataValidation entity)
    {
        return new DataValidationDto(
            entity.Id,
            entity.DataSourceId,
            entity.SchemaVersionId,
            entity.AgentId,
            entity.Status,
            entity.ExecutedAt,
            entity.DurationMs,
            entity.TotalRecords,
            entity.PassedRecords,
            entity.FailedRecords,
            entity.WarningRecords,
            entity.Errors,
            entity.Warnings,
            entity.Summary,
            entity.GetPassRate(),
            entity.CreatedAt);
    }
}
