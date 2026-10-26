'use client'

import { useState } from 'react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Alert } from '@/components/ui/feedback'
import { Checkbox, Input, Textarea } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useOrderActionMutation, type OrderAction } from '@/features/admin/orders/queries'
import type { Schemas } from '@/lib/api/types'
import { getErrorMessage, isApiError } from '@/lib/errors'
import { formatVnd } from '@/lib/format'

type AdminOrderDetail = Schemas['AdminOrderDetailResponse']
type DialogKind = 'confirm' | 'ship' | 'cancel' | 'markPaid' | 'markRefunded' | 'prepare' | 'deliver' | null

/** Current local time as a `datetime-local` value. */
function nowLocalInput(): string {
  const d = new Date()
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset())
  return d.toISOString().slice(0, 16)
}

/**
 * Action buttons rendered strictly from the API's `actions` flags — no status rules are
 * re-derived here. Each action opens a dialog collecting its inputs.
 */
export function OrderActions({ order }: { order: AdminOrderDetail }) {
  const { actions } = order
  const mutation = useOrderActionMutation(order.id)
  const [dialog, setDialog] = useState<DialogKind>(null)
  const [markPaid, setMarkPaid] = useState(false)
  const [transactionRef, setTransactionRef] = useState('')
  const [note, setNote] = useState('')
  const [reason, setReason] = useState('')
  const [paidAt, setPaidAt] = useState(nowLocalInput)
  const [error, setError] = useState<string | null>(null)

  const needsPaymentHint = order.payment.method === 'BankTransfer' && order.payment.status !== 'Paid'

  const open = (kind: DialogKind) => {
    setError(null)
    setMarkPaid(false)
    setTransactionRef(kind === 'markRefunded' ? '' : (order.payment.transactionRef ?? ''))
    setNote('')
    setReason('')
    setPaidAt(nowLocalInput())
    setDialog(kind)
  }

  const run = (action: OrderAction, success: string) =>
    mutation.mutate(action, {
      onSuccess: () => {
        toast.success(success)
        setDialog(null)
      },
      onError: (e) => {
        if (isApiError(e, 'Orders.PaymentRequired')) setMarkPaid(true)
        setError(getErrorMessage(e))
      },
    })

  const anyAction =
    actions.canConfirm ||
    actions.canStartPreparing ||
    actions.canShip ||
    actions.canDeliver ||
    actions.canCancel ||
    actions.canMarkPaid ||
    actions.canMarkRefunded

  return (
    <>
      {anyAction ? (
        <div className="grid gap-2">
          {actions.canConfirm ? <Button onClick={() => open('confirm')}>Xác nhận đơn</Button> : null}
          {actions.canStartPreparing ? (
            <Button onClick={() => open('prepare')}>Bắt đầu chuẩn bị</Button>
          ) : null}
          {actions.canShip ? <Button onClick={() => open('ship')}>Giao cho vận chuyển</Button> : null}
          {actions.canDeliver ? <Button onClick={() => open('deliver')}>Đã giao thành công</Button> : null}
          {actions.canMarkPaid ? (
            <Button variant="outline" onClick={() => open('markPaid')}>
              Ghi nhận đã thanh toán
            </Button>
          ) : null}
          {actions.canMarkRefunded ? (
            <Button variant="outline" onClick={() => open('markRefunded')}>
              Đã hoàn tiền cho khách
            </Button>
          ) : null}
          {actions.canCancel ? (
            <Button variant="destructive" onClick={() => open('cancel')}>
              Hủy đơn
            </Button>
          ) : null}
        </div>
      ) : (
        <p className="text-sm text-muted-foreground">Không còn thao tác nào cho đơn này.</p>
      )}

      <Dialog open={dialog !== null} onOpenChange={(o) => (o ? undefined : setDialog(null))}>
        <DialogContent>
          {dialog === 'confirm' ? (
            <>
              <DialogHeader>
                <DialogTitle>Xác nhận đơn {order.orderNumber}</DialogTitle>
                <DialogDescription>Xác nhận sẽ trừ kho chính thức cho đơn này.</DialogDescription>
              </DialogHeader>
              {needsPaymentHint ? (
                <Alert>
                  Đơn chuyển khoản {formatVnd(order.total)} chưa ghi nhận thanh toán. Nội dung CK:{' '}
                  <span className="font-mono">{order.payment.transferContent ?? '—'}</span>
                </Alert>
              ) : null}
              {needsPaymentHint || markPaid ? (
                <div className="grid gap-3">
                  <Label htmlFor="confirm-paid" className="font-normal">
                    <Checkbox
                      id="confirm-paid"
                      checked={markPaid}
                      onChange={(e) => setMarkPaid(e.target.checked)}
                    />
                    Đã nhận tiền chuyển khoản
                  </Label>
                  {markPaid ? (
                    <div className="grid gap-1.5">
                      <Label htmlFor="confirm-ref">Mã giao dịch (không bắt buộc)</Label>
                      <Input
                        id="confirm-ref"
                        value={transactionRef}
                        onChange={(e) => setTransactionRef(e.target.value)}
                      />
                    </div>
                  ) : null}
                </div>
              ) : null}
            </>
          ) : null}

          {dialog === 'prepare' ? (
            <DialogHeader>
              <DialogTitle>Bắt đầu chuẩn bị đơn {order.orderNumber}?</DialogTitle>
              <DialogDescription>Đơn sẽ chuyển sang trạng thái “Đang chuẩn bị”.</DialogDescription>
            </DialogHeader>
          ) : null}

          {dialog === 'deliver' ? (
            <DialogHeader>
              <DialogTitle>Xác nhận đã giao đơn {order.orderNumber}?</DialogTitle>
              <DialogDescription>
                Đây là trạng thái cuối, không thể đổi lại.
                {order.payment.method === 'Cod' ? ' Đơn COD sẽ được ghi nhận đã thanh toán.' : ''}
              </DialogDescription>
            </DialogHeader>
          ) : null}

          {dialog === 'ship' ? (
            <>
              <DialogHeader>
                <DialogTitle>Giao đơn {order.orderNumber} cho vận chuyển</DialogTitle>
              </DialogHeader>
              <div className="grid gap-1.5">
                <Label htmlFor="ship-note">Ghi chú (mã vận đơn, đơn vị vận chuyển...)</Label>
                <Textarea
                  id="ship-note"
                  value={note}
                  maxLength={500}
                  onChange={(e) => setNote(e.target.value)}
                />
              </div>
            </>
          ) : null}

          {dialog === 'cancel' ? (
            <>
              <DialogHeader>
                <DialogTitle>Hủy đơn {order.orderNumber}</DialogTitle>
                <DialogDescription>
                  Hàng giữ cho đơn sẽ được trả lại kho (trừ đơn đang giao).
                </DialogDescription>
              </DialogHeader>
              <div className="grid gap-1.5">
                <Label htmlFor="cancel-reason">
                  Lý do hủy <span className="text-destructive">*</span>
                </Label>
                <Textarea
                  id="cancel-reason"
                  value={reason}
                  maxLength={500}
                  required
                  onChange={(e) => setReason(e.target.value)}
                />
              </div>
            </>
          ) : null}

          {dialog === 'markPaid' ? (
            <>
              <DialogHeader>
                <DialogTitle>Ghi nhận thanh toán</DialogTitle>
                <DialogDescription>Số tiền: {formatVnd(order.payment.amount)}</DialogDescription>
              </DialogHeader>
              <div className="grid gap-3">
                <div className="grid gap-1.5">
                  <Label htmlFor="paid-ref">Mã giao dịch</Label>
                  <Input
                    id="paid-ref"
                    value={transactionRef}
                    onChange={(e) => setTransactionRef(e.target.value)}
                  />
                </div>
                <div className="grid gap-1.5">
                  <Label htmlFor="paid-at">Thời điểm nhận tiền</Label>
                  <Input
                    id="paid-at"
                    type="datetime-local"
                    value={paidAt}
                    onChange={(e) => setPaidAt(e.target.value)}
                  />
                </div>
              </div>
            </>
          ) : null}

          {dialog === 'markRefunded' ? (
            <>
              <DialogHeader>
                <DialogTitle>Xác nhận đã hoàn tiền</DialogTitle>
                <DialogDescription>
                  Đơn đã hủy nhưng khách đã thanh toán {formatVnd(order.payment.amount)}. Chỉ xác nhận sau khi
                  đã chuyển trả tiền.
                </DialogDescription>
              </DialogHeader>
              <div className="grid gap-1.5">
                <Label htmlFor="refund-ref">Mã giao dịch hoàn tiền (không bắt buộc)</Label>
                <Input
                  id="refund-ref"
                  value={transactionRef}
                  onChange={(e) => setTransactionRef(e.target.value)}
                />
              </div>
            </>
          ) : null}

          {error ? <Alert variant="error">{error}</Alert> : null}

          <DialogFooter>
            <Button variant="outline" onClick={() => setDialog(null)} disabled={mutation.isPending}>
              Đóng
            </Button>
            <Button
              variant={dialog === 'cancel' ? 'destructive' : 'default'}
              loading={mutation.isPending}
              disabled={dialog === 'cancel' && reason.trim().length === 0}
              onClick={() => {
                const ref = transactionRef.trim() || null
                switch (dialog) {
                  case 'confirm':
                    return run(
                      { type: 'confirm', markPaid, transactionRef: markPaid ? ref : null },
                      'Đã xác nhận đơn',
                    )
                  case 'prepare':
                    return run({ type: 'prepare' }, 'Đơn đang được chuẩn bị')
                  case 'ship':
                    return run({ type: 'ship', note: note.trim() || null }, 'Đã chuyển sang đang giao')
                  case 'deliver':
                    return run({ type: 'deliver' }, 'Đơn đã giao thành công')
                  case 'cancel':
                    return run({ type: 'cancel', reason: reason.trim() }, 'Đã hủy đơn')
                  case 'markPaid':
                    return run(
                      {
                        type: 'markPaid',
                        transactionRef: ref,
                        paidAt: paidAt ? new Date(paidAt).toISOString() : null,
                      },
                      'Đã ghi nhận thanh toán',
                    )
                  case 'markRefunded':
                    return run({ type: 'markRefunded', transactionRef: ref }, 'Đã ghi nhận hoàn tiền')
                  default:
                    return undefined
                }
              }}
            >
              {dialog === 'cancel' ? 'Hủy đơn' : 'Xác nhận'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}
