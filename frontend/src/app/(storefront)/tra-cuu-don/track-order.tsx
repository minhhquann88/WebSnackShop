'use client'

import { useSearchParams } from 'next/navigation'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'
import { OrderDetailView } from '@/components/storefront/order-detail-view'
import { useTrackOrderQuery } from '@/features/orders/queries'
import { trackSchema, type TrackFormValues } from '@/features/orders/schemas'
import { getErrorMessage, isApiError } from '@/lib/errors'

export function TrackOrder() {
  const searchParams = useSearchParams()
  const [query, setQuery] = useState({ orderNumber: '', phone: '' })
  const form = useForm<TrackFormValues>({
    resolver: zodResolver(trackSchema),
    defaultValues: { orderNumber: searchParams.get('orderNumber') ?? '', phone: '' },
  })
  const result = useTrackOrderQuery(query.orderNumber, query.phone)

  const onSubmit = form.handleSubmit((values) =>
    setQuery({ orderNumber: values.orderNumber.toUpperCase(), phone: values.phone }),
  )

  return (
    <div className="grid gap-6">
      <Card className="mx-auto w-full max-w-xl">
        <CardHeader>
          <CardTitle>Tra cứu đơn hàng</CardTitle>
          <CardDescription>
            Nhập mã đơn (ví dụ SS-20261026-00001) và số điện thoại người nhận.
          </CardDescription>
        </CardHeader>
        <form onSubmit={onSubmit} noValidate className="grid gap-4 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
          <FormField id="orderNumber" label="Mã đơn hàng" error={form.formState.errors.orderNumber?.message}>
            <Input autoComplete="off" {...form.register('orderNumber')} />
          </FormField>
          <FormField id="phone" label="Số điện thoại" error={form.formState.errors.phone?.message}>
            <Input type="tel" inputMode="numeric" autoComplete="tel" {...form.register('phone')} />
          </FormField>
          <Button type="submit" loading={result.isFetching}>
            Tra cứu
          </Button>
        </form>
      </Card>

      {query.orderNumber ? (
        result.isPending ? (
          <Spinner />
        ) : result.isError ? (
          <Alert variant="error" className="mx-auto w-full max-w-xl">
            {isApiError(result.error, 'Orders.NotFound') || isApiError(result.error, 'Http.404')
              ? 'Không tìm thấy đơn hàng khớp với mã đơn và số điện thoại đã nhập.'
              : getErrorMessage(result.error)}
          </Alert>
        ) : (
          <OrderDetailView order={result.data} />
        )
      ) : null}
    </div>
  )
}
