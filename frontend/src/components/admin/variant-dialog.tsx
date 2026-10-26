'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { VariantFields } from '@/components/admin/product-fields'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Alert } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Checkbox, Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  VARIANT_FIELDS,
  emptyVariant,
  fromVariant,
  toVariantRequest,
  variantSchema,
} from '@/features/admin/catalog/schemas'
import type { Schemas } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'

const dialogSchema = z.object({
  variant: variantSchema,
  initialStock: z
    .string()
    .trim()
    .refine(
      (v) => v === '' || (/^\d+$/.test(v) && Number(v) <= 1_000_000),
      'Tồn ban đầu là số nguyên 0 - 1.000.000.',
    ),
  isActive: z.boolean(),
})
type DialogValues = z.infer<typeof dialogSchema>

const KNOWN = [...VARIANT_FIELDS.map((f) => `variant.${f}`), 'initialStock']
const PREFIX_MAP = {
  'variant.price.amount': 'variant.price',
  'variant.compareAtPrice.amount': 'variant.compareAtPrice',
}

export type VariantSubmit =
  | { mode: 'add'; body: Schemas['AddVariantRequest'] }
  | { mode: 'edit'; variantId: string; body: Schemas['UpdateVariantRequest'] }

/** Add/edit a variant. Mount with a `key` so defaults reset per variant. */
export function VariantDialog({
  open,
  onOpenChange,
  variant,
  onSubmit,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  variant: Schemas['AdminVariantResponse'] | null
  onSubmit: (value: VariantSubmit) => Promise<unknown>
}) {
  const [error, setError] = useState<string | null>(null)
  const form = useForm<DialogValues>({
    resolver: zodResolver(dialogSchema),
    defaultValues: {
      variant: variant ? fromVariant(variant) : emptyVariant,
      initialStock: '0',
      isActive: variant?.isActive ?? true,
    },
  })

  const submit = form.handleSubmit(async (values) => {
    setError(null)
    try {
      const v = toVariantRequest(values.variant)
      await onSubmit(
        variant
          ? { mode: 'edit', variantId: variant.id, body: { variant: v, isActive: values.isActive } }
          : {
              mode: 'add',
              body: {
                variant: v,
                initialStock: values.initialStock === '' ? 0 : Number(values.initialStock),
              },
            },
      )
      onOpenChange(false)
    } catch (e) {
      const rest = applyFieldErrors(e, form.setError, KNOWN, PREFIX_MAP)
      setError([getErrorMessage(e), ...rest].join(' '))
    }
  })

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <form onSubmit={submit} noValidate className="grid gap-4">
          <DialogHeader>
            <DialogTitle>{variant ? `Sửa phân loại ${variant.sku}` : 'Thêm phân loại'}</DialogTitle>
          </DialogHeader>
          <VariantFields form={form} prefix="variant" idPrefix="dlg-variant" />
          {variant ? (
            <Label htmlFor="dlg-variant-active" className="font-normal">
              <Checkbox id="dlg-variant-active" {...form.register('isActive')} />
              Đang bán
            </Label>
          ) : (
            <FormField
              id="dlg-variant-stock"
              label="Tồn kho ban đầu"
              error={form.formState.errors.initialStock?.message}
              className="sm:max-w-xs"
            >
              <Input inputMode="numeric" {...form.register('initialStock')} />
            </FormField>
          )}
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
