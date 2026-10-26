namespace SnackShop.Application.Common;

/// <summary>Implemented by list queries; validators call <see cref="Validation.ValidationRules.ValidatePaging{T}"/>.</summary>
public interface IPagedQuery
{
    int Page { get; }

    int PageSize { get; }
}
