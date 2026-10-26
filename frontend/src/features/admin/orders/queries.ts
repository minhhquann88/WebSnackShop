'use client'

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  cancelOrder,
  confirmOrder,
  deliverOrder,
  getAdminOrder,
  listAdminOrders,
  markOrderPaid,
  markOrderRefunded,
  shipOrder,
  startPreparingOrder,
  updateInternalNote,
  type AdminOrderFilters,
} from './api'

export const adminOrderKeys = {
  all: ['admin', 'orders'] as const,
  list: (f: AdminOrderFilters) => ['admin', 'orders', 'list', f] as const,
  detail: (id: string) => ['admin', 'orders', 'detail', id] as const,
}

export function useAdminOrdersQuery(filters: AdminOrderFilters) {
  return useQuery({
    queryKey: adminOrderKeys.list(filters),
    queryFn: () => listAdminOrders(filters),
    placeholderData: keepPreviousData,
  })
}

export function useAdminOrderQuery(id: string) {
  return useQuery({ queryKey: adminOrderKeys.detail(id), queryFn: () => getAdminOrder(id) })
}

export type OrderAction =
  | { type: 'confirm'; markPaid: boolean; transactionRef: string | null }
  | { type: 'prepare' }
  | { type: 'ship'; note: string | null }
  | { type: 'deliver' }
  | { type: 'cancel'; reason: string }
  | { type: 'markPaid'; transactionRef: string | null; paidAt: string | null }
  | { type: 'markRefunded'; transactionRef: string | null }
  | { type: 'internalNote'; note: string | null }

function runAction(id: string, action: OrderAction): Promise<unknown> {
  switch (action.type) {
    case 'confirm':
      return confirmOrder(id, action.markPaid, action.transactionRef)
    case 'prepare':
      return startPreparingOrder(id)
    case 'ship':
      return shipOrder(id, action.note)
    case 'deliver':
      return deliverOrder(id)
    case 'cancel':
      return cancelOrder(id, action.reason)
    case 'markPaid':
      return markOrderPaid(id, action.transactionRef, action.paidAt)
    case 'markRefunded':
      return markOrderRefunded(id, action.transactionRef)
    case 'internalNote':
      return updateInternalNote(id, action.note)
  }
}

export function useOrderActionMutation(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (action: OrderAction) => runAction(id, action),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: adminOrderKeys.all }),
        queryClient.invalidateQueries({ queryKey: ['admin', 'dashboard'] }),
      ]),
  })
}
