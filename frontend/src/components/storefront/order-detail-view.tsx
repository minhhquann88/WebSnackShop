import type { ReactNode } from 'react'
import { Card, CardTitle } from '@/components/ui/card'
import { RemoteImage } from '@/components/ui/remote-image'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/status-badges'
import type { OrderDetail } from '@/lib/api/types'
import { formatDateTime, formatVnd } from '@/lib/format'
import { orderStatusLabels, paymentMethodLabels } from '@/lib/labels'
import { BankTransferBox } from './bank-transfer-box'

/** Customer-facing order detail (my order, guest tracking). Actions are passed in by the caller. */
export function OrderDetailView({ order, actions }: { order: OrderDetail; actions?: ReactNode }) {
  const showTransfer = order.bankTransfer && order.paymentStatus === 'Unpaid' && order.status !== 'Cancelled'
  return (
    <div className="grid gap-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="grid gap-1">
          <h1 className="text-2xl font-bold">Đơn hàng {order.orderNumber}</h1>
          <p className="text-sm text-muted-foreground">Đặt lúc {formatDateTime(order.placedAt)}</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <OrderStatusBadge status={order.status} />
          <PaymentStatusBadge status={order.paymentStatus} />
          {actions}
        </div>
      </div>

      {order.status === 'Cancelled' && order.cancelReason ? (
        <p className="rounded-lg border border-destructive/30 bg-destructive/5 p-3 text-sm">
          <span className="font-medium">Lý do hủy:</span> {order.cancelReason}
        </p>
      ) : null}

      {showTransfer && order.bankTransfer ? <BankTransferBox info={order.bankTransfer} /> : null}

      <div className="grid gap-6 lg:grid-cols-[1fr_320px]">
        <Card>
          <CardTitle>Sản phẩm</CardTitle>
          <ul className="grid gap-3">
            {order.items.map((item) => (
              <li key={item.variantId} className="flex gap-3">
                <RemoteImage src={item.imageUrl} alt="" className="size-16 shrink-0 rounded-md" />
                <div className="min-w-0 flex-1 text-sm">
                  <p className="font-medium">{item.productName}</p>
                  <p className="text-muted-foreground">
                    {item.variantName} · {formatVnd(item.unitPrice)} × {item.quantity}
                  </p>
                </div>
                <p className="shrink-0 text-sm font-medium">{formatVnd(item.lineTotal)}</p>
              </li>
            ))}
          </ul>
          <dl className="grid gap-2 border-t pt-3 text-sm">
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Tạm tính</dt>
              <dd>{formatVnd(order.subtotal)}</dd>
            </div>
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Phí vận chuyển</dt>
              <dd>{order.shippingFee.amount === 0 ? 'Miễn phí' : formatVnd(order.shippingFee)}</dd>
            </div>
            {order.discountAmount.amount > 0 ? (
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Giảm giá</dt>
                <dd>-{formatVnd(order.discountAmount)}</dd>
              </div>
            ) : null}
            <div className="flex justify-between text-base font-semibold">
              <dt>Tổng cộng</dt>
              <dd className="text-primary">{formatVnd(order.total)}</dd>
            </div>
          </dl>
        </Card>

        <div className="grid content-start gap-6">
          <Card>
            <CardTitle>Giao đến</CardTitle>
            <address className="grid gap-0.5 text-sm not-italic">
              <span className="font-medium">{order.shippingAddress.fullName}</span>
              <span>{order.shippingAddress.phone}</span>
              <span className="text-muted-foreground">
                {[
                  order.shippingAddress.line1,
                  order.shippingAddress.ward,
                  order.shippingAddress.district,
                  order.shippingAddress.province,
                ].join(', ')}
              </span>
              <span className="text-muted-foreground">{order.contactEmail}</span>
            </address>
            <p className="text-sm">
              <span className="text-muted-foreground">Thanh toán: </span>
              {paymentMethodLabels[order.paymentMethod]}
            </p>
            {order.customerNote ? (
              <p className="text-sm">
                <span className="text-muted-foreground">Ghi chú: </span>
                {order.customerNote}
              </p>
            ) : null}
          </Card>

          <Card>
            <CardTitle>Hành trình đơn hàng</CardTitle>
            <ol className="grid gap-3 border-l-2 border-primary/30 pl-4">
              {order.timeline.map((entry, index) => (
                <li key={`${entry.status}-${entry.changedAt}-${index}`} className="relative text-sm">
                  <span
                    className="absolute top-1 -left-[1.4rem] size-2.5 rounded-full bg-primary"
                    aria-hidden
                  />
                  <p className="font-medium">{orderStatusLabels[entry.status]}</p>
                  <p className="text-xs text-muted-foreground">{formatDateTime(entry.changedAt)}</p>
                  {entry.note ? <p className="text-muted-foreground">{entry.note}</p> : null}
                </li>
              ))}
            </ol>
          </Card>
        </div>
      </div>
    </div>
  )
}
