import Link from 'next/link'
import { SiteHeader } from '@/components/storefront/site-header'
import { getCategories, getPublicSettings } from '@/features/catalog/server'
import { formatVnd } from '@/lib/format'

/**
 * The storefront is a shelf: cream ground, the stripe band on top, white cartons and coloured
 * bags on it, and the back of the bag (chili field) as the footer.
 */
export default async function StorefrontLayout({ children }: { children: React.ReactNode }) {
  const [categories, settings] = await Promise.all([getCategories(), getPublicSettings()])
  return (
    <div className="flex min-h-dvh flex-col">
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50 focus:rounded-full focus:bg-mango focus:px-4 focus:py-2 focus:font-bold"
      >
        Bỏ qua đến nội dung
      </a>
      <SiteHeader categories={(categories ?? []).map((c) => ({ slug: c.slug, name: c.name }))} />
      <main id="main" className="mx-auto w-full max-w-6xl flex-1 px-5 py-6 sm:px-6 sm:py-8">
        {children}
      </main>
      <footer className="mt-10 bg-chili text-primary-foreground">
        <div className="h-3 stripes-sm" aria-hidden />
        <div className="mx-auto grid max-w-6xl gap-8 px-5 py-10 text-sm sm:grid-cols-3 sm:px-6">
          <div className="grid content-start gap-2">
            <p className="font-display text-3xl leading-none font-extrabold overprint-sm">
              {settings?.shopName ?? 'SnackShop'}
            </p>
            {settings ? (
              <>
                <p className="text-primary-foreground/85">{settings.shopAddress}</p>
                <p className="text-primary-foreground/85">
                  Hotline:{' '}
                  <a
                    href={`tel:${settings.shopPhone}`}
                    className="font-bold text-primary-foreground hover:underline"
                  >
                    {settings.shopPhone}
                  </a>
                </p>
              </>
            ) : null}
          </div>
          <nav aria-label="Hỗ trợ" className="grid content-start gap-2">
            <p className="font-display text-xl font-extrabold">Hỗ trợ</p>
            <Link
              href="/tra-cuu-don"
              className="text-primary-foreground/85 hover:text-primary-foreground hover:underline"
            >
              Tra cứu đơn hàng
            </Link>
            <Link
              href="/tai-khoan/don-hang"
              className="text-primary-foreground/85 hover:text-primary-foreground hover:underline"
            >
              Đơn hàng của tôi
            </Link>
          </nav>
          <div className="grid content-start gap-2">
            <p className="font-display text-xl font-extrabold">Thanh toán & giao hàng</p>
            <p className="text-primary-foreground/85">COD - thanh toán khi nhận hàng</p>
            <p className="text-primary-foreground/85">Chuyển khoản ngân hàng (VietQR)</p>
            <p className="text-primary-foreground/85">
              Chọn món, đặt hàng, giao tận nhà toàn quốc; theo dõi đơn bằng mã đơn.
            </p>
            {settings?.freeShippingThreshold ? (
              <p className="w-fit rounded-full bg-mango px-3 py-1 font-bold text-foreground">
                Freeship từ {formatVnd(settings.freeShippingThreshold)}
              </p>
            ) : null}
          </div>
        </div>
        <p className="border-t border-primary-foreground/20 py-4 text-center text-xs text-primary-foreground/75">
          © {new Date().getFullYear()} SnackShop. Đồ ăn vặt ngon mỗi ngày.
        </p>
      </footer>
    </div>
  )
}
