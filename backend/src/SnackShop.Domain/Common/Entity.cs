namespace SnackShop.Domain.Common;

/// <summary>
/// Base class for entities identified by a <see cref="Guid"/> (version 7, created in the factory).
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>EF Core constructor.</summary>
    protected Entity()
    {
    }

    public Guid Id { get; private init; }

    public bool Equals(Entity? other) => other is not null && other.GetType() == GetType() && other.Id == Id;

    public override bool Equals(object? obj) => obj is Entity other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
