'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
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
import { FormField } from '@/components/ui/form-field'
import { Input, Textarea } from '@/components/ui/input'
import { useStockMutations } from '@/features/admin/inventory/queries'
import type { Schemas } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { formatNumber } from '@/lib/format'

export type StockDialogKind = 'receive' | 'adjust' | 'threshold'
type Stock = Schemas['StockLevelResponse']

const intString = (min: number, max: number, message: string) =>
  z
    .string()
    .trim()
    .refine((v) => /^-?\d+$/.test(v) && Number(v) >= min && Number(v) <= max, message)

const schemas = {
  receive: z.object({
    amount: intString(1, 1_000_000, 'Số lượng nhập là số nguyên từ 1.'),
    text: z.string().trim().max(500, 'Nội dung quá dài.'),
  }),
  adjust: z.object({
    amount: intString(-1_000_000, 1_000_000, 'Nhập số nguyên khác 0 (âm để giảm).').refine(
      (v) => Number(v) !== 0,
      'Số điều chỉnh phải khác 0.',
    ),
    text: z.string().trim().min(1, 'Vui lòng nhập lý do.').max(500, 'Nội dung quá dài.'),
  }),
  threshold: z.object({
    amount: intString(0, 1_000_000, 'Ngưỡng là số nguyên ≥ 0.'),
    text: z.string(),
  }),
}
type Values = { amount: string; text: string }

const titles: Record<StockDialogKind, string> = {
  receive: 'Nhập kho',
  adjust: 'Điều chỉnh tồn kho',
  threshold: 'Ngưỡng cảnh báo sắp hết',
}

/** Receive / adjust / threshold for one variant. Mount with a `key` to reset between rows. */
export function StockDialog({
  kind,
  stock,
  onOpenChange,
}: {
  kind: StockDialogKind | null
  stock: Stock | null
  onOpenChange: (open: boolean) => void
}) {
  const mutations = useStockMutations()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<Values>({
    resolver: zodResolver(schemas[kind ?? 'receive']),
    defaultValues: { amount: kind === 'threshold' && stock ? String(stock.lowStockThreshold) : '', text: '' },
  })
  if (!kind || !stock) return null

  const submit = form.handleSubmit(async ({ amount, text }) => {
    setError(null)
    const n = Number(amount)
    try {
      if (kind === 'receive')
        await mutations.receive.mutateAsync({ variantId: stock.variantId, quantity: n, note: text || null })
      else if (kind === 'adjust')
        await mutations.adjust.mutateAsync({ variantId: stock.variantId, delta: n, reason: text })
      else await mutations.threshold.mutateAsync({ variantId: stock.variantId, threshold: n })
      toast.success('Đã cập nhật kho')
      onOpenChange(false)
    } catch (e) {
      const key = kind === 'receive' ? 'quantity' : kind === 'adjust' ? 'delta' : 'threshold'
      const textKey = kind === 'receive' ? 'note' : 'reason'
      const rest = applyFieldErrors(e, form.setError, ['amount', 'text'], {
        [key]: 'amount',
        [textKey]: 'text',
      })
      setError([getErrorMessage(e), ...rest].join(' '))
    }
  })

  const err = form.formState.errors
  return (
    <Dialog open onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={submit} noValidate className="grid gap-4">
          <DialogHeader>
            <DialogTitle>{titles[kind]}</DialogTitle>
            <DialogDescription>
              {stock.productName} · {stock.variantName} ({stock.sku}). Tồn:{' '}
              {formatNumber(stock.quantityOnHand)}, đang giữ: {formatNumber(stock.quantityReserved)}, có thể
              bán: {formatNumber(stock.available)}.
            </DialogDescription>
          </DialogHeader>
          <FormField
            id="stock-amount"
            label={
              kind === 'receive' ? 'Số lượng nhập' : kind === 'adjust' ? 'Số điều chỉnh (+/-)' : 'Ngưỡng'
            }
            required
            error={err.amount?.message}
            hint={kind === 'adjust' ? 'Ví dụ: -3 khi hàng hỏng, 5 khi kiểm kê thừa.' : undefined}
          >
            <Input inputMode="numeric" autoFocus {...form.register('amount')} />
          </FormField>
          {kind !== 'threshold' ? (
            <FormField
              id="stock-text"
              label={kind === 'receive' ? 'Ghi chú (nhà cung cấp, số phiếu...)' : 'Lý do'}
              required={kind === 'adjust'}
              error={err.text?.message}
            >
              <Textarea {...form.register('text')} />
            </FormField>
          ) : null}
          {error ? <Alert variant="error">{error}</Alert> : null}
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Đóng
            </Button>
            <Button type="submit" loading={form.formState.isSubmitting}>
              Lưu
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
