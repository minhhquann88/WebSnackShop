'use client'

import Link from 'next/link'
import { useParams } from 'next/navigation'
import { useState } from 'react'
import { toast } from 'sonner'
import { ArrowLeft } from 'lucide-react'
import { Button, buttonVariants } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { ErrorState, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Textarea } from '@/components/ui/input'
import { OrderDetailView } from '@/components/storefront/order-detail-view'
import { useCancelMyOrderMutation, useMyOrderQuery } from '@/features/orders/queries'
import { getErrorMessage } from '@/lib/errors'

export default function MyOrderDetailPage() {
  const params = useParams<{ orderNumber: string }>()
  const orderNumber = decodeURIComponent(params.orderNumber)
  const order = useMyOrderQuery(orderNumber)
  const cancel = useCancelMyOrderMutation(orderNumber)
  const [open, setOpen] = useState(false)
  const [reason, setReason] = useState('')

  async function onCancel() {
    try {
      await cancel.mutateAsync(reason.trim() || null)
      toast.success('Đã hủy đơn hàng')
      setOpen(false)
    } catch (e) {
      toast.error(getErrorMessage(e))
      void order.refetch()
    }
  }

  return (
    <div className="grid gap-4">
      <Link
        href="/tai-khoan/don-hang"
        className={buttonVariants({ variant: 'ghost', size: 'sm', className: 'w-fit' })}
      >
        <ArrowLeft aria-hidden /> Tất cả đơn hàng
      </Link>
      {order.isPending ? (
        <Spinner />
      ) : order.isError ? (
        <ErrorState message={getErrorMessage(order.error)} onRetry={() => order.refetch()} />
      ) : (
        <OrderDetailView
          order={order.data}
          actions={
            order.data.canCancel ? (
              <Button variant="outline" size="sm" className="text-destructive" onClick={() => setOpen(true)}>
                Hủy đơn
              </Button>
            ) : null
          }
        />
      )}
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Hủy đơn {orderNumber}?</DialogTitle>
            <DialogDescription>Đơn hàng đã hủy không thể khôi phục.</DialogDescription>
          </DialogHeader>
          <FormField id="cancel-reason" label="Lý do hủy (không bắt buộc)">
            <Textarea rows={3} value={reason} maxLength={500} onChange={(e) => setReason(e.target.value)} />
          </FormField>
          <DialogFooter>
            <Button variant="outline" onClick={() => setOpen(false)}>
              Giữ đơn
            </Button>
            <Button variant="destructive" loading={cancel.isPending} onClick={onCancel}>
              Xác nhận hủy
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
