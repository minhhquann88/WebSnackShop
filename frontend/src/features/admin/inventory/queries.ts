'use client'

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  adjustStock,
  listStock,
  listStockMovements,
  receiveStock,
  setLowStockThreshold,
  type StockFilters,
} from './api'

export const adminInventoryKeys = {
  all: ['admin', 'inventory'] as const,
  list: (f: StockFilters) => ['admin', 'inventory', 'list', f] as const,
  movements: (variantId: string, page: number) =>
    ['admin', 'inventory', 'movements', variantId, page] as const,
}

export function useStockQuery(filters: StockFilters) {
  return useQuery({
    queryKey: adminInventoryKeys.list(filters),
    queryFn: () => listStock(filters),
    placeholderData: keepPreviousData,
  })
}

export function useStockMovementsQuery(variantId: string, page: number, pageSize = 20) {
  return useQuery({
    queryKey: adminInventoryKeys.movements(variantId, page),
    queryFn: () => listStockMovements(variantId, page, pageSize),
    placeholderData: keepPreviousData,
  })
}

export function useStockMutations() {
  const queryClient = useQueryClient()
  const invalidate = () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: adminInventoryKeys.all }),
      queryClient.invalidateQueries({ queryKey: ['admin', 'dashboard'] }),
      queryClient.invalidateQueries({ queryKey: ['admin', 'products'] }),
    ])
  return {
    receive: useMutation({
      mutationFn: (v: { variantId: string; quantity: number; note: string | null }) =>
        receiveStock(v.variantId, v.quantity, v.note),
      onSuccess: invalidate,
    }),
    adjust: useMutation({
      mutationFn: (v: { variantId: string; delta: number; reason: string }) =>
        adjustStock(v.variantId, v.delta, v.reason),
      onSuccess: invalidate,
    }),
    threshold: useMutation({
      mutationFn: (v: { variantId: string; threshold: number }) =>
        setLowStockThreshold(v.variantId, v.threshold),
      onSuccess: invalidate,
    }),
  }
}
