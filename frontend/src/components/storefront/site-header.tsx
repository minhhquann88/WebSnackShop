'use client'

import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { useState } from 'react'
import { LayoutDashboard, LogOut, Menu, Search, ShoppingBag, User, X } from 'lucide-react'
import { Button, buttonVariants } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useCart } from '@/features/cart/use-cart'
import { isStaff, useAuthStore } from '@/lib/auth/auth.store'
import { logout } from '@/lib/auth/session'
import { cn } from '@/lib/utils'

type HeaderCategory = { slug: string; name: string }

export function SiteHeader({ categories }: { categories: HeaderCategory[] }) {
  const router = useRouter()
  const pathname = usePathname()
  const [menuOpen, setMenuOpen] = useState(false)
  const { itemCount } = useCart()
  const status = useAuthStore((s) => s.status)
  const user = useAuthStore((s) => s.user)

  function onSearch(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const value = new FormData(event.currentTarget).get('q')
    const q = typeof value === 'string' ? value.trim() : ''
    setMenuOpen(false)
    router.push(q ? `/san-pham?search=${encodeURIComponent(q)}` : '/san-pham')
  }

  async function onLogout() {
    await logout()
    router.push('/')
    router.refresh()
  }

  return (
    <header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
      <div className="mx-auto flex h-16 max-w-6xl items-center gap-3 px-4">
        <Button
          variant="ghost"
          size="icon"
          className="md:hidden"
          aria-label={menuOpen ? 'Đóng menu' : 'Mở menu'}
          aria-expanded={menuOpen}
          aria-controls="mobile-menu"
          onClick={() => setMenuOpen((v) => !v)}
        >
          {menuOpen ? <X aria-hidden /> : <Menu aria-hidden />}
        </Button>
        <Link href="/" className="flex items-center gap-1.5 text-lg font-bold text-primary">
          <span aria-hidden>🍿</span>
          <span>SnackShop</span>
        </Link>

        <form role="search" onSubmit={onSearch} className="relative ml-2 hidden max-w-md flex-1 md:block">
          <label htmlFor="header-search" className="sr-only">
            Tìm sản phẩm
          </label>
          <Search
            className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground"
            aria-hidden
          />
          <Input
            id="header-search"
            name="q"
            type="search"
            placeholder="Tìm khô gà, bánh tráng..."
            className="pl-9"
          />
        </form>

        <nav aria-label="Tài khoản và giỏ hàng" className="ml-auto flex items-center gap-1">
          {status === 'authenticated' && user ? (
            <>
              {isStaff(user) ? (
                <Link
                  href="/admin"
                  className={buttonVariants({ variant: 'ghost', size: 'icon' })}
                  aria-label="Trang quản trị"
                >
                  <LayoutDashboard aria-hidden />
                </Link>
              ) : null}
              <Link
                href="/tai-khoan"
                className={cn(buttonVariants({ variant: 'ghost' }), 'hidden px-3 sm:inline-flex')}
                aria-label="Tài khoản của tôi"
              >
                <User aria-hidden />
                <span className="max-w-32 truncate">{user.fullName}</span>
              </Link>
              <Link
                href="/tai-khoan"
                className={cn(buttonVariants({ variant: 'ghost', size: 'icon' }), 'sm:hidden')}
                aria-label="Tài khoản của tôi"
              >
                <User aria-hidden />
              </Link>
              <Button variant="ghost" size="icon" onClick={onLogout} aria-label="Đăng xuất">
                <LogOut aria-hidden />
              </Button>
            </>
          ) : status === 'anonymous' ? (
            <Link
              href={`/dang-nhap?returnUrl=${encodeURIComponent(pathname)}`}
              className={buttonVariants({ variant: 'ghost', size: 'sm' })}
            >
              <User aria-hidden />
              Đăng nhập
            </Link>
          ) : null}
          <Link
            href="/gio-hang"
            className={cn(buttonVariants({ variant: 'outline', size: 'icon' }), 'relative')}
            aria-label={`Giỏ hàng, ${itemCount} sản phẩm`}
          >
            <ShoppingBag aria-hidden />
            {itemCount > 0 ? (
              <span className="absolute -top-1.5 -right-1.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1 text-xs font-semibold text-primary-foreground">
                {itemCount > 99 ? '99+' : itemCount}
              </span>
            ) : null}
          </Link>
        </nav>
      </div>

      <nav aria-label="Danh mục" className="hidden border-t md:block">
        <ul className="mx-auto flex max-w-6xl gap-1 overflow-x-auto px-4 py-1.5 text-sm">
          <li>
            <Link
              href="/san-pham"
              className={cn(
                buttonVariants({ variant: 'ghost', size: 'sm' }),
                pathname === '/san-pham' && 'bg-accent',
              )}
            >
              Tất cả
            </Link>
          </li>
          {categories.map((c) => (
            <li key={c.slug}>
              <Link
                href={`/danh-muc/${c.slug}`}
                className={cn(
                  buttonVariants({ variant: 'ghost', size: 'sm' }),
                  pathname === `/danh-muc/${c.slug}` && 'bg-accent',
                )}
              >
                {c.name}
              </Link>
            </li>
          ))}
          <li className="ml-auto">
            <Link href="/tra-cuu-don" className={buttonVariants({ variant: 'ghost', size: 'sm' })}>
              Tra cứu đơn hàng
            </Link>
          </li>
        </ul>
      </nav>

      {menuOpen ? (
        <div id="mobile-menu" className="border-t md:hidden">
          <form role="search" onSubmit={onSearch} className="relative px-4 pt-3">
            <label htmlFor="mobile-search" className="sr-only">
              Tìm sản phẩm
            </label>
            <Input id="mobile-search" name="q" type="search" placeholder="Tìm sản phẩm..." />
          </form>
          <ul className="grid gap-1 p-2" onClick={() => setMenuOpen(false)}>
            <li>
              <Link href="/san-pham" className="block rounded-md px-3 py-2 hover:bg-accent">
                Tất cả sản phẩm
              </Link>
            </li>
            {categories.map((c) => (
              <li key={c.slug}>
                <Link href={`/danh-muc/${c.slug}`} className="block rounded-md px-3 py-2 hover:bg-accent">
                  {c.name}
                </Link>
              </li>
            ))}
            <li>
              <Link href="/tra-cuu-don" className="block rounded-md px-3 py-2 hover:bg-accent">
                Tra cứu đơn hàng
              </Link>
            </li>
          </ul>
        </div>
      ) : null}
    </header>
  )
}
