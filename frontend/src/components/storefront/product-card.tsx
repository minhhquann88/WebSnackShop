import Link from 'next/link'
import { RemoteImage } from '@/components/ui/remote-image'
import type { ProductSummary } from '@/lib/api/types'
import { formatNumber } from '@/lib/format'
import { toneClasses, toneFor, type Tone } from '@/lib/tone'
import { cn } from '@/lib/utils'
import { PriceStamp } from './price-tag'

/** A flag printed on the bag face: a small burst seal in the bag's own colour vocabulary. */
export function Seal({
  tone,
  children,
  className,
}: {
  tone: Tone
  children: React.ReactNode
  className?: string
}) {
  return (
    <span
      className={cn(
        'seal burst size-11 text-[0.5rem] sm:size-14 sm:text-[0.6rem]',
        tone === 'mango' ? 'bg-chili text-primary-foreground' : 'bg-mango text-foreground',
        className,
      )}
    >
      {children}
    </span>
  )
}

/**
 * A snack bag on the shelf: the whole card is a colour field with a crimped stripe seal along the
 * top, the photo sits on it as a disc, the price is a round stamp at the top-right, and the
 * "Chọn mua" pill opens the product (variants are chosen there). Hover lifts the bag off the shelf.
 */
export function ProductCard({ product, tone }: { product: ProductSummary; tone?: Tone }) {
  const t = tone ?? toneFor(product.id)
  const c = toneClasses[t]
  const hasSeal = !product.inStock || product.isFeatured
  return (
    <Link
      href={`/san-pham/${product.slug}`}
      className={cn(
        'group bag flex h-full w-full flex-col gap-3 p-4 pt-7 lift focus-visible:ring-[3px] focus-visible:ring-ring/40 focus-visible:outline-none',
        c.bg,
        c.text,
      )}
    >
      <span className="absolute inset-x-0 top-0 h-4 stripes-sm crimp" aria-hidden />
      <div className="relative pr-14 sm:pr-16">
        <RemoteImage
          src={product.imageUrl}
          alt=""
          className="size-24 rounded-full bg-card object-cover ring-4 ring-white/60 sm:size-28"
        />
        <PriceStamp
          price={product.price}
          compareAtPrice={product.compareAtPrice}
          className={cn(
            'absolute top-0 right-0 z-10 size-[4.25rem] text-[11px] whitespace-nowrap sm:size-20 sm:text-sm',
            c.rim,
          )}
        />
      </div>
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <h3 className="line-clamp-2 text-base leading-snug font-bold sm:text-lg">{product.name}</h3>
        {product.shortDescription ? (
          <p className={cn('line-clamp-2 hidden text-xs sm:block', c.muted, hasSeal && 'sm:pr-16')}>
            {product.shortDescription}
          </p>
        ) : null}
      </div>
      <div className={cn('mt-auto flex items-center justify-between gap-2', hasSeal && 'pr-12 sm:pr-16')}>
        <span
          className={cn(
            'inline-flex h-8 items-center rounded-full px-3.5 text-xs font-extrabold whitespace-nowrap transition-colors',
            c.pill,
          )}
        >
          {product.inStock ? 'Chọn mua' : 'Xem'}
        </span>
        {!product.inStock ? (
          <Seal tone={t} className="absolute right-3 bottom-3 bg-card text-muted-foreground">
            Hết hàng
          </Seal>
        ) : product.isFeatured ? (
          <Seal tone={t} className="absolute right-3 bottom-3">
            Bán chạy
          </Seal>
        ) : product.soldCount > 0 ? (
          <span className={cn('hidden text-xs font-semibold sm:inline', c.muted)}>
            Đã bán {formatNumber(product.soldCount)}
          </span>
        ) : null}
      </div>
    </Link>
  )
}

export function ProductGrid({
  products,
  tone,
  className,
}: {
  products: ProductSummary[]
  tone?: Tone
  className?: string
}) {
  return (
    <ul className={cn('grid grid-cols-2 gap-3 sm:gap-4 md:grid-cols-3 lg:grid-cols-4', className)}>
      {products.map((p) => (
        <li key={p.id} className="flex">
          <ProductCard product={p} tone={tone} />
        </li>
      ))}
    </ul>
  )
}
