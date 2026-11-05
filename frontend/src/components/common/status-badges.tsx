import { Badge } from '@/components/ui/badge'
import type { OrderStatus, PaymentStatus } from '@/lib/api/types'
import {
  orderStatusLabels,
  orderStatusVariants,
  paymentStatusLabels,
  paymentStatusVariants,
} from '@/lib/labels'

/** Order and payment states are stamped: once set they read as done, not as a toggle. */
export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  return (
    <Badge key={status} shape="stamp" variant={orderStatusVariants[status]}>
      {orderStatusLabels[status]}
    </Badge>
  )
}

export function PaymentStatusBadge({ status }: { status: PaymentStatus }) {
  return (
    <Badge key={status} shape="stamp" variant={paymentStatusVariants[status]}>
      {paymentStatusLabels[status]}
    </Badge>
  )
}
