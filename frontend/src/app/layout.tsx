import type { Metadata, Viewport } from 'next'
import { Baloo_2, Be_Vietnam_Pro } from 'next/font/google'
import { Providers } from './providers'
import './globals.css'

const beVietnam = Be_Vietnam_Pro({
  subsets: ['latin', 'vietnamese'],
  weight: ['400', '500', '600', '700'],
  variable: '--font-be-vietnam',
  display: 'swap',
})

const baloo = Baloo_2({
  subsets: ['latin', 'vietnamese'],
  weight: ['700', '800'],
  variable: '--font-baloo',
  display: 'swap',
})

export const metadata: Metadata = {
  title: { default: 'SnackShop - Đồ ăn vặt ngon mỗi ngày', template: '%s | SnackShop' },
  description:
    'SnackShop - cửa hàng đồ ăn vặt: khô gà, bánh tráng, hạt sấy, kẹo bánh. Giao hàng toàn quốc, thanh toán COD hoặc chuyển khoản.',
}

export const viewport: Viewport = {
  themeColor: '#ff4d2e',
}

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi" className={`${beVietnam.variable} ${baloo.variable}`}>
      <body className="min-h-dvh font-sans">
        <Providers>{children}</Providers>
      </body>
    </html>
  )
}
