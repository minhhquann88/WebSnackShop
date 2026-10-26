import Link from 'next/link'
import { ArrowRight } from 'lucide-react'
import { buttonVariants } from '@/components/ui/button'
import { EmptyState, ErrorState } from '@/components/ui/feedback'
import { ProductGrid } from '@/components/storefront/product-card'
import { RemoteImage } from '@/components/ui/remote-image'
import { getCategories, getProducts } from '@/features/catalog/server'

export const revalidate = 60

export default async function HomePage() {
  const [categories, featured, bestSellers] = await Promise.all([
    getCategories(),
    getProducts({ featured: true, pageSize: 8, sort: '-soldCount' }),
    getProducts({ sort: '-createdAt', pageSize: 8 }),
  ])

  return (
    <div className="grid gap-10">
      <section className="grid items-center gap-6 rounded-2xl bg-gradient-to-br from-primary/10 via-accent to-secondary p-6 sm:p-10 md:grid-cols-2">
        <div className="grid gap-4">
          <h1 className="text-3xl leading-tight font-bold text-balance sm:text-4xl">
            Đồ ăn vặt ngon, <span className="text-primary">giao tận nhà</span>
          </h1>
          <p className="text-muted-foreground">
            Khô gà lá chanh, bánh tráng, hạt sấy và nhiều món ăn vặt chọn lọc. Đặt hàng không cần tài khoản,
            thanh toán COD hoặc chuyển khoản.
          </p>
          <div className="flex flex-wrap gap-2">
            <Link href="/san-pham" className={buttonVariants({ size: 'lg' })}>
              Mua ngay
            </Link>
            <Link href="/tra-cuu-don" className={buttonVariants({ size: 'lg', variant: 'outline' })}>
              Tra cứu đơn
            </Link>
          </div>
        </div>
        <div className="hidden text-center text-8xl md:block" aria-hidden>
          🍿🥜🍘
        </div>
      </section>

      <section aria-labelledby="categories-heading" className="grid gap-4">
        <h2 id="categories-heading" className="text-xl font-semibold">
          Danh mục
        </h2>
        {categories === null ? (
          <ErrorState message="Không tải được danh mục. Vui lòng thử lại sau." />
        ) : categories.length === 0 ? (
          <EmptyState title="Chưa có danh mục" />
        ) : (
          <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5">
            {categories.map((c) => (
              <li key={c.id}>
                <Link
                  href={`/danh-muc/${c.slug}`}
                  className="flex h-full items-center gap-3 rounded-xl border bg-card p-3 transition-colors hover:border-primary hover:bg-accent"
                >
                  {c.imageUrl ? (
                    <RemoteImage src={c.imageUrl} alt="" className="size-12 rounded-lg" />
                  ) : (
                    <span
                      className="flex size-12 shrink-0 items-center justify-center rounded-lg bg-secondary text-xl"
                      aria-hidden
                    >
                      {c.name.charAt(0)}
                    </span>
                  )}
                  <span className="grid">
                    <span className="text-sm font-medium">{c.name}</span>
                    <span className="text-xs text-muted-foreground">{c.productCount} sản phẩm</span>
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>

      <ProductSection
        title="Sản phẩm nổi bật"
        id="featured"
        data={featured}
        href="/san-pham?sort=-soldCount"
      />
      <ProductSection title="Hàng mới về" id="newest" data={bestSellers} href="/san-pham?sort=-createdAt" />
    </div>
  )
}

function ProductSection({
  title,
  id,
  data,
  href,
}: {
  title: string
  id: string
  data: Awaited<ReturnType<typeof getProducts>>
  href: string
}) {
  return (
    <section aria-labelledby={`${id}-heading`} className="grid gap-4">
      <div className="flex items-center justify-between gap-2">
        <h2 id={`${id}-heading`} className="text-xl font-semibold">
          {title}
        </h2>
        <Link href={href} className={buttonVariants({ variant: 'link', size: 'sm' })}>
          Xem tất cả <ArrowRight aria-hidden />
        </Link>
      </div>
      {data === null ? (
        <ErrorState message="Không tải được sản phẩm. Vui lòng thử lại sau." />
      ) : data.items.length === 0 ? (
        <EmptyState title="Chưa có sản phẩm" />
      ) : (
        <ProductGrid products={data.items} />
      )}
    </section>
  )
}
