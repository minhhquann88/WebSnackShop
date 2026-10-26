import type { Metadata } from 'next'
import { Suspense } from 'react'
import { Spinner } from '@/components/ui/feedback'
import { TrackOrder } from './track-order'

export const metadata: Metadata = { title: 'Tra cứu đơn hàng' }

export default function TrackOrderPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <TrackOrder />
    </Suspense>
  )
}
