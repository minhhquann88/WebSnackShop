using Shouldly;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Domain.Tests.Ordering;

/// <summary>Covers every row of the transition table in docs/03-domain-model.md §5.</summary>
public sealed class OrderTests
{
    private static readonly OrderActor Customer = OrderActor.Customer(Guid.CreateVersion7());

    private static Order InStatus(OrderStatus status, PaymentMethod method = PaymentMethod.Cod)
    {
        var order = TestData.Order(method);
        var now = TestData.Now;
        if (status == OrderStatus.Pending)
        {
            return order;
        }

        if (status == OrderStatus.Cancelled)
        {
            order.Cancel(TestData.Staff, "test", now);
            return order;
        }

        order.Confirm(TestData.Staff, now, markPaid: true, "REF").IsSuccess.ShouldBeTrue();
        if (status >= OrderStatus.Preparing)
        {
            order.StartPreparing(TestData.Staff, now).IsSuccess.ShouldBeTrue();
        }

        if (status >= OrderStatus.Shipping)
        {
            order.Ship(TestData.Staff, null, now).IsSuccess.ShouldBeTrue();
        }

        if (status >= OrderStatus.Delivered)
        {
            order.MarkDelivered(TestData.Staff, now).IsSuccess.ShouldBeTrue();
        }

        return order;
    }

