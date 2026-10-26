using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Users.ListUsers;

/// <summary>Accounts filtered by role (e.g. "Staff" to manage employees) and name/email search.</summary>
public sealed record ListUsersQuery(
    string? Role = null,
    string? Search = null,
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<AdminUserResponse>>, IPagedQuery;
