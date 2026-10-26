'use client'

import { useCallback } from 'react'
import { usePathname, useRouter, useSearchParams } from 'next/navigation'

/**
 * Keeps list filters in the URL (`?status=Pending&page=2`) so admin views are shareable and
 * survive reloads. Setting any filter other than `page` resets to page 1.
 */
export function useUrlFilters() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const pathname = usePathname()

  const get = useCallback((key: string): string => searchParams.get(key) ?? '', [searchParams])

  const page = Math.max(1, Number.parseInt(searchParams.get('page') ?? '1', 10) || 1)

  const set = useCallback(
    (updates: Record<string, string | number | boolean | null | undefined>) => {
      const next = new URLSearchParams(searchParams.toString())
      for (const [key, value] of Object.entries(updates)) {
        if (value === null || value === undefined || value === '' || value === false) next.delete(key)
        else next.set(key, String(value))
      }
      if (!('page' in updates)) next.delete('page')
      const qs = next.toString()
      router.replace(qs ? `${pathname}?${qs}` : pathname, { scroll: false })
    },
    [searchParams, router, pathname],
  )

  return { get, set, page }
}
