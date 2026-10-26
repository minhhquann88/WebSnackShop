using Shouldly;
using SnackShop.Domain.Inventory;

namespace SnackShop.Domain.Tests.Inventory;

public sealed class StockItemTests
{
    private static StockItem WithStock(int onHand)
    {
        var stock = StockItem.CreateFor(Guid.CreateVersion7());
        if (onHand > 0)
        {
            stock.Receive(onHand, "init", TestData.Ctx);
        }

        return stock;
    }

    [Fact]
    public void Receive_IncreasesOnHand_AndRecordsPurchase()
    {
        var stock = WithStock(10);

        stock.QuantityOnHand.ShouldBe(10);
        stock.Available.ShouldBe(10);
        stock.Movements.Single().Type.ShouldBe(StockMovementType.Purchase);
        stock.Movements.Single().OnHandAfter.ShouldBe(10);
    }

    [Fact]
    public void Reserve_ReducesAvailable_NotOnHand()
    {
        var stock = WithStock(10);

        stock.Reserve(3, "SKU", Guid.CreateVersion7(), TestData.Ctx).IsSuccess.ShouldBeTrue();

        stock.QuantityOnHand.ShouldBe(10);
        stock.QuantityReserved.ShouldBe(3);
        stock.Available.ShouldBe(7);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_FailsWithInsufficientStock()
    {
        var stock = WithStock(2);
        stock.Reserve(2, "SKU", Guid.CreateVersion7(), TestData.Ctx);

        var result = stock.Reserve(1, "SKU", Guid.CreateVersion7(), TestData.Ctx);

        result.Error.Code.ShouldBe("Inventory.InsufficientStock");
        stock.QuantityReserved.ShouldBe(2);
    }

    [Fact]
    public void CommitReservation_RemovesFromBothCounters()
    {
        var stock = WithStock(10);
        var orderId = Guid.CreateVersion7();
        stock.Reserve(4, "SKU", orderId, TestData.Ctx);

        stock.CommitReservation(4, orderId, TestData.Ctx).IsSuccess.ShouldBeTrue();

        stock.QuantityOnHand.ShouldBe(6);
        stock.QuantityReserved.ShouldBe(0);
        stock.Movements.Last().Type.ShouldBe(StockMovementType.Sale);
        stock.Movements.Last().Quantity.ShouldBe(-4);
    }

    [Fact]
    public void ReleaseReservation_RestoresAvailability()
    {
        var stock = WithStock(10);
        var orderId = Guid.CreateVersion7();
        stock.Reserve(4, "SKU", orderId, TestData.Ctx);

        stock.ReleaseReservation(4, orderId, TestData.Ctx).IsSuccess.ShouldBeTrue();

        stock.Available.ShouldBe(10);
        stock.Movements.Last().Type.ShouldBe(StockMovementType.ReservationRelease);
    }

    [Fact]
    public void ReleaseOrCommit_MoreThanReserved_Fails()
    {
        var stock = WithStock(10);
        var orderId = Guid.CreateVersion7();
        stock.Reserve(1, "SKU", orderId, TestData.Ctx);

        stock.ReleaseReservation(2, orderId, TestData.Ctx).Error.ShouldBe(InventoryErrors.ReservationMismatch);
        stock.CommitReservation(2, orderId, TestData.Ctx).Error.ShouldBe(InventoryErrors.ReservationMismatch);
    }

    [Fact]
    public void Return_AddsBackToOnHand()
    {
        var stock = WithStock(5);

        stock.Return(2, Guid.CreateVersion7(), TestData.Ctx).IsSuccess.ShouldBeTrue();

        stock.QuantityOnHand.ShouldBe(7);
        stock.Movements.Last().Type.ShouldBe(StockMovementType.Return);
    }

    [Fact]
    public void Adjust_BelowReserved_Fails()
    {
        var stock = WithStock(5);
        stock.Reserve(4, "SKU", Guid.CreateVersion7(), TestData.Ctx);

        stock.Adjust(-2, "count", TestData.Ctx).Error.ShouldBe(InventoryErrors.AdjustmentBelowReserved);
        stock.Adjust(-1, "count", TestData.Ctx).IsSuccess.ShouldBeTrue();
        stock.QuantityOnHand.ShouldBe(4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Receive_NonPositive_Fails(int quantity)
    {
        WithStock(0).Receive(quantity, null, TestData.Ctx).Error.ShouldBe(InventoryErrors.InvalidQuantity);
    }

    [Fact]
    public void IsLowStock_UsesAvailableAgainstThreshold()
    {
        var stock = WithStock(12);
        stock.SetLowStockThreshold(10);
        stock.IsLowStock.ShouldBeFalse();

        stock.Reserve(2, "SKU", Guid.CreateVersion7(), TestData.Ctx);

        stock.IsLowStock.ShouldBeTrue();
        stock.SetLowStockThreshold(-1).Error.ShouldBe(InventoryErrors.InvalidThreshold);
    }

    [Fact]
    public void Movements_RecordWhoAndWhen()
    {
        var stock = WithStock(1);

        stock.Movements.Single().CreatedAt.ShouldBe(TestData.Now);
        stock.Movements.Single().CreatedBy.ShouldBe(TestData.Ctx.UserId);
    }
}
