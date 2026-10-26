using MediatR;
using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Behaviors;

/// <summary>
/// Commits the unit of work after a successful command handler. Queries and failed commands commit nothing.
/// Concurrency conflicts become a 409 <see cref="Error"/> instead of an exception.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result
{
    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Concurrency.Conflict",
        "The record was modified by someone else. Reload and try again.");

    public static readonly Error DuplicateRequest = Error.Conflict(
        "Concurrency.DuplicateRequest", "The same data was created by a parallel request. Retry the request.");

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (response.IsFailure)
        {
            return response;
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ResultFactory.CreateFailure<TResponse>(ConcurrencyConflict);
        }
        catch (UniqueConstraintViolationException)
        {
            // e.g. two checkouts with the same Idempotency-Key, or two first requests creating the same cart.
            // A retry finds the row the other request created.
            return ResultFactory.CreateFailure<TResponse>(DuplicateRequest);
        }

        return response;
    }
}
