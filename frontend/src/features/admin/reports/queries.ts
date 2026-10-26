'use client'

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { Schemas } from '@/lib/api/types'
import { getAdminSettings, getDashboard, getRevenueReport, getTopProducts, updateAdminSettings } from './api'

export const adminReportKeys = {
  dashboard: ['admin', 'dashboard'] as const,
  revenue: (from: string, to: string) => ['admin', 'reports', 'revenue', from, to] as const,
  top: (from: string, to: string, limit: number) => ['admin', 'reports', 'top', from, to, limit] as const,
  settings: ['admin', 'settings'] as const,
}

export function useDashboardQuery() {
  return useQuery({ queryKey: adminReportKeys.dashboard, queryFn: getDashboard, refetchInterval: 60_000 })
}

export function useRevenueQuery(from: string, to: string, enabled = true) {
  return useQuery({
    queryKey: adminReportKeys.revenue(from, to),
    queryFn: () => getRevenueReport(from, to),
    placeholderData: keepPreviousData,
    enabled,
  })
}

export function useTopProductsQuery(from: string, to: string, limit = 10, enabled = true) {
  return useQuery({
    queryKey: adminReportKeys.top(from, to, limit),
    queryFn: () => getTopProducts(from, to, limit),
    placeholderData: keepPreviousData,
    enabled,
  })
}

export function useAdminSettingsQuery(enabled = true) {
  return useQuery({ queryKey: adminReportKeys.settings, queryFn: getAdminSettings, enabled })
}

export function useUpdateSettingsMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: Schemas['UpdateSettingsCommand']) => updateAdminSettings(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: adminReportKeys.settings }),
  })
}
