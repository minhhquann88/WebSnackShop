import { api, unwrap } from '@/lib/api/client'
import type { Schemas } from '@/lib/api/types'

export type PlaceOrderInput = Schemas['PlaceOrderRequest']

/** `Idempotency-Key` makes retries of the same checkout attempt return the same order. */
export const placeOrder = (body: PlaceOrderInput, idempotencyKey: string) =>
  unwrap(api.POST('/api/v1/orders', { body, params: { header: { 'Idempotency-Key': idempotencyKey } } }))

export const listMyOrders = (page: number, pageSize = 10) =>
  unwrap(api.GET('/api/v1/me/orders', { params: { query: { page: page, pageSize: pageSize } } }))

export const getMyOrder = (orderNumber: string) =>
  unwrap(api.GET('/api/v1/me/orders/{orderNumber}', { params: { path: { orderNumber } } }))

export const cancelMyOrder = (orderNumber: string, reason: string | null) =>
  unwrap(
    api.POST('/api/v1/me/orders/{orderNumber}/cancel', {
      params: { path: { orderNumber } },
      body: { reason },
    }),
  )

export const trackOrder = (orderNumber: string, phone: string) =>
  unwrap(api.GET('/api/v1/orders/track', { params: { query: { orderNumber, phone } } }))
