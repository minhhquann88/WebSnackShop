'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { AdminOnly } from '@/components/admin/admin-only'
import { PageHeader } from '@/components/admin/page-header'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, ErrorState, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input, Textarea } from '@/components/ui/input'
import { useAdminSettingsQuery, useUpdateSettingsMutation } from '@/features/admin/reports/queries'
import type { Schemas } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { formatDateTime } from '@/lib/format'

const money = z
  .string()
  .trim()
  .refine((v) => /^\d+$/.test(v), 'Nhập số tiền (VND), không dấu chấm.')

const base = z.object({
  shopName: z.string().trim().min(1, 'Vui lòng nhập tên cửa hàng.').max(200, 'Quá dài.'),
  shopPhone: z
    .string()
    .trim()
    .regex(/^0\d{9,10}$/, 'Số điện thoại bắt đầu bằng 0, gồm 10-11 chữ số.'),
  shopAddress: z.string().trim().min(1, 'Vui lòng nhập địa chỉ.').max(500, 'Quá dài.'),
  flatShippingFee: money,
  freeShippingThreshold: z
    .string()
    .trim()
    .refine((v) => v === '' || (/^\d+$/.test(v) && Number(v) > 0), 'Để trống hoặc nhập số tiền > 0.'),
  bankName: z.string().trim().min(1, 'Vui lòng nhập tên ngân hàng.').max(200, 'Quá dài.'),
  bankAccountNumber: z
    .string()
    .trim()
    .regex(/^\d{6,20}$/, 'Số tài khoản gồm 6-20 chữ số.'),
  bankAccountName: z.string().trim().min(1, 'Vui lòng nhập tên chủ tài khoản.').max(200, 'Quá dài.'),
  bankBin: z
    .string()
    .trim()
    .regex(/^\d{6}$/, 'Mã BIN ngân hàng gồm đúng 6 chữ số (VietQR).'),
  unpaidBankTransferExpiryHours: z
    .string()
    .trim()
    .refine((v) => /^\d+$/.test(v) && Number(v) >= 1 && Number(v) <= 168, 'Từ 1 đến 168 giờ.'),
})
const schema = base.refine(
  (v) => v.freeShippingThreshold === '' || Number(v.freeShippingThreshold) > Number(v.flatShippingFee),
  {
    path: ['freeShippingThreshold'],
    message: 'Ngưỡng miễn phí vận chuyển phải lớn hơn phí vận chuyển.',
  },
)
type Values = z.infer<typeof schema>
const FIELDS = Object.keys(base.shape)

function toValues(s: Schemas['AdminSettingsResponse']): Values {
  return {
    shopName: s.shopName,
    shopPhone: s.shopPhone,
    shopAddress: s.shopAddress,
    flatShippingFee: String(Math.round(s.flatShippingFee.amount)),
    freeShippingThreshold: s.freeShippingThreshold ? String(Math.round(s.freeShippingThreshold.amount)) : '',
    bankName: s.bankName,
    bankAccountNumber: s.bankAccountNumber,
    bankAccountName: s.bankAccountName,
    bankBin: s.bankBin,
    unpaidBankTransferExpiryHours: String(s.unpaidBankTransferExpiryHours),
  }
}

