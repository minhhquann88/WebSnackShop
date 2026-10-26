'use client'

import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { useAuthStore } from '@/lib/auth/auth.store'
import { refreshSession } from '@/lib/auth/session'
import { mergeGuestCart } from '@/features/cart/api'

/**
 * Restores the session on page load (BFF refresh → access token in memory) and, whenever the
 * user becomes authenticated, merges the guest cart into the server cart. When the user signs
 * out, user-scoped cached queries are dropped.
 */
export function SessionBootstrap() {
  const queryClient = useQueryClient()
  const status = useAuthStore((s) => s.status)
  const userId = useAuthStore((s) => s.user?.id)

  useEffect(() => {
    void refreshSession()
  }, [])

  useEffect(() => {
    if (status === 'authenticated' && userId) {
      mergeGuestCart(queryClient).catch(() => {
        toast.error('Không gộp được giỏ hàng tạm. Vui lòng tải lại trang.')
      })
    }
    if (status === 'anonymous') {
      queryClient.removeQueries({ queryKey: ['me'] })
      queryClient.removeQueries({ queryKey: ['admin'] })
    }
  }, [status, userId, queryClient])

  return null
}
