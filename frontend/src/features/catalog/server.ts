import 'server-only'
import { cache } from 'react'
import { publicApi } from '@/lib/api/server'
import type { Schemas } from '@/lib/api/types'

/** Sort keys accepted by `GET /products` (validator whitelist). */
export const PRODUCT_SORTS = [
  { value: '-createdAt', label: 'Mới nhất' },
  { value: '-soldCount', label: 'Bán chạy' },
  { value: 'price', label: 'Giá tăng dần' },
  { value: '-price', label: 'Giá giảm dần' },
  { value: 'name', label: 'Tên A → Z' },
] as const

const SORT_VALUES = new Set<string>([
  'createdAt',
  '-createdAt',
  'price',
  '-price',
  'soldCount',
  '-soldCount',
  'name',
  '-name',
])

export function normaliseSort(value: string | undefined): string | undefined {
  return value && SORT_VALUES.has(value) ? value : undefined
}

export function normalisePage(value: string | undefined): number {
  const page = Number.parseInt(value ?? '1', 10)
  return Number.isFinite(page) && page > 0 ? Math.min(page, 1000) : 1
}

/** Catalog reads for Server Components (ISR, revalidate 60 s). Return null on failure so pages can degrade. */
export const getCategories = cache(async (): Promise<Schemas['CategoryResponse'][] | null> => {
  try {
    const { data } = await publicApi.GET('/api/v1/categories')
    return data ?? null
  } catch {
    return null
  }
})

export async function getProducts(query: {
  categorySlug?: string
  search?: string
  sort?: string
  featured?: boolean
  page?: number
  pageSize?: number
}): Promise<Schemas['PagedResponseOfProductSummaryResponse'] | null> {
  try {
    const { data } = await publicApi.GET('/api/v1/products', {
      params: {
        query: {
          categorySlug: query.categorySlug,
          search: query.search,
          sort: query.sort,
          featured: query.featured,
          page: query.page,
          pageSize: query.pageSize,
        },
      },
    })
    return data ?? null
  } catch {
    return null
  }
}

/** `undefined` = not found (404), `null` = API unavailable. */
export const getProduct = cache(
  async (slug: string): Promise<Schemas['ProductDetailResponse'] | null | undefined> => {
    try {
      const { data, response } = await publicApi.GET('/api/v1/products/{slug}', {
        params: { path: { slug } },
      })
      if (response.status === 404 || response.status === 400) return undefined
      return data ?? null
    } catch {
      return null
    }
  },
)

export const getPublicSettings = cache(async (): Promise<Schemas['PublicSettingsResponse'] | null> => {
  try {
    const { data } = await publicApi.GET('/api/v1/settings/public')
    return data ?? null
  } catch {
    return null
  }
})
