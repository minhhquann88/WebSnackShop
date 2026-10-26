using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Auth;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.GetMe;

public sealed class GetMeQueryHandler(ICurrentUser currentUser, IIdentityService identity)
    : IQueryHandler<GetMeQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return Result.Failure<UserResponse>(userId.Error);
        }

        var account = await identity.FindByIdAsync(userId.Value, cancellationToken);
        return account is null ? Result.Failure<UserResponse>(AuthErrors.UserNotFound) : UserResponse.From(account);
    }
}
