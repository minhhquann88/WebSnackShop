import Link from 'next/link'
import { Badge } from '@/components/ui/badge'
import { RemoteImage } from '@/components/ui/remote-image'
import type { ProductSummary } from '@/lib/api/types'
import { formatNumber } from '@/lib/format'
import { PriceTag } from './price-tag'

export function ProductCard({ product }: { product: ProductSummary }) {
  return (
    <Link
      href={`/san-pham/${product.slug}`}
      className="group flex w-full flex-col overflow-hidden rounded-xl border bg-card transition-shadow hover:shadow-md focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
    >
      <div className="relative aspect-square overflow-hidden bg-muted">
        <RemoteImage
          src={product.imageUrl}
          alt={product.name}
          className="size-full transition-transform duration-300 group-hover:scale-105"
        />
        <div className="absolute top-2 left-2 flex flex-col gap-1">
          {product.isFeatured ? <Badge>Nổi bật</Badge> : null}
          {!product.inStock ? <Badge variant="muted">Hết hàng</Badge> : null}
        </div>
      </div>
      <div className="flex flex-1 flex-col gap-1.5 p-3">
        <h3 className="line-clamp-2 text-sm font-medium group-hover:text-primary sm:text-base">
          {product.name}
        </h3>
        {product.shortDescription ? (
          <p className="line-clamp-2 hidden text-xs text-muted-foreground sm:block">
            {product.shortDescription}
          </p>
        ) : null}
        <div className="mt-auto flex flex-col gap-1 pt-1">
          <PriceTag price={product.price} compareAtPrice={product.compareAtPrice} />
          {product.soldCount > 0 ? (
            <span className="text-xs text-muted-foreground">Đã bán {formatNumber(product.soldCount)}</span>
          ) : null}
        </div>
      </div>
    </Link>
  )
}

export function ProductGrid({ products }: { products: ProductSummary[] }) {
  return (
    <ul className="grid grid-cols-2 gap-3 sm:gap-4 md:grid-cols-3 lg:grid-cols-4">
      {products.map((p) => (
        <li key={p.id} className="flex">
          <div className="flex w-full">
            <ProductCard product={p} />
          </div>
        </li>
      ))}
    </ul>
  )
}
