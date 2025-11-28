using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;

namespace Isotyp.Application.Services;

/// <summary>
/// Service for managing local agents. Agents process data locally; cloud sees metadata only.
/// </summary>
public class AgentService : IAgentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public AgentService(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Result<AgentDto>> RegisterAsync(RegisterAgentDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Agents.GetByAgentKeyAsync(dto.AgentKey, cancellationToken);
        if (existing != null)
            return Result<AgentDto>.Failure($"Agent with key '{dto.AgentKey}' already exists.");

        var agent = Agent.Create(
            dto.Name,
            dto.Description,
            dto.AgentKey,
            dto.AgentVersion,
            dto.HostName,
            userId);

        await _unitOfWork.Agents.AddAsync(agent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create,
            nameof(Agent),
            agent.Id,
            userId,
            userId,
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(agent)),
            null,
            null,
            null,
            cancellationToken);

        return Result<AgentDto>.Success(MapToDto(agent));
    }

    public async Task<Result<AgentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(id, cancellationToken);
        if (agent == null)
            return Result<AgentDto>.Failure("Agent not found.");

        return Result<AgentDto>.Success(MapToDto(agent));
    }

    public async Task<Result<AgentDto>> GetByAgentKeyAsync(string agentKey, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByAgentKeyAsync(agentKey, cancellationToken);
        if (agent == null)
            return Result<AgentDto>.Failure("Agent not found.");

        return Result<AgentDto>.Success(MapToDto(agent));
    }

    public async Task<Result<IReadOnlyList<AgentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var agents = await _unitOfWork.Agents.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<AgentDto>>.Success(agents.Select(MapToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<AgentDto>>> GetConnectedAsync(CancellationToken cancellationToken = default)
    {
        var agents = await _unitOfWork.Agents.GetConnectedAgentsAsync(cancellationToken);
        return Result<IReadOnlyList<AgentDto>>.Success(agents.Select(MapToDto).ToList());
    }

    public async Task<Result> ConnectAsync(string agentKey, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByAgentKeyAsync(agentKey, cancellationToken);
        if (agent == null)
            return Result.Failure("Agent not found.");

        agent.Connect();
        await _unitOfWork.Agents.UpdateAsync(agent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.AgentConnected,
            nameof(Agent),
            agent.Id,
            "system",
            "Agent System",
            null,
            null,
            $"Agent connected: {agent.Name} ({agent.HostName})",
            agent.HostName,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DisconnectAsync(string agentKey, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByAgentKeyAsync(agentKey, cancellationToken);
        if (agent == null)
            return Result.Failure("Agent not found.");

        agent.Disconnect();
        await _unitOfWork.Agents.UpdateAsync(agent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.AgentDisconnected,
            nameof(Agent),
            agent.Id,
            "system",
            "Agent System",
            null,
            null,
            $"Agent disconnected: {agent.Name} ({agent.HostName})",
            agent.HostName,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> HeartbeatAsync(AgentHeartbeatDto dto, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByAgentKeyAsync(dto.AgentKey, cancellationToken);
        if (agent == null)
            return Result.Failure("Agent not found.");

        agent.RecordHeartbeat();
        await _unitOfWork.Agents.UpdateAsync(agent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AuthorizeForDataSourceAsync(AuthorizeAgentDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(dto.AgentId, cancellationToken);
        if (agent == null)
            return Result.Failure("Agent not found.");

        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(dto.DataSourceId, cancellationToken);
        if (dataSource == null)
            return Result.Failure("Data source not found.");

        agent.AuthorizeDataSource(dto.DataSourceId, userId);
        await _unitOfWork.Agents.UpdateAsync(agent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update,
            nameof(Agent),
            agent.Id,
            userId,
            userId,
            null,
            null,
            $"Authorized for data source: {dataSource.Name} ({dto.DataSourceId})",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RevokeDataSourceAuthorizationAsync(AuthorizeAgentDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(dto.AgentId, cancellationToken);
        if (agent == null)
            return Result.Failure("Agent not found.");

        agent.RevokeDataSourceAuthorization(dto.DataSourceId, userId);
        await _unitOfWork.Agents.UpdateAsync(agent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update,
            nameof(Agent),
            agent.Id,
            userId,
            userId,
            null,
            null,
            $"Revoked authorization for data source: {dto.DataSourceId}",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    private static AgentDto MapToDto(Agent entity)
    {
        return new AgentDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.AgentKey,
            entity.AgentVersion,
            entity.HostName,
            entity.IsConnected,
            entity.LastHeartbeat,
            entity.CreatedAt);
    }
}
