namespace SnackShop.Domain.Common;

/// <summary>
/// Audit columns filled by the persistence layer (AuditableEntityInterceptor). Never set by hand.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }

    Guid? CreatedBy { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }

    Guid? UpdatedBy { get; set; }
}
