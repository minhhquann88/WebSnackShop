'use client'

import Link from 'next/link'
import { useSearchParams } from 'next/navigation'
import { useState } from 'react'
import { CheckCircle2 } from 'lucide-react'
import { buttonVariants } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Alert, Spinner } from '@/components/ui/feedback'
import { BankTransferBox } from '@/components/storefront/bank-transfer-box'
import { readLastOrder } from '@/features/orders/last-order'
import { useMyOrderQuery } from '@/features/orders/queries'
import { useAuthStore } from '@/lib/auth/auth.store'
import type { PlaceOrderResponse } from '@/lib/api/types'
import { formatVnd } from '@/lib/format'
import { paymentMethodLabels } from '@/lib/labels'
import { useHydrated } from '@/lib/use-hydrated'

function MyOrderBankTransfer({ orderNumber }: { orderNumber: string }) {
  const order = useMyOrderQuery(orderNumber)
  if (order.isPending) return <Spinner />
  if (!order.data?.bankTransfer) return null
  return <BankTransferBox info={order.data.bankTransfer} />
}

export function OrderSuccess() {
  const hydrated = useHydrated()
  const orderNumber = useSearchParams().get('orderNumber') ?? ''
  const status = useAuthStore((s) => s.status)
  const [order] = useState<PlaceOrderResponse | null>(() =>
    typeof window === 'undefined' ? null : readLastOrder(orderNumber),
  )

  if (!hydrated) return <Spinner />
  if (!orderNumber) {
    return (
      <Alert variant="error">
        Không tìm thấy mã đơn hàng. <Link href="/tra-cuu-don">Tra cứu đơn hàng</Link>
      </Alert>
    )
  }

  const isBankTransfer = order?.paymentMethod === 'BankTransfer'

  return (
    <div className="mx-auto grid max-w-3xl gap-6">
      <Card className="items-center text-center">
        <CheckCircle2 className="size-14 text-success" aria-hidden />
        <h1 className="text-2xl font-bold">Đặt hàng thành công!</h1>
        <p className="text-muted-foreground">
          Cảm ơn bạn đã mua hàng. Mã đơn của bạn là <strong className="text-foreground">{orderNumber}</strong>
          . Chúng tôi đã gửi email xác nhận kèm thông tin đơn hàng.
        </p>
        {order ? (
          <dl className="grid gap-1 text-sm">
            <div className="flex justify-center gap-2">
              <dt className="text-muted-foreground">Tổng thanh toán:</dt>
              <dd className="font-semibold text-primary">{formatVnd(order.total)}</dd>
            </div>
            <div className="flex justify-center gap-2">
              <dt className="text-muted-foreground">Thanh toán:</dt>
              <dd>{paymentMethodLabels[order.paymentMethod]}</dd>
            </div>
          </dl>
        ) : null}
      </Card>

      {order?.bankTransfer ? (
        <BankTransferBox info={order.bankTransfer} />
      ) : !order && status === 'authenticated' ? (
        <MyOrderBankTransfer orderNumber={orderNumber} />
      ) : null}

      {isBankTransfer ? (
        <Alert>Đơn hàng sẽ được xác nhận sau khi shop nhận được tiền chuyển khoản.</Alert>
      ) : null}

      <div className="flex flex-col justify-center gap-2 sm:flex-row">
        {status === 'authenticated' ? (
          <Link
            href={`/tai-khoan/don-hang/${encodeURIComponent(orderNumber)}`}
            className={buttonVariants({ variant: 'outline' })}
          >
            Xem chi tiết đơn hàng
          </Link>
        ) : (
          <Link
            href={`/tra-cuu-don?orderNumber=${encodeURIComponent(orderNumber)}`}
            className={buttonVariants({ variant: 'outline' })}
          >
            Tra cứu đơn hàng
          </Link>
        )}
        <Link href="/san-pham" className={buttonVariants()}>
          Tiếp tục mua sắm
        </Link>
      </div>
    </div>
  )
}
