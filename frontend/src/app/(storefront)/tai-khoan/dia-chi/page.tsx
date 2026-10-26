'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { MapPin, Plus } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardTitle } from '@/components/ui/card'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Alert, EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Checkbox, Input } from '@/components/ui/input'
import { AddressFields } from '@/components/storefront/address-fields'
import {
  useAddressesQuery,
  useDeleteAddressMutation,
  useSaveAddressMutation,
  useSetDefaultAddressMutation,
} from '@/features/account/queries'
import { ADDRESS_FIELDS, addressSchema, emptyAddress } from '@/features/orders/schemas'
import type { SavedAddress } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'

const savedAddressSchema = z.object({
  label: z.string().trim().max(50, 'Tên gợi nhớ tối đa 50 ký tự.'),
  address: addressSchema,
  isDefault: z.boolean(),
})
type SavedAddressFormValues = z.infer<typeof savedAddressSchema>

const FORM_FIELDS = ['label', 'isDefault', ...ADDRESS_FIELDS.map((f) => `address.${f}`)]

export default function AddressesPage() {
  const addresses = useAddressesQuery()
  const remove = useDeleteAddressMutation()
  const setDefault = useSetDefaultAddressMutation()
  const [editing, setEditing] = useState<SavedAddress | 'new' | null>(null)

  async function run(action: () => Promise<unknown>, success: string) {
    try {
      await action()
      toast.success(success)
    } catch (e) {
      toast.error(getErrorMessage(e))
    }
  }

  return (
    <Card>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <CardTitle>Sổ địa chỉ</CardTitle>
        <Button size="sm" onClick={() => setEditing('new')}>
          <Plus aria-hidden /> Thêm địa chỉ
        </Button>
      </div>
      {addresses.isPending ? (
        <Spinner />
      ) : addresses.isError ? (
        <ErrorState message={getErrorMessage(addresses.error)} onRetry={() => addresses.refetch()} />
      ) : addresses.data.length === 0 ? (
        <EmptyState
          title="Chưa có địa chỉ nào"
          description="Lưu địa chỉ để thanh toán nhanh hơn ở lần mua sau."
        />
      ) : (
        <ul className="grid gap-3 sm:grid-cols-2">
          {addresses.data.map((a) => (
            <li key={a.id} className="grid gap-2 rounded-lg border p-3 text-sm">
              <div className="flex items-center gap-2">
                <MapPin className="size-4 text-primary" aria-hidden />
                <span className="font-medium">{a.label ?? 'Địa chỉ'}</span>
                {a.isDefault ? <Badge variant="success">Mặc định</Badge> : null}
              </div>
              <address className="grid not-italic">
                <span>
                  {a.fullName} · {a.phone}
                </span>
                <span className="text-muted-foreground">
                  {[a.line1, a.ward, a.district, a.province].join(', ')}
                </span>
              </address>
              <div className="flex flex-wrap gap-1">
                <Button variant="outline" size="sm" onClick={() => setEditing(a)}>
                  Sửa
                </Button>
                {!a.isDefault ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    disabled={setDefault.isPending}
                    onClick={() => run(() => setDefault.mutateAsync(a.id), 'Đã đặt làm địa chỉ mặc định')}
                  >
                    Đặt mặc định
                  </Button>
                ) : null}
                <Button
                  variant="ghost"
                  size="sm"
                  className="text-destructive"
                  disabled={remove.isPending}
                  onClick={() => {
                    if (window.confirm('Xóa địa chỉ này?'))
                      void run(() => remove.mutateAsync(a.id), 'Đã xóa địa chỉ')
                  }}
                >
                  Xóa
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}
      <Dialog open={editing !== null} onOpenChange={(open) => (open ? undefined : setEditing(null))}>
        <DialogContent>
          {editing !== null ? (
            <AddressForm
              key={editing === 'new' ? 'new' : editing.id}
              address={editing === 'new' ? null : editing}
              onDone={() => setEditing(null)}
            />
          ) : null}
        </DialogContent>
      </Dialog>
    </Card>
  )
}

function AddressForm({ address, onDone }: { address: SavedAddress | null; onDone: () => void }) {
  const save = useSaveAddressMutation()
  const [formError, setFormError] = useState<string | null>(null)
  const form = useForm<SavedAddressFormValues>({
    resolver: zodResolver(savedAddressSchema),
    defaultValues: address
      ? {
          label: address.label ?? '',
          isDefault: address.isDefault,
          address: {
            fullName: address.fullName,
            phone: address.phone,
            line1: address.line1,
            ward: address.ward,
            district: address.district,
            province: address.province,
          },
        }
      : { label: '', isDefault: false, address: emptyAddress },
  })

  const onSubmit = form.handleSubmit(async (values) => {
    setFormError(null)
    try {
      await save.mutateAsync({
        id: address?.id ?? null,
        body: { label: values.label || null, address: values.address, isDefault: values.isDefault },
      })
      toast.success(address ? 'Đã cập nhật địa chỉ' : 'Đã thêm địa chỉ')
      onDone()
    } catch (e) {
      const unmatched = applyFieldErrors(e, form.setError, FORM_FIELDS)
      setFormError(unmatched.length > 0 ? unmatched.join(' ') : getErrorMessage(e))
    }
  })

  return (
    <form onSubmit={onSubmit} noValidate className="grid gap-4">
      <DialogHeader>
        <DialogTitle>{address ? 'Sửa địa chỉ' : 'Thêm địa chỉ'}</DialogTitle>
        <DialogDescription>Địa chỉ dùng để giao hàng tại Việt Nam.</DialogDescription>
      </DialogHeader>
      {formError ? <Alert variant="error">{formError}</Alert> : null}
      <FormField
        id="addr-label"
        label="Tên gợi nhớ"
        hint="Ví dụ: Nhà riêng, Công ty"
        error={form.formState.errors.label?.message}
      >
        <Input {...form.register('label')} />
      </FormField>
      <AddressFields
        prefix="address"
        register={form.register}
        errors={form.formState.errors}
        idPrefix="addr"
      />
      <label className="flex items-center gap-2 text-sm">
        <Checkbox {...form.register('isDefault')} />
        Đặt làm địa chỉ mặc định
      </label>
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onDone}>
          Hủy
        </Button>
        <Button type="submit" loading={form.formState.isSubmitting}>
          Lưu địa chỉ
        </Button>
      </DialogFooter>
    </form>
  )
}
