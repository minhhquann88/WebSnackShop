import type { Metadata } from 'next'
import Link from 'next/link'
import { notFound } from 'next/navigation'
import { ErrorState } from '@/components/ui/feedback'
import { ProductGallery } from '@/components/storefront/product-gallery'
import { ProductPurchase } from '@/components/storefront/product-purchase'
import { getProduct } from '@/features/catalog/server'
import { formatNumber } from '@/lib/format'

type Params = Promise<{ slug: string }>

export const revalidate = 60

export async function generateMetadata({ params }: { params: Params }): Promise<Metadata> {
  const { slug } = await params
  const product = await getProduct(slug)
  if (!product) return { title: 'Sản phẩm' }
  const image = product.images.find((i) => i.isPrimary) ?? product.images[0]
  return {
    title: product.name,
    description: product.shortDescription ?? undefined,
    openGraph: {
      title: product.name,
      description: product.shortDescription ?? undefined,
      images: image ? [image.url] : [],
    },
  }
}

export default async function ProductPage({ params }: { params: Params }) {
  const { slug } = await params
  const product = await getProduct(slug)
  if (product === undefined) notFound()
  if (product === null) return <ErrorState message="Không tải được sản phẩm. Vui lòng thử lại sau." />

  const facts: [string, string | null][] = [
    ['Thương hiệu', product.brand],
    ['Xuất xứ', product.origin],
    ['Thành phần', product.ingredients],
    ['Hạn sử dụng', product.shelfLifeDays ? `${product.shelfLifeDays} ngày` : null],
  ]

  return (
    <div className="grid gap-8">
      <nav aria-label="Breadcrumb" className="text-sm text-muted-foreground">
        <ol className="flex flex-wrap items-center gap-1">
          <li>
            <Link href="/" className="hover:text-foreground">
              Trang chủ
            </Link>
          </li>
          <li aria-hidden>/</li>
          <li>
            <Link href={`/danh-muc/${product.category.slug}`} className="hover:text-foreground">
              {product.category.name}
            </Link>
          </li>
          <li aria-hidden>/</li>
          <li aria-current="page" className="text-foreground">
            {product.name}
          </li>
        </ol>
      </nav>

      <div className="grid gap-6 md:grid-cols-2 md:gap-10">
        <ProductGallery images={product.images} name={product.name} />
        <div className="grid content-start gap-4">
          <div className="grid gap-2">
            <h1 className="text-2xl font-bold sm:text-3xl">{product.name}</h1>
            {product.shortDescription ? (
              <p className="text-muted-foreground">{product.shortDescription}</p>
            ) : null}
            {product.soldCount > 0 ? (
              <p className="text-sm text-muted-foreground">Đã bán {formatNumber(product.soldCount)}</p>
            ) : null}
          </div>
          <ProductPurchase product={product} />
        </div>
      </div>

      <section aria-labelledby="desc-heading" className="grid gap-4 md:grid-cols-3">
        <div className="grid content-start gap-2 md:col-span-2">
          <h2 id="desc-heading" className="text-lg font-semibold">
            Mô tả sản phẩm
          </h2>
          <p className="leading-relaxed whitespace-pre-line text-muted-foreground">
            {product.description ?? product.shortDescription ?? 'Đang cập nhật.'}
          </p>
        </div>
        <dl className="grid content-start gap-3 rounded-xl border p-4 text-sm">
          {facts
            .filter((f): f is [string, string] => !!f[1])
            .map(([label, value]) => (
              <div key={label} className="grid gap-0.5">
                <dt className="text-muted-foreground">{label}</dt>
                <dd className="font-medium">{value}</dd>
              </div>
            ))}
        </dl>
      </section>
    </div>
  )
}
