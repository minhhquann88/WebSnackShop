import type { Metadata } from 'next'
import { Suspense } from 'react'
import { Spinner } from '@/components/ui/feedback'
import { ConfirmEmail } from './confirm-email'

export const metadata: Metadata = { title: 'Xác nhận email', robots: { index: false } }

export default function ConfirmEmailPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <ConfirmEmail />
    </Suspense>
  )
}
