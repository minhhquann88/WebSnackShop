using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Users.ListUsers;

public sealed class ListUsersQueryHandler(IIdentityService identity)
    : IQueryHandler<ListUsersQuery, PagedResponse<AdminUserResponse>>
{
    public async Task<Result<PagedResponse<AdminUserResponse>>> Handle(ListUsersQuery query, CancellationToken cancellationToken)
    {
        var page = await identity.ListUsersAsync(query.Role, query.Search, query.Page, query.PageSize, cancellationToken);
        return new PagedResponse<AdminUserResponse>(
            page.Items.Select(AdminUserResponse.From).ToList(), page.Page, page.PageSize, page.TotalCount);
    }
}
