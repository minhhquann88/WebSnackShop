using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.Addresses.ListMyAddresses;

public sealed class ListMyAddressesQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IQueryHandler<ListMyAddressesQuery, IReadOnlyList<SavedAddressResponse>>
{
    public async Task<Result<IReadOnlyList<SavedAddressResponse>>> Handle(ListMyAddressesQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return Result.Failure<IReadOnlyList<SavedAddressResponse>>(userId.Error);
        }

        var addresses = await db.Customers.AsNoTracking()
            .Where(c => c.Id == userId.Value)
            .SelectMany(c => c.Addresses)
            .OrderByDescending(a => a.IsDefault).ThenBy(a => a.Id)
            .Select(a => new SavedAddressResponse(a.Id, a.Label, a.FullName, a.Phone, a.Line1, a.Ward, a.District, a.Province, a.IsDefault))
            .ToListAsync(cancellationToken);

        return addresses;
    }
}
