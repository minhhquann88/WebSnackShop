'use client'

import Link from 'next/link'
import { useState } from 'react'
import { ChevronRight } from 'lucide-react'
import { buttonVariants } from '@/components/ui/button'
import { Card, CardTitle } from '@/components/ui/card'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Pagination, totalPagesOf } from '@/components/ui/pagination'
import { RemoteImage } from '@/components/ui/remote-image'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/status-badges'
import { useMyOrdersQuery } from '@/features/orders/queries'
import { getErrorMessage } from '@/lib/errors'
import { formatDateTime, formatVnd } from '@/lib/format'
import { paymentMethodShortLabels } from '@/lib/labels'

export default function MyOrdersPage() {
  const [page, setPage] = useState(1)
  const orders = useMyOrdersQuery(page)

  return (
    <Card>
      <CardTitle>Đơn hàng của tôi</CardTitle>
      {orders.isPending ? (
        <Spinner />
      ) : orders.isError ? (
        <ErrorState message={getErrorMessage(orders.error)} onRetry={() => orders.refetch()} />
      ) : orders.data.items.length === 0 ? (
        <EmptyState
          title="Bạn chưa có đơn hàng nào"
          action={
            <Link href="/san-pham" className={buttonVariants()}>
              Mua sắm ngay
            </Link>
          }
        />
      ) : (
        <>
          <ul className="grid gap-3" aria-busy={orders.isFetching}>
            {orders.data.items.map((o) => (
              <li key={o.id}>
                <Link
                  href={`/tai-khoan/don-hang/${encodeURIComponent(o.orderNumber)}`}
                  className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-accent/50"
                >
                  <RemoteImage src={o.firstItemImageUrl} alt="" className="size-14 shrink-0 rounded-md" />
                  <div className="grid min-w-0 flex-1 gap-1 text-sm">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-semibold">{o.orderNumber}</span>
                      <OrderStatusBadge status={o.status} />
                      <PaymentStatusBadge status={o.paymentStatus} />
                    </div>
                    <p className="truncate text-muted-foreground">
                      {o.firstItemName}
                      {o.itemCount > 1 ? ` và ${o.itemCount - 1} sản phẩm khác` : ''}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {formatDateTime(o.placedAt)} · {paymentMethodShortLabels[o.paymentMethod]}
                    </p>
                  </div>
                  <span className="shrink-0 font-semibold text-primary">{formatVnd(o.total)}</span>
                  <ChevronRight className="size-4 shrink-0 text-muted-foreground" aria-hidden />
                </Link>
              </li>
            ))}
          </ul>
          <Pagination page={orders.data.page} totalPages={totalPagesOf(orders.data)} onPageChange={setPage} />
        </>
      )}
    </Card>
  )
}
