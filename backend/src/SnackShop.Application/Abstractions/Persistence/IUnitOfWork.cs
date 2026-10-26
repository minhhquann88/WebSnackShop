namespace SnackShop.Application.Abstractions.Persistence;

/// <summary>
/// Commits the current transaction. Called by UnitOfWorkBehavior after a successful command handler; handlers never call it.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
