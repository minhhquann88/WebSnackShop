using Microsoft.EntityFrameworkCore;
using Npgsql;
using SnackShop.Application.Abstractions.Persistence;

namespace SnackShop.Infrastructure.Persistence;

public sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new UniqueConstraintViolationException($"Unique constraint '{pg.ConstraintName}' violated.", ex)
            {
                ConstraintName = pg.ConstraintName,
            };
        }
    }
}
