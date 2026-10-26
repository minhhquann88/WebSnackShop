'use client'

import type { ReactNode } from 'react'
import { Lock } from 'lucide-react'
import { isAdmin, useAuthStore } from '@/lib/auth/auth.store'

/** Renders children only for Admin; Staff sees a notice and no requests are fired. */
export function AdminOnly({ children }: { children: ReactNode }) {
  const user = useAuthStore((s) => s.user)
  if (!isAdmin(user)) {
    return (
      <div className="mx-auto flex max-w-md flex-col items-center gap-3 rounded-xl border bg-background p-10 text-center">
        <Lock className="size-8 text-muted-foreground" aria-hidden />
        <p className="font-medium">Chỉ Admin mới truy cập được trang này</p>
        <p className="text-sm text-muted-foreground">Liên hệ quản trị viên nếu bạn cần quyền này.</p>
      </div>
    )
  }
  return <>{children}</>
}

export function useIsAdmin(): boolean {
  return isAdmin(useAuthStore((s) => s.user))
}
