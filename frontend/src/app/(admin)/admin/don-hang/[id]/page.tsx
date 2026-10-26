'use client'

import { useState } from 'react'
import { useParams } from 'next/navigation'
import { toast } from 'sonner'
import { PageHeader } from '@/components/admin/page-header'
import { OrderActions } from '@/components/admin/order-actions'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/status-badges'
import { Button } from '@/components/ui/button'
import { Card, CardTitle } from '@/components/ui/card'
import { ErrorState, Spinner } from '@/components/ui/feedback'
import { Textarea } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { RemoteImage } from '@/components/ui/remote-image'
import { useAdminOrderQuery, useOrderActionMutation } from '@/features/admin/orders/queries'
import type { Schemas } from '@/lib/api/types'
import { getErrorMessage } from '@/lib/errors'
import { formatDateTime, formatVnd } from '@/lib/format'
import { actorTypeLabels, orderStatusLabels, paymentMethodLabels } from '@/lib/labels'

type AdminOrderDetail = Schemas['AdminOrderDetailResponse']

function InternalNoteEditor({ order }: { order: AdminOrderDetail }) {
  const [note, setNote] = useState(order.internalNote ?? '')
  const mutation = useOrderActionMutation(order.id)
  const dirty = note !== (order.internalNote ?? '')
  const save = () =>
    mutation.mutate(
      { type: 'internalNote', note: note.trim() || null },
      {
        onSuccess: () => toast.success('Đã lưu ghi chú nội bộ'),
        onError: (e) => toast.error(getErrorMessage(e)),
      },
    )
  return (
    <Card>
      <CardTitle>Ghi chú nội bộ</CardTitle>
      <div className="grid gap-1.5">
        <Label htmlFor="internal-note">Chỉ nhân viên thấy</Label>
        <Textarea
          id="internal-note"
          value={note}
          maxLength={1000}
          onChange={(e) => setNote(e.target.value)}
        />
      </div>
      <Button
        className="w-fit"
        variant="outline"
        onClick={save}
        disabled={!dirty}
        loading={mutation.isPending}
      >
        Lưu ghi chú
      </Button>
    </Card>
  )
}

