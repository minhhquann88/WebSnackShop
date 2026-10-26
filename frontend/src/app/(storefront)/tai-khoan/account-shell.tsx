'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import type { ReactNode } from 'react'
import { KeyRound, MapPin, Package, UserRound } from 'lucide-react'
import { Alert } from '@/components/ui/feedback'
import { RequireAuth } from '@/features/auth/require-auth'
import { useAuthStore } from '@/lib/auth/auth.store'
import { cn } from '@/lib/utils'

const NAV = [
  { href: '/tai-khoan', label: 'Hồ sơ', icon: UserRound, exact: true },
  { href: '/tai-khoan/don-hang', label: 'Đơn hàng', icon: Package, exact: false },
  { href: '/tai-khoan/dia-chi', label: 'Sổ địa chỉ', icon: MapPin, exact: false },
  { href: '/tai-khoan/mat-khau', label: 'Đổi mật khẩu', icon: KeyRound, exact: false },
] as const

export function AccountShell({ children }: { children: ReactNode }) {
  const pathname = usePathname()
  const user = useAuthStore((s) => s.user)
  return (
    <RequireAuth>
      <div className="grid gap-6 md:grid-cols-[220px_1fr]">
        <nav aria-label="Tài khoản" className="md:sticky md:top-24 md:h-fit">
          <p className="mb-3 hidden truncate font-semibold md:block">{user?.fullName}</p>
          <ul className="flex gap-1 overflow-x-auto md:flex-col">
            {NAV.map((item) => {
              const active = item.exact ? pathname === item.href : pathname.startsWith(item.href)
              const Icon = item.icon
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    aria-current={active ? 'page' : undefined}
                    className={cn(
                      'flex items-center gap-2 rounded-md px-3 py-2 text-sm whitespace-nowrap hover:bg-accent',
                      active && 'bg-accent font-medium text-primary',
                    )}
                  >
                    <Icon className="size-4" aria-hidden />
                    {item.label}
                  </Link>
                </li>
              )
            })}
          </ul>
        </nav>
        <div className="grid min-w-0 content-start gap-4">
          {user && !user.emailConfirmed ? (
            <Alert>
              Email của bạn chưa được xác nhận. Vui lòng mở liên kết trong email chúng tôi đã gửi.
            </Alert>
          ) : null}
          {children}
        </div>
      </div>
    </RequireAuth>
  )
}
