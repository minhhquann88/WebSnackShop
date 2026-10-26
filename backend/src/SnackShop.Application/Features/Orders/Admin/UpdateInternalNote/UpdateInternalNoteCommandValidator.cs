using FluentValidation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.UpdateInternalNote;

public sealed class UpdateInternalNoteCommandValidator : AbstractValidator<UpdateInternalNoteCommand>
{
    public UpdateInternalNoteCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(Order.MaxInternalNoteLength);
    }
}
