import type { Metadata } from 'next'
import { ProductListing } from '@/components/storefront/product-listing'
import { normalisePage, normaliseSort } from '@/features/catalog/server'

type SearchParams = Promise<{ search?: string; sort?: string; page?: string }>

export async function generateMetadata({ searchParams }: { searchParams: SearchParams }): Promise<Metadata> {
  const { search } = await searchParams
  return { title: search ? `Tìm kiếm "${search}"` : 'Tất cả sản phẩm' }
}

export default async function ProductsPage({ searchParams }: { searchParams: SearchParams }) {
  const params = await searchParams
  const search = params.search?.trim().slice(0, 100) || undefined
  return (
    <div className="grid gap-6">
      <h1 className="text-2xl font-bold">{search ? `Kết quả cho "${search}"` : 'Tất cả sản phẩm'}</h1>
      <ProductListing
        basePath="/san-pham"
        search={search}
        sort={normaliseSort(params.sort)}
        page={normalisePage(params.page)}
      />
    </div>
  )
}
