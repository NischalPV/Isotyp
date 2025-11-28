using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;

namespace Isotyp.Application.Services;

/// <summary>
/// Service implementation for managing data sources.
/// </summary>
public class DataSourceService : IDataSourceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public DataSourceService(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Result<DataSourceDto>> CreateAsync(CreateDataSourceDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.DataSources.GetByNameAsync(dto.Name, cancellationToken);
        if (existing != null)
            return Result<DataSourceDto>.Failure($"Data source with name '{dto.Name}' already exists.");

        var dataSource = DataSource.Create(
            dto.Name,
            dto.Description,
            dto.Type,
            dto.ConnectionStringReference,
            userId);

        await _unitOfWork.DataSources.AddAsync(dataSource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create,
            nameof(DataSource),
            dataSource.Id,
            userId,
            userId,
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource)),
            null,
            null,
            null,
            cancellationToken);

        return Result<DataSourceDto>.Success(MapToDto(dataSource));
    }

    public async Task<Result<DataSourceDto>> UpdateAsync(Guid id, UpdateDataSourceDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(id, cancellationToken);
        if (dataSource == null)
            return Result<DataSourceDto>.Failure("Data source not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource));

        dataSource.Update(dto.Name, dto.Description, userId);
        await _unitOfWork.DataSources.UpdateAsync(dataSource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update,
            nameof(DataSource),
            dataSource.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource)),
            null,
            null,
            null,
            cancellationToken);

        return Result<DataSourceDto>.Success(MapToDto(dataSource));
    }

    public async Task<Result<DataSourceDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(id, cancellationToken);
        if (dataSource == null)
            return Result<DataSourceDto>.Failure("Data source not found.");

        return Result<DataSourceDto>.Success(MapToDto(dataSource));
    }

    public async Task<Result<IReadOnlyList<DataSourceDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var dataSources = await _unitOfWork.DataSources.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<DataSourceDto>>.Success(dataSources.Select(MapToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<DataSourceDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var dataSources = await _unitOfWork.DataSources.GetActiveDataSourcesAsync(cancellationToken);
        return Result<IReadOnlyList<DataSourceDto>>.Success(dataSources.Select(MapToDto).ToList());
    }

    public async Task<Result> ActivateAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(id, cancellationToken);
        if (dataSource == null)
            return Result.Failure("Data source not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource));

        dataSource.Activate(userId);
        await _unitOfWork.DataSources.UpdateAsync(dataSource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update,
            nameof(DataSource),
            dataSource.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource)),
            "Activated",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(id, cancellationToken);
        if (dataSource == null)
            return Result.Failure("Data source not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource));

        dataSource.Deactivate(userId);
        await _unitOfWork.DataSources.UpdateAsync(dataSource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update,
            nameof(DataSource),
            dataSource.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource)),
            "Deactivated",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(id, cancellationToken);
        if (dataSource == null)
            return Result.Failure("Data source not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(dataSource));

        dataSource.MarkAsDeleted(userId);
        await _unitOfWork.DataSources.UpdateAsync(dataSource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Delete,
            nameof(DataSource),
            dataSource.Id,
            userId,
            userId,
            stateBefore,
            null,
            null,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    private static DataSourceDto MapToDto(DataSource entity)
    {
        return new DataSourceDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Type,
            entity.IsActive,
            entity.LastConnectedAt,
            entity.LastValidatedAt,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.Version);
    }
}
