namespace SnackShop.Domain.Common;

/// <summary>
/// Marks catalog entities that are hidden instead of removed. A global query filter excludes deleted rows.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTimeOffset? DeletedAt { get; set; }
}
