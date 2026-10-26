using FluentValidation;

namespace SnackShop.Application.Features.Cart.ClearCart;

/// <summary>No input to validate; present because every command has a validator (architecture rule).</summary>
public sealed class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>;
