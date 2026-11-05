import Link from 'next/link'
import { ArrowRight } from 'lucide-react'
import { buttonVariants } from '@/components/ui/button'
import { EmptyState, ErrorState } from '@/components/ui/feedback'
import { RemoteImage } from '@/components/ui/remote-image'
import { ProductGrid } from '@/components/storefront/product-card'
import { getCategories, getProducts, getPublicSettings } from '@/features/catalog/server'
import { formatNumber } from '@/lib/format'
import { toneAt, toneClasses } from '@/lib/tone'
import { cn } from '@/lib/utils'

export const revalidate = 60

/** Compact VND for the starburst: 300.000 ₫ → "300K". */
function burstAmount(amount: number): string {
  return amount >= 1_000_000
    ? `${(amount / 1_000_000).toLocaleString('vi-VN', { maximumFractionDigits: 1 })}TR`
    : `${Math.round(amount / 1_000)}K`
}

export default async function HomePage() {
  const [categories, featured, newest, settings] = await Promise.all([
    getCategories(),
    getProducts({ featured: true, pageSize: 5, sort: '-soldCount' }),
    getProducts({ sort: '-createdAt', pageSize: 8 }),
    getPublicSettings(),
  ])
  const hero = featured?.items[0]
  const threshold = settings?.freeShippingThreshold ?? null

  return (
    <div className="grid gap-12">
      {/* First viewport: the big bag face and the category bags beside it. */}
      <section aria-labelledby="hero-heading" className="grid gap-4 lg:grid-cols-[3fr_2fr] lg:gap-6">
        <div className="bag relative flex min-h-[440px] flex-col justify-end bg-chili p-7 text-primary-foreground sm:p-10 lg:min-h-[520px]">
          <div className="absolute top-6 right-6 sm:top-8 sm:right-8">
            <span className="burst size-36 bg-mango p-5 font-display text-base leading-tight font-extrabold text-foreground sm:size-44 sm:text-lg">
              {threshold ? (
                <>
                  FREESHIP
                  <br />
                  TỪ {burstAmount(threshold.amount)}
                </>
              ) : (
                <>
                  COD
                  <br />
                  HOẶC QR
                </>
              )}
            </span>
          </div>
          <div className="relative z-10 grid max-w-md gap-5">
            <h1
              id="hero-heading"
              className="font-display text-5xl leading-[0.95] font-extrabold overprint sm:text-6xl lg:text-7xl"
            >
              Tối nay ăn vặt gì?
            </h1>
            <p className="max-w-md text-base text-primary-foreground sm:text-lg">
              Khô gà, bánh tráng, hạt sấy, kẹo bánh. Đặt không cần tài khoản, COD hoặc quét VietQR.
            </p>
            <div className="flex flex-wrap gap-2">
              <Link href="/san-pham" className={buttonVariants({ size: 'lg', variant: 'secondary' })}>
                Xem thực đơn
              </Link>
            </div>
          </div>
          {hero ? (
            <Link
              href={`/san-pham/${hero.slug}`}
              aria-label={`Xem ${hero.name}`}
              className="absolute -right-8 -bottom-10 hidden size-56 rounded-full ring-8 ring-white/25 transition-transform duration-300 ease-out hover:scale-105 focus-visible:ring-mango focus-visible:outline-none sm:block lg:size-72"
            >
              <RemoteImage src={hero.imageUrl} alt="" className="size-full rounded-full object-cover" />
            </Link>
          ) : null}
        </div>

        <div className="grid gap-3">
          {categories === null ? (
            <ErrorState message="Không tải được danh mục. Vui lòng thử lại sau." />
          ) : categories.length === 0 ? (
            <EmptyState title="Chưa có danh mục" />
          ) : (
            categories.slice(0, 5).map((c, index) => {
              const cream = index === 4
              const t = toneClasses[toneAt(index)]
              return (
                <Link
                  key={c.id}
                  href={`/danh-muc/${c.slug}`}
                  className={cn(
                    'bag flex min-h-[84px] items-center justify-between gap-3 px-5 py-4 lift focus-visible:ring-[3px] focus-visible:ring-ring/40 focus-visible:outline-none',
                    cream ? 'border-[3px] border-chili bg-card text-chili-deep' : cn(t.bg, t.text),
                  )}
                >
                  <span className="absolute inset-x-0 top-0 h-4 stripes-sm crimp" aria-hidden />
                  <span className="grid gap-0.5">
                    <span className="font-display text-xl leading-tight font-extrabold sm:text-2xl">
                      {c.name}
                    </span>
                    <span className={cn('text-xs font-semibold', cream ? 'text-muted-foreground' : t.muted)}>
                      {formatNumber(c.productCount)} sản phẩm
                    </span>
                  </span>
                  <ArrowRight className="size-5 shrink-0" aria-hidden />
                </Link>
              )
            })
          )}
        </div>
      </section>

      <section aria-labelledby="featured-heading" className="grid gap-4">
        <div className="flex items-baseline justify-between gap-2">
          <h2 id="featured-heading" className="font-display text-3xl leading-none font-extrabold">
            Món bán chạy
          </h2>
          <Link href="/san-pham?sort=-soldCount" className={buttonVariants({ variant: 'link', size: 'sm' })}>
            Xem tất cả <ArrowRight aria-hidden />
          </Link>
        </div>
        {featured === null ? (
          <ErrorState message="Không tải được sản phẩm. Vui lòng thử lại sau." />
        ) : featured.items.length === 0 ? (
          <EmptyState title="Chưa có sản phẩm nổi bật" />
        ) : (
          <ProductGrid products={featured.items} className="lg:grid-cols-5" />
        )}
      </section>

      <section aria-labelledby="newest-heading" className="grid gap-4">
        <div className="flex items-baseline justify-between gap-2">
          <h2 id="newest-heading" className="font-display text-3xl leading-none font-extrabold">
            Hàng mới về
          </h2>
          <Link href="/san-pham?sort=-createdAt" className={buttonVariants({ variant: 'link', size: 'sm' })}>
            Xem tất cả <ArrowRight aria-hidden />
          </Link>
        </div>
        {newest === null ? (
          <ErrorState message="Không tải được sản phẩm. Vui lòng thử lại sau." />
        ) : newest.items.length === 0 ? (
          <EmptyState title="Chưa có sản phẩm" />
        ) : (
          <ProductGrid products={newest.items} />
        )}
      </section>
    </div>
  )
}