function OrderDetail({ order }: { order: AdminOrderDetail }) {
  const a = order.shippingAddress
  return (
    <div className="grid gap-6 lg:grid-cols-3">
      <div className="grid content-start gap-6 lg:col-span-2">
        <Card>
          <CardTitle>Sản phẩm</CardTitle>
          <ul className="divide-y">
            {order.items.map((item) => (
              <li key={item.variantId} className="flex gap-3 py-3">
                <RemoteImage
                  src={item.imageUrl}
                  alt={item.productName}
                  className="size-14 shrink-0 rounded-md border"
                />
                <div className="min-w-0 flex-1">
                  <p className="font-medium">{item.productName}</p>
                  <p className="text-sm text-muted-foreground">
                    {item.variantName} · <span className="font-mono text-xs">{item.sku}</span>
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {formatVnd(item.unitPrice)} × {item.quantity}
                  </p>
                </div>
                <p className="font-medium tabular-nums">{formatVnd(item.lineTotal)}</p>
              </li>
            ))}
          </ul>
          <dl className="grid gap-1 border-t pt-3 text-sm">
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Tạm tính</dt>
              <dd className="tabular-nums">{formatVnd(order.subtotal)}</dd>
            </div>
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Phí vận chuyển</dt>
              <dd className="tabular-nums">{formatVnd(order.shippingFee)}</dd>
            </div>
            {order.discountAmount.amount > 0 ? (
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Giảm giá</dt>
                <dd className="tabular-nums">-{formatVnd(order.discountAmount)}</dd>
              </div>
            ) : null}
            <div className="flex justify-between text-base font-semibold">
              <dt>Tổng cộng</dt>
              <dd className="tabular-nums">{formatVnd(order.total)}</dd>
            </div>
          </dl>
        </Card>

        <Card>
          <CardTitle>Lịch sử trạng thái</CardTitle>
          <ol className="relative grid gap-4 border-l pl-5">
            {order.history.map((h, i) => (
              <li key={`${h.changedAt}-${i}`} className="relative">
                <span
                  className="absolute top-1.5 -left-[25px] size-2.5 rounded-full bg-primary"
                  aria-hidden
                />
                <p className="text-sm font-medium">
                  {h.fromStatus ? `${orderStatusLabels[h.fromStatus]} → ` : ''}
                  {orderStatusLabels[h.toStatus]}
                </p>
                <p className="text-xs text-muted-foreground">
                  {formatDateTime(h.changedAt)} · {h.actorType ? actorTypeLabels[h.actorType] : '—'}
                </p>
                {h.note ? <p className="mt-1 text-sm">{h.note}</p> : null}
              </li>
            ))}
          </ol>
        </Card>
      </div>

      <div className="grid content-start gap-6">
        <Card>
          <CardTitle>Thao tác</CardTitle>
          <OrderActions order={order} />
        </Card>

        <Card>
          <CardTitle>Thanh toán</CardTitle>
          <dl className="grid gap-2 text-sm">
            <div>
              <dt className="text-muted-foreground">Phương thức</dt>
              <dd>{paymentMethodLabels[order.payment.method]}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Trạng thái</dt>
              <dd>
                <PaymentStatusBadge status={order.payment.status} />
              </dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Số tiền</dt>
              <dd className="tabular-nums">{formatVnd(order.payment.amount)}</dd>
            </div>
            {order.payment.transferContent ? (
              <div>
                <dt className="text-muted-foreground">Nội dung chuyển khoản</dt>
                <dd className="font-mono">{order.payment.transferContent}</dd>
              </div>
            ) : null}
            {order.payment.transactionRef ? (
              <div>
                <dt className="text-muted-foreground">Mã giao dịch</dt>
                <dd className="font-mono">{order.payment.transactionRef}</dd>
              </div>
            ) : null}
            {order.payment.paidAt ? (
              <div>
                <dt className="text-muted-foreground">Thanh toán lúc</dt>
                <dd>{formatDateTime(order.payment.paidAt)}</dd>
              </div>
            ) : null}
          </dl>
        </Card>

        <Card>
          <CardTitle>Giao hàng</CardTitle>
          <address className="grid gap-0.5 text-sm not-italic">
            <span className="font-medium">{a.fullName}</span>
            <a href={`tel:${a.phone}`} className="text-primary hover:underline">
              {a.phone}
            </a>
            <span>{a.line1}</span>
            <span>
              {a.ward}, {a.district}, {a.province}
            </span>
          </address>
          <div className="grid gap-0.5 text-sm">
            <span className="text-muted-foreground">Email liên hệ</span>
            <a href={`mailto:${order.contactEmail}`} className="break-all text-primary hover:underline">
              {order.contactEmail}
            </a>
            <span className="text-muted-foreground">
              {order.customerId ? 'Khách có tài khoản' : 'Khách vãng lai'}
            </span>
          </div>
          {order.customerNote ? (
            <div className="grid gap-0.5 text-sm">
              <span className="text-muted-foreground">Ghi chú của khách</span>
              <p className="whitespace-pre-line">{order.customerNote}</p>
            </div>
          ) : null}
          {order.cancelReason ? (
            <div className="grid gap-0.5 text-sm">
              <span className="text-muted-foreground">Lý do hủy</span>
              <p>
                {order.cancelReason}
                {order.cancelledBy ? ` (${actorTypeLabels[order.cancelledBy]})` : ''}
              </p>
            </div>
          ) : null}
        </Card>

        <InternalNoteEditor key={order.internalNote ?? ''} order={order} />
      </div>
    </div>
  )
}

export default function AdminOrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const query = useAdminOrderQuery(id)

  return (
    <>
      <PageHeader
        backHref="/admin/don-hang"
        backLabel="Danh sách đơn"
        title={query.data ? `Đơn ${query.data.orderNumber}` : 'Chi tiết đơn hàng'}
        description={
          query.data ? (
            <span className="flex flex-wrap items-center gap-2">
              <OrderStatusBadge status={query.data.status} />
              <span>Đặt lúc {formatDateTime(query.data.placedAt)}</span>
            </span>
          ) : undefined
        }
      />
      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : (
        <OrderDetail order={query.data} />
      )}
    </>
  )
}
