using BuildingBlocks.EFCore;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Outbox.EF;

public class EfInboxService : IInboxService
{
    private readonly IDbContext _dbContext;

    public EfInboxService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        => _dbContext.OutboxMessages.AnyAsync(x => x.Id == eventId, cancellationToken);
}
