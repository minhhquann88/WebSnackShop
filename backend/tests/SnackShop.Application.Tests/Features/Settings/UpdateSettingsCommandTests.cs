using FluentValidation.TestHelper;
using NSubstitute;
using Shouldly;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Settings.UpdateSettings;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Tests.Features.Settings;

public sealed class UpdateSettingsCommandTests
{
    private static UpdateSettingsCommand ValidCommand() => new(
        ShopName: "Snack House",
        ShopPhone: "0912345678",
        ShopAddress: "123 Lê Lợi",
        FlatShippingFee: new MoneyRequest(25_000m),
        FreeShippingThreshold: new MoneyRequest(300_000m),
        BankName: "Vietcombank",
        BankAccountNumber: "0011002233",
        BankAccountName: "NGUYEN VAN A",
        BankBin: "970436",
        UnpaidBankTransferExpiryHours: 24);

    public sealed class Validator
    {
        private readonly UpdateSettingsCommandValidator _validator = new();

        [Fact]
        public void ValidCommand_Passes()
        {
            _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("12345")]
        [InlineData("091234567a")]
        public void InvalidPhone_Fails(string phone)
        {
            var command = ValidCommand() with { ShopPhone = phone };

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ShopPhone);
        }

        [Fact]
        public void NegativeShippingFee_Fails()
        {
            var command = ValidCommand() with { FlatShippingFee = new MoneyRequest(-1m) };

            _validator.TestValidate(command).ShouldHaveValidationErrorFor("FlatShippingFee.Amount");
        }

        [Fact]
        public void NullThreshold_IsAllowed()
        {
            var command = ValidCommand() with { FreeShippingThreshold = null };

            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("9704361")]
        [InlineData("97043a")]
        public void InvalidBin_Fails(string bin)
        {
            var command = ValidCommand() with { BankBin = bin };

            _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.BankBin);
        }
    }

    public sealed class Handler
    {
        [Fact]
        public async Task Handle_WhenSettingsMissing_ReturnsNotSeeded()
        {
            var repository = Substitute.For<IShopSettingsRepository>();
            repository.GetAsync(Arg.Any<CancellationToken>()).Returns((ShopSettings?)null);
            var handler = new UpdateSettingsCommandHandler(repository);

            var result = await handler.Handle(ValidCommand(), TestContext.Current.CancellationToken);

            result.Error.ShouldBe(SettingsErrors.NotSeeded);
        }

        [Fact]
        public async Task Handle_WhenValid_UpdatesEntity()
        {
            var settings = ShopSettings.CreateDefault();
            var repository = Substitute.For<IShopSettingsRepository>();
            repository.GetAsync(Arg.Any<CancellationToken>()).Returns(settings);
            var handler = new UpdateSettingsCommandHandler(repository);

            var result = await handler.Handle(ValidCommand(), TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            settings.ShopName.ShouldBe("Snack House");
            settings.FlatShippingFee.Amount.ShouldBe(25_000m);
        }

        [Fact]
        public async Task Handle_WhenDomainRejects_PropagatesError()
        {
            var settings = ShopSettings.CreateDefault();
            var repository = Substitute.For<IShopSettingsRepository>();
            repository.GetAsync(Arg.Any<CancellationToken>()).Returns(settings);
            var handler = new UpdateSettingsCommandHandler(repository);
            var command = ValidCommand() with { FreeShippingThreshold = new MoneyRequest(10_000m) };

            var result = await handler.Handle(command, TestContext.Current.CancellationToken);

            result.Error.ShouldBe(SettingsErrors.FreeShippingThresholdTooLow);
        }
    }
}
