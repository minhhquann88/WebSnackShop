import type { Metadata } from 'next'
import { Suspense } from 'react'
import { Spinner } from '@/components/ui/feedback'
import { LoginForm } from './login-form'

export const metadata: Metadata = { title: 'Đăng nhập' }

export default function LoginPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <LoginForm />
    </Suspense>
  )
}
