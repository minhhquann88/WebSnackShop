import type { Metadata } from 'next'
import Link from 'next/link'
import { notFound } from 'next/navigation'
import { ProductListing } from '@/components/storefront/product-listing'
import { getCategories, normalisePage, normaliseSort } from '@/features/catalog/server'

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
  const category = categories?.find((c) => c.slug === slug)
  // Unknown slug with a working API → 404; if the API is down, still render (listing shows the error state).
  if (categories && !category) notFound()

  return (
    <div className="grid gap-6">
      <nav aria-label="Breadcrumb" className="text-sm text-muted-foreground">
        <ol className="flex flex-wrap items-center gap-1">
          <li>
            <Link href="/" className="hover:text-foreground">
              Trang chủ
            </Link>
          </li>
          <li aria-hidden>/</li>
          <li aria-current="page" className="text-foreground">
            {category?.name ?? slug}
          </li>
        </ol>
      </nav>
      <div className="grid gap-1">
        <h1 className="text-2xl font-bold">{category?.name ?? 'Danh mục'}</h1>
        {category?.description ? <p className="text-muted-foreground">{category.description}</p> : null}
      </div>
      <ProductListing
        basePath={`/danh-muc/${slug}`}
        categorySlug={slug}
        sort={normaliseSort(query.sort)}
        page={normalisePage(query.page)}
      />
    </div>
  )
}
