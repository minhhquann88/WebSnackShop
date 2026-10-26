import type { Metadata } from 'next'
import { Suspense } from 'react'
import { Spinner } from '@/components/ui/feedback'
import { OrderSuccess } from './order-success'

export const metadata: Metadata = { title: 'Đặt hàng thành công', robots: { index: false } }

export default function OrderSuccessPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <OrderSuccess />
    </Suspense>
  )
}
