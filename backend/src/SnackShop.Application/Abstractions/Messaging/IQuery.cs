using MediatR;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Abstractions.Messaging;

/// <summary>A read operation. Never mutates state; handlers use IApplicationDbContext with AsNoTracking + projection.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
