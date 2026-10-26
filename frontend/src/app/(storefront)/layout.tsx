import Link from 'next/link'
import { SiteHeader } from '@/components/storefront/site-header'
import { getCategories, getPublicSettings } from '@/features/catalog/server'
import { formatVnd } from '@/lib/format'

export default async function StorefrontLayout({ children }: { children: React.ReactNode }) {
  const [categories, settings] = await Promise.all([getCategories(), getPublicSettings()])
  return (
    <div className="flex min-h-dvh flex-col">
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50 focus:rounded focus:bg-background focus:px-3 focus:py-2"
      >
        Bỏ qua đến nội dung
      </a>
      {settings?.freeShippingThreshold ? (
        <p className="bg-primary px-4 py-1.5 text-center text-xs font-medium text-primary-foreground sm:text-sm">
          Miễn phí vận chuyển cho đơn từ {formatVnd(settings.freeShippingThreshold)}
        </p>
      ) : null}
      <SiteHeader categories={(categories ?? []).map((c) => ({ slug: c.slug, name: c.name }))} />
      <main id="main" className="mx-auto w-full max-w-6xl flex-1 px-4 py-6 sm:py-8">
        {children}
      </main>
      <footer className="border-t bg-muted/40">
        <div className="mx-auto grid max-w-6xl gap-6 px-4 py-8 text-sm sm:grid-cols-3">
          <div className="grid gap-1">
            <p className="font-semibold">{settings?.shopName ?? 'SnackShop'}</p>
            {settings ? (
              <>
                <p className="text-muted-foreground">{settings.shopAddress}</p>
                <p className="text-muted-foreground">
                  Hotline: <a href={`tel:${settings.shopPhone}`}>{settings.shopPhone}</a>
                </p>
              </>
            ) : null}
          </div>
          <nav aria-label="Hỗ trợ" className="grid content-start gap-1">
            <p className="font-semibold">Hỗ trợ</p>
            <Link href="/tra-cuu-don" className="text-muted-foreground hover:text-foreground">
              Tra cứu đơn hàng
            </Link>
            <Link href="/tai-khoan/don-hang" className="text-muted-foreground hover:text-foreground">
              Đơn hàng của tôi
            </Link>
          </nav>
          <div className="grid content-start gap-1">
            <p className="font-semibold">Thanh toán</p>
            <p className="text-muted-foreground">COD - thanh toán khi nhận hàng</p>
            <p className="text-muted-foreground">Chuyển khoản ngân hàng (VietQR)</p>
          </div>
        </div>
        <p className="border-t py-4 text-center text-xs text-muted-foreground">
          © {new Date().getFullYear()} SnackShop. Đồ ăn vặt ngon mỗi ngày.
        </p>
      </footer>
    </div>
  )
}
