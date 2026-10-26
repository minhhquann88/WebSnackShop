'use client'

import { useRouter, useSearchParams } from 'next/navigation'
import { useCallback } from 'react'
import type { User } from '@/lib/api/types'
import { isStaff } from '@/lib/auth/auth.store'
import { safeReturnUrl } from './return-url'

/** After sign-in: go to `returnUrl` if safe, otherwise admin home for staff, storefront home for customers. */
export function useAuthRedirect() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const returnUrl = searchParams.get('returnUrl')
  return useCallback(
    (user: User) => {
      router.replace(safeReturnUrl(returnUrl, isStaff(user) ? '/admin' : '/'))
    },
    [router, returnUrl],
  )
}
