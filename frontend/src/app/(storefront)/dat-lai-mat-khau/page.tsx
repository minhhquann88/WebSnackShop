import type { Metadata } from 'next'
import { Suspense } from 'react'
import { Spinner } from '@/components/ui/feedback'
import { ResetPasswordForm } from './reset-password-form'

export const metadata: Metadata = { title: 'Đặt lại mật khẩu', robots: { index: false } }

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <ResetPasswordForm />
    </Suspense>
  )
}
