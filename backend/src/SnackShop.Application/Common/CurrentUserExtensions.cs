using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common.Authorization;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Common;

public static class CurrentUserExtensions
{
    public static readonly Error NotAuthenticated = Error.Unauthorized("Auth.NotAuthenticated", "You must be signed in.");

    /// <summary>Returns the user id, or a 401 error when the request is anonymous.</summary>
    public static Result<Guid> RequireUserId(this ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        return currentUser.UserId is { } id ? id : Result.Failure<Guid>(NotAuthenticated);
    }

    public static bool IsStaff(this ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        return currentUser.IsInRole(Roles.Staff) || currentUser.IsInRole(Roles.Admin);
    }

    /// <summary>Order actor for the current request: staff, signed-in customer, or anonymous customer.</summary>
    public static OrderActor ToOrderActor(this ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        return currentUser.IsStaff() && currentUser.UserId is { } staffId
            ? OrderActor.Staff(staffId)
            : OrderActor.Customer(currentUser.UserId);
    }
}
