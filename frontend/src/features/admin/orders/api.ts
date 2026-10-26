import { api, unwrap } from '@/lib/api/client'
import type { OrderStatus, PaymentMethod, PaymentStatus } from '@/lib/api/types'

export interface AdminOrderFilters {
  status?: OrderStatus
  paymentMethod?: PaymentMethod
  paymentStatus?: PaymentStatus
  /** YYYY-MM-DD in shop time. */
  from?: string
  /** YYYY-MM-DD in shop time (inclusive). */
  to?: string
  search?: string
  page: number
  pageSize: number
}

/**
 * Vietnam calendar day → UTC ISO instant. The API takes date-time; it rejects (500) non-UTC
 * offsets such as `+07:00`, so the instant is always sent as `...Z`.
 */
export function shopDayStartUtc(day: string): string {
  return new Date(`${day}T00:00:00+07:00`).toISOString()
}

export function shopDayEndUtc(day: string): string {
  return new Date(`${day}T23:59:59.999+07:00`).toISOString()
}

export const listAdminOrders = (f: AdminOrderFilters) =>
  unwrap(
    api.GET('/api/v1/admin/orders', {
      params: {
        query: {
          status: f.status,
          paymentMethod: f.paymentMethod,
          paymentStatus: f.paymentStatus,
          from: f.from ? shopDayStartUtc(f.from) : undefined,
          to: f.to ? shopDayEndUtc(f.to) : undefined,
          search: f.search || undefined,
          page: f.page,
          pageSize: f.pageSize,
        },
      },
    }),
  )

export const getAdminOrder = (id: string) =>
  unwrap(api.GET('/api/v1/admin/orders/{id}', { params: { path: { id } } }))

const path = (id: string) => ({ params: { path: { id } } })

export const confirmOrder = (id: string, markPaid: boolean, transactionRef: string | null) =>
  unwrap(api.POST('/api/v1/admin/orders/{id}/confirm', { ...path(id), body: { markPaid, transactionRef } }))

export const startPreparingOrder = (id: string) =>
  unwrap(api.POST('/api/v1/admin/orders/{id}/prepare', path(id)))

export const shipOrder = (id: string, note: string | null) =>
  unwrap(api.POST('/api/v1/admin/orders/{id}/ship', { ...path(id), body: { note } }))

export const deliverOrder = (id: string) => unwrap(api.POST('/api/v1/admin/orders/{id}/deliver', path(id)))

export const cancelOrder = (id: string, reason: string) =>
  unwrap(api.POST('/api/v1/admin/orders/{id}/cancel', { ...path(id), body: { reason } }))

export const markOrderPaid = (id: string, transactionRef: string | null, paidAt: string | null) =>
  unwrap(
    api.POST('/api/v1/admin/orders/{id}/payment/mark-paid', {
      ...path(id),
      body: { transactionRef, paidAt },
    }),
  )

export const markOrderRefunded = (id: string, transactionRef: string | null) =>
  unwrap(
    api.POST('/api/v1/admin/orders/{id}/payment/mark-refunded', {
      ...path(id),
      body: { transactionRef },
    }),
  )

export const updateInternalNote = (id: string, note: string | null) =>
  unwrap(api.PUT('/api/v1/admin/orders/{id}/internal-note', { ...path(id), body: { note } }))
