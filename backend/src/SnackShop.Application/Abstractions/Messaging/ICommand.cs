using MediatR;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Abstractions.Messaging;

/// <summary>Marker shared by all commands so pipeline behaviors (UnitOfWork) can target writes only.</summary>
public interface IBaseCommand;

/// <summary>A write operation with no return value beyond success/failure.</summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>A write operation returning <typeparamref name="TResponse"/> (usually an id or a minimal response).</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;
