import type { PlaceOrderResponse } from '@/lib/api/types'

const KEY = 'snackshop-last-order'

/** Keeps the PlaceOrder response for the success page (bank transfer QR) across the client navigation/reload. */
export function saveLastOrder(order: PlaceOrderResponse): void {
  try {
    sessionStorage.setItem(KEY, JSON.stringify(order))
  } catch {
    // Storage unavailable (private mode): the success page falls back to tracking links.
  }
}

export function readLastOrder(orderNumber: string): PlaceOrderResponse | null {
  try {
    const raw = sessionStorage.getItem(KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as PlaceOrderResponse
    return parsed.orderNumber === orderNumber ? parsed : null
  } catch {
    return null
  }
}
