using Microsoft.EntityFrameworkCore;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure.Data;

namespace Isotyp.Infrastructure.Repositories;

public class SchemaChangeRequestRepository : Repository<SchemaChangeRequest>, ISchemaChangeRequestRepository
{
    public SchemaChangeRequestRepository(IsotypDbContext context) : base(context)
    {
    }

    public override async Task<SchemaChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Approvals)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SchemaChangeRequest>> GetBySchemaVersionIdAsync(Guid schemaVersionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Approvals)
            .Where(r => r.SchemaVersionId == schemaVersionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SchemaChangeRequest>> GetByStatusAsync(ApprovalStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Approvals)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SchemaChangeRequest>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Approvals)
            .Where(r => r.Status == ApprovalStatus.Submitted ||
                       r.Status == ApprovalStatus.TechnicalApproved ||
                       r.Status == ApprovalStatus.BusinessApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SchemaChangeRequest>> GetAiSuggestedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Approvals)
            .Where(r => r.IsAiSuggested)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