    [Fact]
    public void Place_ComputesTotals_SnapshotsLines_AndRaisesEvent()
    {
        var order = TestData.Order(PaymentMethod.Cod, TestData.Line(2, 50_000m), TestData.Line(1, 30_000m));

        order.Status.ShouldBe(OrderStatus.Pending);
        order.Subtotal.ShouldBe(Money.Vnd(130_000m));
        order.ShippingFee.ShouldBe(Money.Vnd(25_000m));
        order.Total.ShouldBe(Money.Vnd(155_000m));
        order.Payment.Amount.ShouldBe(order.Total);
        order.Payment.Status.ShouldBe(PaymentStatus.Unpaid);
        order.ContactEmail.ShouldBe("a@example.com");
        order.StatusHistory.Single().ToStatus.ShouldBe(OrderStatus.Pending);
        order.DomainEvents.OfType<OrderPlacedDomainEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Place_BankTransfer_SetsTransferContentFromOrderNumber()
    {
        var order = TestData.Order(PaymentMethod.BankTransfer);

        order.OrderNumber.Value.ShouldBe("SS-20261022-00042");
        order.Payment.TransferContent.ShouldBe("SS2026102200042");
    }

    [Fact]
    public void Place_WithoutLines_Fails()
    {
        var data = new PlaceOrderData(OrderNumber.Create(TestData.Now, 1), null, "a@b.vn", TestData.Address(), PaymentMethod.Cod, null, null);

        Order.Place(data, [], Money.Zero(), TestData.Now).Error.ShouldBe(OrderErrors.EmptyOrder);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Place_WithQuantityOutOfRange_Fails(int quantity)
    {
        var data = new PlaceOrderData(OrderNumber.Create(TestData.Now, 1), null, "a@b.vn", TestData.Address(), PaymentMethod.Cod, null, null);

        Order.Place(data, [TestData.Line(quantity)], Money.Zero(), TestData.Now).Error.ShouldBe(OrderErrors.InvalidQuantity);
    }

    [Fact]
    public void OrderNumber_UsesVietnamDate()
    {
        // 18:00 UTC on 21 Oct is 01:00 on 22 Oct in Vietnam.
        OrderNumber.Create(new DateTimeOffset(2026, 10, 21, 18, 0, 0, TimeSpan.Zero), 7).Value.ShouldBe("SS-20261022-00007");
    }

    [Fact]
    public void Confirm_Cod_FromPending_Succeeds()
    {
        var order = TestData.Order();

        order.Confirm(TestData.Staff, TestData.Now, markPaid: false, null).IsSuccess.ShouldBeTrue();

        order.Status.ShouldBe(OrderStatus.Confirmed);
        order.ConfirmedAt.ShouldBe(TestData.Now);
        order.Payment.Status.ShouldBe(PaymentStatus.Unpaid);
        order.DomainEvents.OfType<OrderConfirmedDomainEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Confirm_UnpaidBankTransfer_RequiresPayment()
    {
        var order = TestData.Order(PaymentMethod.BankTransfer);

        order.Confirm(TestData.Staff, TestData.Now, markPaid: false, null).Error.ShouldBe(OrderErrors.PaymentRequired);
        order.Status.ShouldBe(OrderStatus.Pending);

        order.Confirm(TestData.Staff, TestData.Now, markPaid: true, "FT123").IsSuccess.ShouldBeTrue();
        order.Payment.Status.ShouldBe(PaymentStatus.Paid);
        order.Payment.TransactionRef.ShouldBe("FT123");
        order.Payment.ConfirmedByUserId.ShouldBe(TestData.Staff.UserId);
    }

    [Fact]
    public void Confirm_BankTransferAlreadyPaid_DoesNotNeedFlag()
    {
        var order = TestData.Order(PaymentMethod.BankTransfer);
        order.MarkPaid(TestData.Staff, "FT1", TestData.Now, TestData.Now).IsSuccess.ShouldBeTrue();

        order.Confirm(TestData.Staff, TestData.Now, markPaid: false, null).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void HappyPath_PendingToDelivered_RecordsEveryStep()
    {
        var order = InStatus(OrderStatus.Delivered);

        order.Status.ShouldBe(OrderStatus.Delivered);
        order.StatusHistory.Select(h => h.ToStatus).ShouldBe(
            [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Shipping, OrderStatus.Delivered]);
        order.ShippedAt.ShouldNotBeNull();
        order.DeliveredAt.ShouldNotBeNull();
    }

    [Fact]
    public void MarkDelivered_Cod_MarksPaymentPaid()
    {
        var order = TestData.Order();
        order.Confirm(TestData.Staff, TestData.Now, false, null);
        order.StartPreparing(TestData.Staff, TestData.Now);
        order.Ship(TestData.Staff, "GHN", TestData.Now);

        order.MarkDelivered(TestData.Staff, TestData.Now).IsSuccess.ShouldBeTrue();

        order.Payment.Status.ShouldBe(PaymentStatus.Paid);
        order.Payment.PaidAt.ShouldBe(TestData.Now);
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Shipping)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Confirm_FromAnythingButPending_Fails(OrderStatus status)
    {
        InStatus(status).Confirm(TestData.Staff, TestData.Now, true, null).Error.ShouldBe(OrderErrors.InvalidStatusTransition);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Shipping)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void StartPreparing_FromAnythingButConfirmed_Fails(OrderStatus status)
    {
        InStatus(status).StartPreparing(TestData.Staff, TestData.Now).Error.ShouldBe(OrderErrors.InvalidStatusTransition);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipping)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Ship_FromAnythingButPreparing_Fails(OrderStatus status)
    {
        InStatus(status).Ship(TestData.Staff, null, TestData.Now).Error.ShouldBe(OrderErrors.InvalidStatusTransition);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void MarkDelivered_FromAnythingButShipping_Fails(OrderStatus status)
    {
        InStatus(status).MarkDelivered(TestData.Staff, TestData.Now).Error.ShouldBe(OrderErrors.InvalidStatusTransition);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, CancelStockEffect.ReleaseReservation)]
    [InlineData(OrderStatus.Confirmed, CancelStockEffect.ReturnToStock)]
    [InlineData(OrderStatus.Preparing, CancelStockEffect.ReturnToStock)]
    [InlineData(OrderStatus.Shipping, CancelStockEffect.None)]
    public void Cancel_ByStaff_ReturnsStockEffectForStatus(OrderStatus status, CancelStockEffect expected)
    {
        var order = InStatus(status);

        var result = order.Cancel(TestData.Staff, "Hết hàng", TestData.Now);

        result.Value.ShouldBe(expected);
        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.CancelReason.ShouldBe("Hết hàng");
        order.CancelledBy.ShouldBe(ActorType.Staff);
        order.DomainEvents.OfType<OrderCancelledDomainEvent>().ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Cancel_TerminalOrder_Fails(OrderStatus status)
    {
        InStatus(status).Cancel(TestData.Staff, "x", TestData.Now).Error.ShouldBe(OrderErrors.InvalidStatusTransition);
    }

    [Fact]
    public void Cancel_ByCustomer_OnlyWhilePending()
    {
        TestData.Order().Cancel(Customer, "Đổi ý", TestData.Now).Value.ShouldBe(CancelStockEffect.ReleaseReservation);
        InStatus(OrderStatus.Confirmed).Cancel(Customer, "Đổi ý", TestData.Now).Error.ShouldBe(OrderErrors.CustomerCanOnlyCancelPending);
        InStatus(OrderStatus.Confirmed).Cancel(OrderActor.System, "x", TestData.Now).Error.ShouldBe(OrderErrors.CustomerCanOnlyCancelPending);
    }

    [Fact]
    public void Cancel_PaidOrder_LeavesRefundPending_UntilStaffMarksRefunded()
    {
        var order = InStatus(OrderStatus.Confirmed, PaymentMethod.BankTransfer);

        order.Cancel(TestData.Staff, "Khách đổi ý", TestData.Now).IsSuccess.ShouldBeTrue();
        order.Payment.Status.ShouldBe(PaymentStatus.RefundPending);

        order.MarkRefunded(TestData.Staff, "FT-REFUND-1", TestData.Now).IsSuccess.ShouldBeTrue();
        order.Payment.Status.ShouldBe(PaymentStatus.Refunded);
        order.MarkRefunded(TestData.Staff, null, TestData.Now).Error.ShouldBe(OrderErrors.NoRefundPending);
    }

    [Fact]
    public void Cancel_ByCustomer_OfPaidPendingOrder_IsRefused()
    {
        var order = TestData.Order(PaymentMethod.BankTransfer);
        order.MarkPaid(TestData.Staff, "FT1", TestData.Now, TestData.Now);

        order.CanCustomerCancel.ShouldBeFalse();
        order.Cancel(Customer, "Đổi ý", TestData.Now).Error.ShouldBe(OrderErrors.PaidOrderNeedsStaff);
    }

    [Fact]
    public void EveryChange_UpdatesLastChangedAt()
    {
        var order = TestData.Order(PaymentMethod.BankTransfer);
        var later = TestData.Now.AddMinutes(5);

        order.MarkPaid(TestData.Staff, "FT1", TestData.Now, later);

        order.LastChangedAt.ShouldBe(later);
    }

    [Fact]
    public void MarkPaid_Twice_OrOnCancelled_Fails()
    {
        var order = TestData.Order(PaymentMethod.BankTransfer);
        order.MarkPaid(TestData.Staff, "A", TestData.Now, TestData.Now).IsSuccess.ShouldBeTrue();
        order.MarkPaid(TestData.Staff, "B", TestData.Now, TestData.Now).Error.ShouldBe(OrderErrors.AlreadyPaid);

        InStatus(OrderStatus.Cancelled, PaymentMethod.BankTransfer)
            .MarkPaid(TestData.Staff, "A", TestData.Now, TestData.Now).Error.ShouldBe(OrderErrors.CannotPayCancelledOrder);
    }

    [Fact]
    public void CanCustomerCancel_OnlyPending()
    {
        TestData.Order().CanCustomerCancel.ShouldBeTrue();
        InStatus(OrderStatus.Confirmed).CanCustomerCancel.ShouldBeFalse();
    }
}
