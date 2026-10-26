'use client'

import Link from 'next/link'
import { useSearchParams } from 'next/navigation'
import { useMutation } from '@tanstack/react-query'
import { Button, buttonVariants } from '@/components/ui/button'
import { Alert } from '@/components/ui/feedback'
import { AuthCard } from '@/components/storefront/auth-card'
import { confirmEmail } from '@/features/account/api'
import { useAuthStore } from '@/lib/auth/auth.store'
import { getErrorMessage } from '@/lib/errors'

/** Confirmation is an explicit button press (not auto-run on load) so link scanners in mail clients can't consume the token. */
export function ConfirmEmail() {
  const searchParams = useSearchParams()
  const userId = searchParams.get('userId') ?? ''
  const token = searchParams.get('token') ?? ''
  const user = useAuthStore((s) => s.user)
  const mutation = useMutation({
    mutationFn: () => confirmEmail({ userId, token }),
    onSuccess: () => {
      const current = useAuthStore.getState().user
      if (current && current.id === userId)
        useAuthStore.getState().setUser({ ...current, emailConfirmed: true })
    },
  })

  if (!userId || !token) {
    return (
      <AuthCard title="Xác nhận email">
        <Alert variant="error">Liên kết xác nhận không hợp lệ. Vui lòng mở lại liên kết trong email.</Alert>
      </AuthCard>
    )
  }

  return (
    <AuthCard
      title="Xác nhận email"
      description="Bấm nút bên dưới để hoàn tất xác nhận địa chỉ email của bạn."
    >
      {mutation.isSuccess ? (
        <>
          <Alert variant="success" title="Email đã được xác nhận">
            Cảm ơn bạn! Tài khoản đã sẵn sàng.
          </Alert>
          <Link href={user ? '/tai-khoan' : '/dang-nhap'} className={buttonVariants({ size: 'lg' })}>
            {user ? 'Đến tài khoản' : 'Đăng nhập'}
          </Link>
        </>
      ) : (
        <>
          {mutation.isError ? <Alert variant="error">{getErrorMessage(mutation.error)}</Alert> : null}
          <Button size="lg" loading={mutation.isPending} onClick={() => mutation.mutate()}>
            Xác nhận email
          </Button>
        </>
      )}
    </AuthCard>
  )
}
