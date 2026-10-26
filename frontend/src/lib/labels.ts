import type { BadgeVariant } from '@/components/ui/badge'
import type { ActorType, OrderStatus, PaymentMethod, PaymentStatus, StockMovementType } from '@/lib/api/types'

/** Display-only labels for API enums. Business rules (what is allowed) always come from the API's `can*` flags. */
export const orderStatusLabels: Record<OrderStatus, string> = {
  Pending: 'Chờ xác nhận',
  Confirmed: 'Đã xác nhận',
  Preparing: 'Đang chuẩn bị',
  Shipping: 'Đang giao',
  Delivered: 'Đã giao',
  Cancelled: 'Đã hủy',
}

export const orderStatusVariants: Record<OrderStatus, BadgeVariant> = {
  Pending: 'warning',
  Confirmed: 'info',
  Preparing: 'info',
  Shipping: 'secondary',
  Delivered: 'success',
  Cancelled: 'destructive',
}

export const paymentMethodLabels: Record<PaymentMethod, string> = {
  Cod: 'Thanh toán khi nhận hàng (COD)',
  BankTransfer: 'Chuyển khoản ngân hàng',
}

export const paymentMethodShortLabels: Record<PaymentMethod, string> = {
  Cod: 'COD',
  BankTransfer: 'Chuyển khoản',
}

export const paymentStatusLabels: Record<PaymentStatus, string> = {
  Unpaid: 'Chưa thanh toán',
  Paid: 'Đã thanh toán',
  Refunded: 'Đã hoàn tiền',
  Failed: 'Thất bại',
  RefundPending: 'Chờ hoàn tiền',
}

export const paymentStatusVariants: Record<PaymentStatus, BadgeVariant> = {
  Unpaid: 'warning',
  Paid: 'success',
  Refunded: 'muted',
  Failed: 'destructive',
  RefundPending: 'warning',
}

export const actorTypeLabels: Record<ActorType, string> = {
  Customer: 'Khách hàng',
  Staff: 'Nhân viên',
  System: 'Hệ thống',
}

export const stockMovementLabels: Record<StockMovementType, string> = {
  Purchase: 'Nhập kho',
  Sale: 'Bán hàng',
  Adjustment: 'Điều chỉnh',
  Return: 'Trả kho',
  Reservation: 'Giữ hàng',
  ReservationRelease: 'Nhả giữ hàng',
}

export const roleLabels: Record<string, string> = {
  Customer: 'Khách hàng',
  Staff: 'Nhân viên',
  Admin: 'Quản trị',
}

export const ORDER_STATUSES: OrderStatus[] = [
  'Pending',
  'Confirmed',
  'Preparing',
  'Shipping',
  'Delivered',
  'Cancelled',
]
export const PAYMENT_METHODS: PaymentMethod[] = ['Cod', 'BankTransfer']
export const PAYMENT_STATUSES: PaymentStatus[] = ['Unpaid', 'Paid', 'Refunded', 'Failed']
