import type { Metadata } from 'next'
import Link from 'next/link'
import { notFound } from 'next/navigation'
import { ProductListing } from '@/components/storefront/product-listing'
import { getCategories, normalisePage, normaliseSort } from '@/features/catalog/server'
import { formatNumber } from '@/lib/format'
import { toneAt, toneClasses } from '@/lib/tone'
import { cn } from '@/lib/utils'

type Params = Promise<{ slug: string }>
type SearchParams = Promise<{ sort?: string; page?: string }>

export async function generateMetadata({ params }: { params: Params }): Promise<Metadata> {
  const { slug } = await params
  const category = (await getCategories())?.find((c) => c.slug === slug)
  return category
    ? { title: category.name, description: category.description ?? undefined }
    : { title: 'Danh mục' }
}

export default async function CategoryPage({
  params,
  searchParams,
}: {
  params: Params
  searchParams: SearchParams
}) {
  const [{ slug }, query] = await Promise.all([params, searchParams])
  const categories = await getCategories()
  const categoryIndex = categories?.findIndex((c) => c.slug === slug) ?? -1
  const category = categoryIndex >= 0 ? categories?.[categoryIndex] : undefined
  // Unknown slug with a working API → 404; if the API is down, still render (listing shows the error state).
  if (categories && !category) notFound()
  const tone = toneAt(Math.max(categoryIndex, 0))
  const c = toneClasses[tone]

  return (
    <div className="grid gap-6">
      <nav aria-label="Breadcrumb" className="text-sm text-muted-foreground">
        <ol className="flex flex-wrap items-center gap-1">
          <li>
            <Link href="/" className="hover:text-chili-deep hover:underline">
              Trang chủ
            </Link>
          </li>
          <li aria-hidden>/</li>
          <li aria-current="page" className="text-foreground">
            {category?.name ?? slug}
          </li>
        </ol>
      </nav>
      {/* The shelf label: this category's own bag, printed in its colour. */}
      <div className={cn('bag grid gap-2 px-6 pt-9 pb-6 sm:px-8 sm:pt-10 sm:pb-8', c.bg, c.text)}>
        <span className="absolute inset-x-0 top-0 h-5 stripes-sm crimp" aria-hidden />
        <h1 className="font-display text-4xl leading-none font-extrabold overprint-sm sm:text-5xl">
          {category?.name ?? 'Danh mục'}
        </h1>
        {category?.description ? <p className={cn('max-w-prose', c.muted)}>{category.description}</p> : null}
        {category ? (
          <p className={cn('text-sm font-bold', c.muted)}>{formatNumber(category.productCount)} sản phẩm</p>
        ) : null}
      </div>
      <ProductListing
        basePath={`/danh-muc/${slug}`}
        categorySlug={slug}
        sort={normaliseSort(query.sort)}
        page={normalisePage(query.page)}
        tone={categoryIndex >= 0 ? tone : undefined}
      />
    </div>
  )
}
