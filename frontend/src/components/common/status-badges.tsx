import { Badge } from '@/components/ui/badge'
import type { OrderStatus, PaymentStatus } from '@/lib/api/types'
import {
  orderStatusLabels,
  orderStatusVariants,
  paymentStatusLabels,
  paymentStatusVariants,
} from '@/lib/labels'

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  return <Badge variant={orderStatusVariants[status]}>{orderStatusLabels[status]}</Badge>
}

export function PaymentStatusBadge({ status }: { status: PaymentStatus }) {
  return <Badge variant={paymentStatusVariants[status]}>{paymentStatusLabels[status]}</Badge>
}
