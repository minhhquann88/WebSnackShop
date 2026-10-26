import { Suspense } from 'react'
import { EmptyState, ErrorState } from '@/components/ui/feedback'
import { LinkPagination, totalPagesOf } from '@/components/ui/pagination'
import { PRODUCT_SORTS, getProducts } from '@/features/catalog/server'
import { formatNumber } from '@/lib/format'
import { ProductGrid } from './product-card'
import { SortSelect } from './sort-select'

const PAGE_SIZE = 20

type ProductListingProps = {
  basePath: string
  categorySlug?: string
  search?: string
  sort?: string
  page: number
}

/** Server-rendered, paginated product grid shared by category and search pages. */
export async function ProductListing({ basePath, categorySlug, search, sort, page }: ProductListingProps) {
  const effectiveSort = sort ?? '-createdAt'
  const data = await getProducts({ categorySlug, search, sort: effectiveSort, page, pageSize: PAGE_SIZE })

  if (data === null) return <ErrorState message="Không tải được sản phẩm. Vui lòng thử lại sau." />

  const totalPages = totalPagesOf(data)
  const hrefForPage = (p: number) => {
    const params = new URLSearchParams()
    if (search) params.set('search', search)
    if (sort) params.set('sort', sort)
    if (p > 1) params.set('page', String(p))
    const qs = params.toString()
    return qs ? `${basePath}?${qs}` : basePath
  }

  return (
    <div className="grid gap-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-muted-foreground" aria-live="polite">
          {formatNumber(data.totalCount)} sản phẩm
        </p>
        <Suspense>
          <SortSelect options={PRODUCT_SORTS} value={effectiveSort} />
        </Suspense>
      </div>
      {data.items.length === 0 ? (
        <EmptyState
          title="Không tìm thấy sản phẩm"
          description={
            search ? `Không có sản phẩm nào khớp với "${search}". Thử từ khóa khác nhé.` : undefined
          }
        />
      ) : (
        <ProductGrid products={data.items} />
      )}
      <LinkPagination page={data.page} totalPages={totalPages} hrefForPage={hrefForPage} />
    </div>
  )
}
