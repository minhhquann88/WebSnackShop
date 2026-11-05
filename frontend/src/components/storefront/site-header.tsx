'use client'

import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { useEffect, useRef, useState } from 'react'
import { LayoutDashboard, LogOut, Search, ShoppingBag, User } from 'lucide-react'
import { Button, buttonVariants } from '@/components/ui/button'
import { useCart } from '@/features/cart/use-cart'
import { isStaff, useAuthStore } from '@/lib/auth/auth.store'
import { logout } from '@/lib/auth/session'
import { toneAt, toneClasses } from '@/lib/tone'
import { cn } from '@/lib/utils'

type HeaderCategory = { slug: string; name: string }

/**
 * The stripe band across the top of the bag: the name printed in display type with an overprint
 * shadow, a white search field, the cart as a mango pill that bumps when the count changes.
 * Categories are a row of printed pills under the band, each in its packaging colour.
 */
export function SiteHeader({ categories }: { categories: HeaderCategory[] }) {
  const router = useRouter()
  const pathname = usePathname()
  const { itemCount } = useCart()
  const status = useAuthStore((s) => s.status)
  const user = useAuthStore((s) => s.user)
  const [bump, setBump] = useState(false)
  const previousCount = useRef(itemCount)

  useEffect(() => {
    if (itemCount > previousCount.current) {
      setBump(true)
      const t = setTimeout(() => setBump(false), 400)
      previousCount.current = itemCount
      return () => clearTimeout(t)
    }
    previousCount.current = itemCount
  }, [itemCount])

  function onSearch(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const value = new FormData(event.currentTarget).get('q')
    const q = typeof value === 'string' ? value.trim() : ''
    router.push(q ? `/san-pham?search=${encodeURIComponent(q)}` : '/san-pham')
  }

  async function onLogout() {
    await logout()
    router.push('/')
    router.refresh()
  }

  const tabs = [
    { href: '/san-pham', label: 'Tất cả' },
    ...categories.map((c) => ({ href: `/danh-muc/${c.slug}`, label: c.name })),
  ]

  const searchForm = (id: string, className?: string) => (
    <form role="search" onSubmit={onSearch} className={className}>
      <label htmlFor={id} className="sr-only">
        Tìm sản phẩm
      </label>
      <div className="relative">
        <Search
          className="pointer-events-none absolute top-1/2 left-3.5 size-4 -translate-y-1/2 text-muted-foreground"
          aria-hidden
        />
        <input
          id={id}
          name="q"
          type="search"
          placeholder="Tìm khô gà, bánh tráng..."
          className="h-11 w-full rounded-full border-2 border-transparent bg-card pr-4 pl-10 text-sm text-foreground outline-none placeholder:text-muted-foreground focus-visible:border-mango"
        />
      </div>
    </form>
  )

  return (
    <header className="z-40 md:sticky md:top-0">
      <div className="stripes text-primary-foreground">
        <div className="mx-auto flex h-[72px] max-w-6xl items-center gap-4 px-5 sm:px-6">
          <Link href="/" className="font-display text-3xl leading-none font-extrabold overprint sm:text-4xl">
            SnackShop
          </Link>

          {searchForm('header-search', 'ml-2 hidden max-w-md flex-1 md:block')}

          <nav aria-label="Tài khoản và giỏ hàng" className="ml-auto flex items-center gap-1">
            {status === 'authenticated' && user ? (
              <>
                {isStaff(user) ? (
                  <Link
                    href="/admin"
                    className={cn(
                      buttonVariants({ variant: 'ghost', size: 'icon' }),
                      'text-primary-foreground hover:bg-chili-deep hover:text-primary-foreground',
                    )}
                    aria-label="Trang quản trị"
                  >
                    <LayoutDashboard aria-hidden />
                  </Link>
                ) : null}
                <Link
                  href="/tai-khoan"
                  className={cn(
                    buttonVariants({ variant: 'ghost' }),
                    'hidden px-3 text-primary-foreground hover:bg-chili-deep hover:text-primary-foreground sm:inline-flex',
                  )}
                  aria-label="Tài khoản của tôi"
                >
                  <User aria-hidden />
                  <span className="max-w-32 truncate">{user.fullName}</span>
                </Link>
                <Link
                  href="/tai-khoan"
                  className={cn(
                    buttonVariants({ variant: 'ghost', size: 'icon' }),
                    'text-primary-foreground hover:bg-chili-deep hover:text-primary-foreground sm:hidden',
                  )}
                  aria-label="Tài khoản của tôi"
                >
                  <User aria-hidden />
                </Link>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={onLogout}
                  aria-label="Đăng xuất"
                  className="text-primary-foreground hover:bg-chili-deep hover:text-primary-foreground"
                >
                  <LogOut aria-hidden />
                </Button>
              </>
            ) : status === 'anonymous' ? (
              <Link
                href={`/dang-nhap?returnUrl=${encodeURIComponent(pathname)}`}
                className={cn(
                  buttonVariants({ variant: 'ghost', size: 'sm' }),
                  'text-primary-foreground hover:bg-chili-deep hover:text-primary-foreground',
                )}
              >
                <User aria-hidden />
                Đăng nhập
              </Link>
            ) : null}
            <Link
              href="/gio-hang"
              className={cn(buttonVariants({ variant: 'secondary' }), 'ml-1 gap-2 px-4', bump && 'bump')}
              aria-label={`Giỏ hàng, ${itemCount} sản phẩm`}
            >
              <ShoppingBag aria-hidden />
              <span className="tabular-nums">{itemCount > 99 ? '99+' : itemCount}</span>
            </Link>
          </nav>
        </div>
        {searchForm('mobile-search', 'px-5 pb-3 md:hidden')}
      </div>

      <nav
        aria-label="Danh mục"
        className="border-b bg-cream/95 backdrop-blur supports-[backdrop-filter]:bg-cream/85"
      >
        <ul className="mx-auto flex max-w-6xl [scrollbar-width:none] items-center gap-2 overflow-x-auto px-5 py-2.5 sm:px-6 [&::-webkit-scrollbar]:hidden">
          {tabs.map((tab, index) => {
            const active = pathname === tab.href
            const tone = toneClasses[toneAt(index + 3)]
            return (
              <li key={tab.href} className="shrink-0">
                <Link
                  href={tab.href}
                  aria-current={active ? 'page' : undefined}
                  className={cn(
                    'inline-flex h-9 items-center rounded-full border-2 px-4 text-sm font-bold whitespace-nowrap transition-colors',
                    active
                      ? cn(tone.bg, tone.text, 'border-transparent')
                      : 'border-foreground/15 bg-card text-foreground hover:border-foreground/60',
                  )}
                >
                  {tab.label}
                </Link>
              </li>
            )
          })}
          <li className="ml-auto shrink-0 pl-2">
            <Link href="/tra-cuu-don" className={buttonVariants({ variant: 'link', size: 'sm' })}>
              Tra cứu đơn hàng
            </Link>
          </li>
        </ul>
      </nav>
    </header>
  )
}
