using System.Data;
using System.Reflection;
using BuildingBlocks.Outbox;
using BuildingBlocks.Outbox.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.EFCore;

public abstract class AppDbContextBase : DbContext, IDbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private IDbContextTransaction _currentTransaction;

    protected AppDbContextBase(DbContextOptions options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        _currentTransaction ??= await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            OnBeforeSaving();
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    // https://www.meziantou.net/entity-framework-core-generate-tracking-columns.htm
    // https://www.meziantou.net/entity-framework-core-soft-delete-using-query-filters.htm
    private void OnBeforeSaving()
    {
        var now = DateTime.Now;
        //var userId = Convert.ToInt32(_httpContextAccessor?.HttpContext?.User?.Identity?.GetSubjectId());

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.CurrentValues["LastModified"] = now;
                    // entry.CurrentValues["LastModifiedBy"] = userId;
                    break;
                case EntityState.Added:
                    entry.CurrentValues["LastModified"] = now;
                    // entry.CurrentValues["LastModifiedBy"] = userId;
                    break;
            }
        }
    }
}
