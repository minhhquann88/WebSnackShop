'use client'

import { useEffect, type ReactNode } from 'react'
import { usePathname, useRouter } from 'next/navigation'
import { Spinner } from '@/components/ui/feedback'
import { useAuthStore } from '@/lib/auth/auth.store'

/** Client guard: waits for session restore, redirects anonymous users to login with `returnUrl`. */
export function RequireAuth({ children }: { children: ReactNode }) {
  const status = useAuthStore((s) => s.status)
  const router = useRouter()
  const pathname = usePathname()

  useEffect(() => {
    if (status === 'anonymous') router.replace(`/dang-nhap?returnUrl=${encodeURIComponent(pathname)}`)
  }, [status, router, pathname])

  if (status !== 'authenticated') return <Spinner label="Đang kiểm tra đăng nhập..." />
  return <>{children}</>
}
