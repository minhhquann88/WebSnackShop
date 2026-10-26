import type { QueryClient } from '@tanstack/react-query'
import { api, unwrap } from '@/lib/api/client'
import type { CartLine } from '@/lib/api/types'
import { useGuestCartStore } from '@/stores/cart.store'

export const cartKeys = {
  mine: ['me', 'cart'] as const,
  estimate: (items: readonly CartLine[]) => ['orders', 'estimate', items] as const,
}

export const fetchMyCart = () => unwrap(api.GET('/api/v1/me/cart'))

export const addMyCartItem = (variantId: string, quantity: number) =>
  unwrap(api.POST('/api/v1/me/cart/items', { body: { variantId, quantity } }))

export const updateMyCartItem = (variantId: string, quantity: number) =>
  unwrap(
    api.PUT('/api/v1/me/cart/items/{variantId}', { params: { path: { variantId } }, body: { quantity } }),
  )

export const removeMyCartItem = (variantId: string) =>
  unwrap(api.DELETE('/api/v1/me/cart/items/{variantId}', { params: { path: { variantId } } }))

export const clearMyCart = () => unwrap(api.DELETE('/api/v1/me/cart'))

export const estimateOrder = (items: CartLine[]) =>
  unwrap(api.POST('/api/v1/orders/estimate', { body: { items } }))

let mergeInFlight: Promise<void> | null = null

/**
 * After sign-in: push the guest (localStorage) cart into `/me/cart/merge`, then clear it locally.
 * Idempotent per sign-in (single-flight) so StrictMode double effects do not merge twice.
 */
export function mergeGuestCart(queryClient: QueryClient): Promise<void> {
  mergeInFlight ??= (async () => {
    const lines = useGuestCartStore.getState().lines
    if (lines.length === 0) return
    const items = lines.map((l) => ({ variantId: l.variantId, quantity: l.quantity }))
    useGuestCartStore.getState().clear()
    try {
      await unwrap(api.POST('/api/v1/me/cart/merge', { body: { items } }))
    } catch (error) {
      // Put the lines back so nothing is lost; the user can retry by reloading.
      for (const line of lines) useGuestCartStore.getState().add(line)
      throw error
    }
    await queryClient.invalidateQueries({ queryKey: cartKeys.mine })
  })().finally(() => {
    mergeInFlight = null
  })
  return mergeInFlight
}
