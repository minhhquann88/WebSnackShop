using Shouldly;
using SnackShop.Domain.Customers;

namespace SnackShop.Domain.Tests.Customers;

public sealed class CustomerAndCartTests
{
    private static Customer NewCustomer() => Customer.Create(Guid.CreateVersion7(), " Nguyễn A ", "A@Mail.com", null);

    [Fact]
    public void Create_NormalisesNameAndEmail()
    {
        var customer = NewCustomer();

        customer.FullName.ShouldBe("Nguyễn A");
        customer.Email.ShouldBe("a@mail.com");
    }

    [Fact]
    public void AddAddress_FirstBecomesDefault_NewDefaultReplacesOld()
    {
        var customer = NewCustomer();
        var first = customer.AddAddress("Nhà", TestData.Address(), makeDefault: false).Value;
        first.IsDefault.ShouldBeTrue();

        var second = customer.AddAddress("Công ty", TestData.Address(), makeDefault: true).Value;

        second.IsDefault.ShouldBeTrue();
        first.IsDefault.ShouldBeFalse();
        customer.Addresses.Count(a => a.IsDefault).ShouldBe(1);
    }

    [Fact]
    public void AddAddress_BeyondMax_Fails()
    {
        var customer = NewCustomer();
        for (var i = 0; i < Customer.MaxAddresses; i++)
        {
            customer.AddAddress(null, TestData.Address(), false).IsSuccess.ShouldBeTrue();
        }

        customer.AddAddress(null, TestData.Address(), false).Error.ShouldBe(CustomerErrors.TooManyAddresses);
    }

    [Fact]
    public void RemoveAddress_Default_PromotesAnother()
    {
        var customer = NewCustomer();
        var first = customer.AddAddress(null, TestData.Address(), false).Value;
        var second = customer.AddAddress(null, TestData.Address(), false).Value;

        customer.RemoveAddress(first.Id).IsSuccess.ShouldBeTrue();

        second.IsDefault.ShouldBeTrue();
        customer.RemoveAddress(first.Id).Error.ShouldBe(CustomerErrors.AddressNotFound);
    }

    [Fact]
    public void Cart_AddItem_SumsAndCapsQuantity()
    {
        var cart = Cart.CreateFor(Guid.CreateVersion7(), TestData.Now);
        var variant = Guid.CreateVersion7();

        cart.AddItem(variant, 60, TestData.Now).IsSuccess.ShouldBeTrue();
        cart.AddItem(variant, 60, TestData.Now).IsSuccess.ShouldBeTrue();

        cart.Items.Single().Quantity.ShouldBe(Cart.MaxQuantityPerItem);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Cart_InvalidQuantity_Fails(int quantity)
    {
        var cart = Cart.CreateFor(Guid.CreateVersion7(), TestData.Now);

        cart.AddItem(Guid.CreateVersion7(), quantity, TestData.Now).Error.ShouldBe(CustomerErrors.InvalidCartQuantity);
    }

    [Fact]
    public void Cart_Merge_CombinesWithExisting_AndSkipsInvalidLines()
    {
        var cart = Cart.CreateFor(Guid.CreateVersion7(), TestData.Now);
        var a = Guid.CreateVersion7();
        var b = Guid.CreateVersion7();
        cart.AddItem(a, 1, TestData.Now);

        cart.Merge([(a, 2), (b, 3), (Guid.CreateVersion7(), 0)], TestData.Now);

        cart.Items.Count.ShouldBe(2);
        cart.Items.Single(i => i.VariantId == a).Quantity.ShouldBe(3);
        cart.Items.Single(i => i.VariantId == b).Quantity.ShouldBe(3);
    }

    [Fact]
    public void Cart_RemoveVariants_AfterCheckout()
    {
        var cart = Cart.CreateFor(Guid.CreateVersion7(), TestData.Now);
        var a = Guid.CreateVersion7();
        var b = Guid.CreateVersion7();
        cart.AddItem(a, 1, TestData.Now);
        cart.AddItem(b, 1, TestData.Now);

        cart.RemoveVariants([a], TestData.Now);

        cart.Items.Single().VariantId.ShouldBe(b);
    }

    [Fact]
    public void Cart_SetQuantity_UnknownVariant_Fails()
    {
        var cart = Cart.CreateFor(Guid.CreateVersion7(), TestData.Now);

        cart.SetQuantity(Guid.CreateVersion7(), 2, TestData.Now).Error.ShouldBe(CustomerErrors.CartItemNotFound);
        cart.RemoveItem(Guid.CreateVersion7(), TestData.Now).Error.ShouldBe(CustomerErrors.CartItemNotFound);
    }
}
