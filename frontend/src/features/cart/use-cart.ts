'use client'

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo } from 'react'
import { useAuthStore } from '@/lib/auth/auth.store'
import type { CartLine, PricedLine } from '@/lib/api/types'
import { useHydrated } from '@/lib/use-hydrated'
import { guestItemCount, useGuestCartStore, type GuestCartLine } from '@/stores/cart.store'
import {
  addMyCartItem,
  cartKeys,
  clearMyCart,
  estimateOrder,
  fetchMyCart,
  removeMyCartItem,
  updateMyCartItem,
} from './api'

export type CartMode = 'loading' | 'guest' | 'user'

/**
 * One cart API for the UI regardless of sign-in state:
 * guests → Zustand/localStorage, customers → `/me/cart`.
 */
export function useCart() {
  const queryClient = useQueryClient()
  const status = useAuthStore((s) => s.status)
  const hydrated = useHydrated()
  const guestLines = useGuestCartStore((s) => s.lines)
  const guest = useGuestCartStore.getState

  const isUser = status === 'authenticated'
  const mode: CartMode = !hydrated || status === 'loading' ? 'loading' : isUser ? 'user' : 'guest'

  const myCart = useQuery({ queryKey: cartKeys.mine, queryFn: fetchMyCart, enabled: isUser })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: cartKeys.mine })

  const addMutation = useMutation({
    mutationFn: async (line: GuestCartLine) => {
      if (isUser) await addMyCartItem(line.variantId, line.quantity)
      else guest().add(line)
    },
    onSuccess: () => (isUser ? invalidate() : undefined),
  })

  const setQuantityMutation = useMutation({
    mutationFn: async ({ variantId, quantity }: { variantId: string; quantity: number }) => {
      if (isUser) await updateMyCartItem(variantId, quantity)
      else guest().setQuantity(variantId, quantity)
    },
    onSuccess: () => (isUser ? invalidate() : undefined),
  })

  const removeMutation = useMutation({
    mutationFn: async (variantId: string) => {
      if (isUser) await removeMyCartItem(variantId)
      else guest().remove(variantId)
    },
    onSuccess: () => (isUser ? invalidate() : undefined),
  })

  const clearMutation = useMutation({
    mutationFn: async () => {
      if (isUser) await clearMyCart()
      else guest().clear()
    },
    onSuccess: () => (isUser ? invalidate() : undefined),
  })

  const items: CartLine[] = useMemo(() => {
    if (mode === 'user')
      return (myCart.data?.items ?? []).map((i) => ({ variantId: i.variantId, quantity: i.quantity }))
    if (mode === 'guest') return guestLines.map((l) => ({ variantId: l.variantId, quantity: l.quantity }))
    return []
  }, [mode, myCart.data, guestLines])

  const itemCount =
    mode === 'user' ? (myCart.data?.itemCount ?? 0) : mode === 'guest' ? guestItemCount(guestLines) : 0

  return {
    mode,
    items,
    itemCount,
    myCart,
    isLoading: mode === 'loading' || (mode === 'user' && myCart.isPending),
    add: addMutation.mutateAsync,
    setQuantity: setQuantityMutation.mutateAsync,
    remove: removeMutation.mutateAsync,
    clear: clearMutation.mutateAsync,
    isMutating:
      addMutation.isPending ||
      setQuantityMutation.isPending ||
      removeMutation.isPending ||
      clearMutation.isPending,
  }
}

/**
 * Server-priced lines for display: `/me/cart` for customers, `POST /orders/estimate` for guests.
 * The totals shown at checkout always come from the estimate endpoint.
 */
export function useEstimate(items: CartLine[], enabled = true) {
  return useQuery({
    queryKey: cartKeys.estimate(items),
    queryFn: () => estimateOrder(items),
    enabled: enabled && items.length > 0,
    placeholderData: (previous) => previous,
  })
}

export type { PricedLine }
