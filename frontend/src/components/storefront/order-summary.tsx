import type { OrderEstimate } from '@/lib/api/types'
import { formatVnd } from '@/lib/format'

/** Totals block fed by `POST /orders/estimate` — never computed client-side. */
export function EstimateSummary({ estimate }: { estimate: OrderEstimate }) {
  const threshold = estimate.freeShippingThreshold
  const remaining = threshold ? threshold.amount - estimate.subtotal.amount : 0
  return (
    <dl className="grid gap-2 text-sm">
      <div className="flex justify-between">
        <dt className="text-muted-foreground">Tạm tính</dt>
        <dd>{formatVnd(estimate.subtotal)}</dd>
      </div>
      <div className="flex justify-between">
        <dt className="text-muted-foreground">Phí vận chuyển</dt>
        <dd>{estimate.shippingFee.amount === 0 ? 'Miễn phí' : formatVnd(estimate.shippingFee)}</dd>
      </div>
      {threshold && remaining > 0 ? (
        <p className="rounded-md bg-accent px-2 py-1.5 text-xs text-accent-foreground">
          Mua thêm {formatVnd(remaining)} để được miễn phí vận chuyển.
        </p>
      ) : null}
      <div className="flex justify-between border-t pt-2 text-base font-semibold">
        <dt>Tổng cộng</dt>
        <dd className="text-primary">{formatVnd(estimate.total)}</dd>
      </div>
    </dl>
  )
}
