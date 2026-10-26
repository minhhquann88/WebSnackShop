'use client'

import { useState, type ReactNode } from 'react'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import {
  BarChart3,
  Boxes,
  FolderTree,
  LayoutDashboard,
  LogOut,
  Menu,
  Package,
  Settings,
  ShieldAlert,
  ShoppingBag,
  UserCog,
  Users,
  X,
  type LucideIcon,
} from 'lucide-react'
import { Button, buttonVariants } from '@/components/ui/button'
import { RequireAuth } from '@/features/auth/require-auth'
import { isAdmin, isStaff, useAuthStore } from '@/lib/auth/auth.store'
import { logout } from '@/lib/auth/session'
import { roleLabels } from '@/lib/labels'
import { cn } from '@/lib/utils'

interface NavItem {
  href: string
  label: string
  icon: LucideIcon
  adminOnly?: boolean
}

const NAV: NavItem[] = [
  { href: '/admin', label: 'Tổng quan', icon: LayoutDashboard },
  { href: '/admin/don-hang', label: 'Đơn hàng', icon: ShoppingBag },
  { href: '/admin/san-pham', label: 'Sản phẩm', icon: Package, adminOnly: true },
  { href: '/admin/danh-muc', label: 'Danh mục', icon: FolderTree, adminOnly: true },
  { href: '/admin/kho', label: 'Kho', icon: Boxes },
  { href: '/admin/khach-hang', label: 'Khách hàng', icon: Users },
  { href: '/admin/nhan-vien', label: 'Nhân viên', icon: UserCog, adminOnly: true },
  { href: '/admin/bao-cao', label: 'Báo cáo', icon: BarChart3, adminOnly: true },
  { href: '/admin/cai-dat', label: 'Cài đặt', icon: Settings, adminOnly: true },
]

function isActive(pathname: string, href: string): boolean {
  return href === '/admin' ? pathname === '/admin' : pathname === href || pathname.startsWith(`${href}/`)
}

function Forbidden() {
  return (
    <main className="mx-auto flex min-h-dvh max-w-md flex-col items-center justify-center gap-4 px-4 text-center">
      <ShieldAlert className="size-10 text-destructive" aria-hidden />
      <h1 className="text-xl font-semibold">Không có quyền truy cập</h1>
      <p className="text-muted-foreground">Khu vực quản trị chỉ dành cho nhân viên cửa hàng.</p>
      <Link href="/" className={buttonVariants()}>
        Về trang chủ
      </Link>
    </main>
  )
}

function Shell({ children }: { children: ReactNode }) {
  const user = useAuthStore((s) => s.user)
  const pathname = usePathname()
  const router = useRouter()
  const [open, setOpen] = useState(false)
  const [loggingOut, setLoggingOut] = useState(false)

  if (!isStaff(user)) return <Forbidden />
  const admin = isAdmin(user)
  const items = NAV.filter((item) => admin || !item.adminOnly)

  const onLogout = async () => {
    setLoggingOut(true)
    try {
      await logout()
    } finally {
      router.replace('/dang-nhap')
    }
  }

  const nav = (
    <nav aria-label="Quản trị" className="grid gap-1 p-3">
      {items.map((item) => {
        const active = isActive(pathname, item.href)
        return (
          <Link
            key={item.href}
            href={item.href}
            onClick={() => setOpen(false)}
            aria-current={active ? 'page' : undefined}
            className={cn(
              'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
              active ? 'bg-primary text-primary-foreground' : 'text-foreground/80 hover:bg-accent',
            )}
          >
            <item.icon className="size-4" aria-hidden />
            {item.label}
          </Link>
        )
      })}
    </nav>
  )

  return (
    <div className="flex min-h-dvh bg-muted/30">
      <aside className="sticky top-0 hidden h-dvh w-60 shrink-0 flex-col border-r bg-background lg:flex">
        <Link href="/admin" className="flex h-14 items-center border-b px-5 text-lg font-bold text-primary">
          SnackShop Admin
        </Link>
        {nav}
        <div className="mt-auto border-t p-3">
          <Link
            href="/"
            className={cn(buttonVariants({ variant: 'ghost', size: 'sm' }), 'w-full justify-start')}
          >
            Xem cửa hàng
          </Link>
        </div>
      </aside>

      {open ? (
        <div
          className="fixed inset-0 z-40 lg:hidden"
          role="dialog"
          aria-modal="true"
          aria-label="Menu quản trị"
        >
          <button
            type="button"
            className="absolute inset-0 bg-black/40"
            aria-label="Đóng menu"
            onClick={() => setOpen(false)}
          />
          <aside className="absolute inset-y-0 left-0 flex w-64 flex-col bg-background shadow-lg">
            <div className="flex h-14 items-center justify-between border-b px-4">
              <span className="text-lg font-bold text-primary">SnackShop Admin</span>
              <Button variant="ghost" size="icon-sm" aria-label="Đóng menu" onClick={() => setOpen(false)}>
                <X aria-hidden />
              </Button>
            </div>
            {nav}
          </aside>
        </div>
      ) : null}

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b bg-background px-4">
          <Button
            variant="ghost"
            size="icon-sm"
            className="lg:hidden"
            aria-label="Mở menu"
            onClick={() => setOpen(true)}
          >
            <Menu aria-hidden />
          </Button>
          <div className="ml-auto flex items-center gap-3">
            <div className="hidden text-right sm:block">
              <p className="text-sm leading-tight font-medium">{user?.fullName}</p>
              <p className="text-xs text-muted-foreground">
                {user?.roles
                  .filter((r) => r !== 'Customer')
                  .map((r) => roleLabels[r] ?? r)
                  .join(', ')}
              </p>
            </div>
            <Button variant="outline" size="sm" onClick={onLogout} loading={loggingOut}>
              <LogOut aria-hidden />
              Đăng xuất
            </Button>
          </div>
        </header>
        <main className="flex-1 p-4 sm:p-6">{children}</main>
      </div>
    </div>
  )
}

export function AdminShell({ children }: { children: ReactNode }) {
  return (
    <RequireAuth>
      <Shell>{children}</Shell>
    </RequireAuth>
  )
}
