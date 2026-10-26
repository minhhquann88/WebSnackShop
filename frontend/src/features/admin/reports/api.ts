import { api, unwrap } from '@/lib/api/client'
import type { Schemas } from '@/lib/api/types'

export const getDashboard = () => unwrap(api.GET('/api/v1/admin/reports/dashboard'))

/** `from`/`to` are calendar dates (YYYY-MM-DD) in shop time. */
export const getRevenueReport = (from: string, to: string) =>
  unwrap(api.GET('/api/v1/admin/reports/revenue', { params: { query: { from, to } } }))

export const getTopProducts = (from: string, to: string, limit = 10) =>
  unwrap(api.GET('/api/v1/admin/reports/top-products', { params: { query: { from, to, limit } } }))

export const getAdminSettings = () => unwrap(api.GET('/api/v1/admin/settings'))

export const updateAdminSettings = (body: Schemas['UpdateSettingsCommand']) =>
  unwrap(api.PUT('/api/v1/admin/settings', { body }))
