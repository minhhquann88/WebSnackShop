using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.UpdateInternalNote;

/// <summary>Staff-only note on an order (never shown to the customer). Null clears it.</summary>
public sealed record UpdateInternalNoteCommand(Guid OrderId, string? Note) : ICommand;