function SettingsForm({ settings }: { settings: Schemas['AdminSettingsResponse'] }) {
  const update = useUpdateSettingsMutation()
  const [formError, setFormError] = useState<string | null>(null)
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: toValues(settings) })
  const err = form.formState.errors

  const onSubmit = form.handleSubmit((v) => {
    setFormError(null)
    update.mutate(
      {
        shopName: v.shopName,
        shopPhone: v.shopPhone,
        shopAddress: v.shopAddress,
        flatShippingFee: { amount: Number(v.flatShippingFee), currency: 'VND' },
        freeShippingThreshold:
          v.freeShippingThreshold === ''
            ? null
            : { amount: Number(v.freeShippingThreshold), currency: 'VND' },
        bankName: v.bankName,
        bankAccountNumber: v.bankAccountNumber,
        bankAccountName: v.bankAccountName,
        bankBin: v.bankBin,
        unpaidBankTransferExpiryHours: Number(v.unpaidBankTransferExpiryHours),
      },
      {
        onSuccess: () => {
          toast.success('Đã lưu cài đặt')
          form.reset(v)
        },
        onError: (e) => {
          const rest = applyFieldErrors(e, form.setError, FIELDS, {
            'flatShippingFee.amount': 'flatShippingFee',
            'freeShippingThreshold.amount': 'freeShippingThreshold',
          })
          setFormError([getErrorMessage(e), ...rest].join(' '))
        },
      },
    )
  })

  return (
    <form onSubmit={onSubmit} noValidate className="grid max-w-3xl gap-6">
      <Card>
        <CardTitle>Cửa hàng</CardTitle>
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField id="set-name" label="Tên cửa hàng" required error={err.shopName?.message}>
            <Input {...form.register('shopName')} />
          </FormField>
          <FormField id="set-phone" label="Số điện thoại" required error={err.shopPhone?.message}>
            <Input type="tel" {...form.register('shopPhone')} />
          </FormField>
          <FormField
            id="set-address"
            label="Địa chỉ"
            required
            error={err.shopAddress?.message}
            className="sm:col-span-2"
          >
            <Textarea rows={2} {...form.register('shopAddress')} />
          </FormField>
        </div>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Vận chuyển</CardTitle>
          <CardDescription>Phí đồng giá toàn quốc; miễn phí khi tạm tính đạt ngưỡng.</CardDescription>
        </CardHeader>
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField id="set-fee" label="Phí vận chuyển (₫)" required error={err.flatShippingFee?.message}>
            <Input inputMode="numeric" {...form.register('flatShippingFee')} />
          </FormField>
          <FormField
            id="set-free"
            label="Ngưỡng miễn phí vận chuyển (₫)"
            error={err.freeShippingThreshold?.message}
            hint="Để trống nếu không áp dụng."
          >
            <Input inputMode="numeric" {...form.register('freeShippingThreshold')} />
          </FormField>
        </div>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Chuyển khoản ngân hàng</CardTitle>
          <CardDescription>Dùng để tạo mã VietQR và hướng dẫn chuyển khoản cho khách.</CardDescription>
        </CardHeader>
        <div className="grid gap-4 sm:grid-cols-2">
          <FormField id="set-bank" label="Ngân hàng" required error={err.bankName?.message}>
            <Input {...form.register('bankName')} />
          </FormField>
          <FormField
            id="set-bin"
            label="Mã BIN ngân hàng"
            required
            error={err.bankBin?.message}
            hint="6 chữ số, ví dụ 970436 (Vietcombank)."
          >
            <Input inputMode="numeric" {...form.register('bankBin')} />
          </FormField>
          <FormField id="set-account" label="Số tài khoản" required error={err.bankAccountNumber?.message}>
            <Input inputMode="numeric" {...form.register('bankAccountNumber')} />
          </FormField>
          <FormField
            id="set-account-name"
            label="Chủ tài khoản"
            required
            error={err.bankAccountName?.message}
          >
            <Input {...form.register('bankAccountName')} />
          </FormField>
          <FormField
            id="set-expiry"
            label="Tự hủy đơn chưa chuyển khoản sau (giờ)"
            required
            error={err.unpaidBankTransferExpiryHours?.message}
          >
            <Input inputMode="numeric" {...form.register('unpaidBankTransferExpiryHours')} />
          </FormField>
        </div>
      </Card>
      {formError ? <Alert variant="error">{formError}</Alert> : null}
      <div className="flex flex-wrap items-center gap-3">
        <Button type="submit" loading={update.isPending} disabled={!form.formState.isDirty}>
          Lưu cài đặt
        </Button>
        <span className="text-sm text-muted-foreground">
          Cập nhật lần cuối: {formatDateTime(settings.updatedAt)}
        </span>
      </div>
    </form>
  )
}

function Settings() {
  const query = useAdminSettingsQuery()
  if (query.isPending) return <Spinner />
  if (query.isError)
    return <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
  return <SettingsForm settings={query.data} />
}

export default function AdminSettingsPage() {
  return (
    <AdminOnly>
      <PageHeader title="Cài đặt cửa hàng" />
      <Settings />
    </AdminOnly>
  )
}
