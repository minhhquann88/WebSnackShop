import { api, unwrap } from '@/lib/api/client'

export interface StockFilters {
  search?: string
  lowStockOnly?: boolean
  page: number
  pageSize: number
}

export const listStock = (f: StockFilters) =>
  unwrap(
    api.GET('/api/v1/admin/inventory', {
      params: {
        query: {
          search: f.search || undefined,
          lowStockOnly: f.lowStockOnly || undefined,
          page: f.page,
          pageSize: f.pageSize,
        },
      },
    }),
  )

export const listStockMovements = (variantId: string, page: number, pageSize: number) =>
  unwrap(
    api.GET('/api/v1/admin/inventory/{variantId}/movements', {
      params: { path: { variantId }, query: { page, pageSize } },
    }),
  )

export const receiveStock = (variantId: string, quantity: number, note: string | null) =>
  unwrap(
    api.POST('/api/v1/admin/inventory/{variantId}/receive', {
      params: { path: { variantId } },
      body: { quantity, note },
    }),
  )

export const adjustStock = (variantId: string, delta: number, reason: string) =>
  unwrap(
    api.POST('/api/v1/admin/inventory/{variantId}/adjust', {
      params: { path: { variantId } },
      body: { delta, reason },
    }),
  )

export const setLowStockThreshold = (variantId: string, threshold: number) =>
  unwrap(
    api.PUT('/api/v1/admin/inventory/{variantId}/threshold', {
      params: { path: { variantId } },
      body: { threshold },
    }),
  )
