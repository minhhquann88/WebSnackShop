namespace SnackShop.Application.Abstractions.Persistence;

/// <summary>
/// Thrown by <see cref="IUnitOfWork"/> when a commit hits a unique index (provider-neutral wrapper of the database
/// error). Usually two concurrent requests creating the same thing; the behavior maps it to a 409 the client can retry.
/// </summary>
public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException()
    {
    }

    public UniqueConstraintViolationException(string message)
        : base(message)
    {
    }

    public UniqueConstraintViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public string? ConstraintName { get; init; }
}
