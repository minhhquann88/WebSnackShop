using FluentValidation;
using Shouldly;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Behaviors;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record SampleCommand(string Name, int Quantity) : ICommand<Guid>;

    private sealed class SampleValidator : AbstractValidator<SampleCommand>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Quantity).InclusiveBetween(1, 99);
        }
    }

    [Fact]
    public async Task Handle_WhenValid_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<Guid>>([new SampleValidator()]);
        var nextCalled = false;
        var expected = Guid.CreateVersion7();

        var result = await behavior.Handle(
            new SampleCommand("ok", 5),
            _ => { nextCalled = true; return Task.FromResult<Result<Guid>>(expected); },
            TestContext.Current.CancellationToken);

        nextCalled.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WhenInvalid_ShortCircuitsWithValidationError()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<Guid>>([new SampleValidator()]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new SampleCommand(string.Empty, 0),
            _ => { nextCalled = true; return Task.FromResult<Result<Guid>>(Guid.Empty); },
            TestContext.Current.CancellationToken);

        nextCalled.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Errors.Keys.ShouldBe(["name", "quantity"], ignoreOrder: true);
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<Guid>>([]);

        var result = await behavior.Handle(
            new SampleCommand(string.Empty, 0),
            _ => Task.FromResult<Result<Guid>>(Guid.Empty),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData("ShippingAddress.Phone", "shippingAddress.phone")]
    [InlineData("Items[0].Quantity", "items[0].quantity")]
    [InlineData("name", "name")]
    [InlineData("", "")]
    public void ToCamelCasePath_ConvertsEachSegment(string input, string expected)
    {
        ValidationBehavior<SampleCommand, Result>.ToCamelCasePath(input).ShouldBe(expected);
    }
}
