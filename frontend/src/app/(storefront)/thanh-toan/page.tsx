'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useEffect, useRef, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { Button, buttonVariants } from '@/components/ui/button'
import { Card, CardTitle } from '@/components/ui/card'
import { Alert, EmptyState, ErrorState, Skeleton, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input, NativeSelect, Textarea } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { RemoteImage } from '@/components/ui/remote-image'
import { AddressFields } from '@/components/storefront/address-fields'
import { EstimateSummary } from '@/components/storefront/order-summary'
import { useAddressesQuery } from '@/features/account/queries'
import { cartKeys } from '@/features/cart/api'
import { useCart, useEstimate } from '@/features/cart/use-cart'
import { saveLastOrder } from '@/features/orders/last-order'
import { usePlaceOrderMutation } from '@/features/orders/queries'
import {
  CHECKOUT_FIELDS,
  checkoutSchema,
  emptyAddress,
  type AddressFormValues,
  type CheckoutFormValues,
} from '@/features/orders/schemas'
import { useAuthStore } from '@/lib/auth/auth.store'
import type { SavedAddress } from '@/lib/api/types'
import { ApiError, applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { formatVnd } from '@/lib/format'
import { paymentMethodLabels } from '@/lib/labels'
import { newUuid } from '@/lib/uuid'
import { useGuestCartStore } from '@/stores/cart.store'

function toAddressValues(a: SavedAddress): AddressFormValues {
  return {
    fullName: a.fullName,
    phone: a.phone,
    line1: a.line1,
    ward: a.ward,
    district: a.district,
    province: a.province,
  }
}

export default function CheckoutPage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const cart = useCart()
  const user = useAuthStore((s) => s.user)
  const isUser = cart.mode === 'user'
  const estimate = useEstimate(cart.items, cart.mode !== 'loading')
  const addresses = useAddressesQuery(isUser)
  const placeOrder = usePlaceOrderMutation()
  const [formError, setFormError] = useState<string | null>(null)
  const [placed, setPlaced] = useState(false)

  // One key per checkout attempt: retries after network errors reuse it (no duplicate orders);
  // a definitive 4xx rejection rotates it so the corrected submission is a new attempt.
  const [idempotencyKey, setIdempotencyKey] = useState(newUuid)

  const form = useForm<CheckoutFormValues>({
    resolver: zodResolver(checkoutSchema),
    defaultValues: {
      contactEmail: '',
      shippingAddress: emptyAddress,
      paymentMethod: 'Cod',
      customerNote: '',
    },
  })
  const { register, handleSubmit, formState, setValue, getValues, setError, control } = form
  const paymentMethod = useWatch({ control, name: 'paymentMethod' })

  // Prefill for signed-in customers (email from the session, default saved address). Runs once per data arrival.
  const prefilled = useRef(false)
  useEffect(() => {
    if (prefilled.current || !isUser || !user || !addresses.data) return
    prefilled.current = true
    if (!getValues('contactEmail')) setValue('contactEmail', user.email)
    const def = addresses.data.find((a) => a.isDefault) ?? addresses.data[0]
    if (def) setValue('shippingAddress', toAddressValues(def))
    else if (user.fullName) {
      setValue('shippingAddress.fullName', user.fullName)
      if (user.phoneNumber) setValue('shippingAddress.phone', user.phoneNumber)
    }
  }, [isUser, user, addresses.data, getValues, setValue])

  if (cart.isLoading || placed)
    return <Spinner label={placed ? 'Đang chuyển đến trang xác nhận...' : 'Đang tải giỏ hàng...'} />
  if (cart.items.length === 0) {
    return (
      <EmptyState
        title="Giỏ hàng đang trống"
        description="Hãy thêm sản phẩm trước khi thanh toán."
        action={
          <Link href="/san-pham" className={buttonVariants()}>
            Xem sản phẩm
          </Link>
        }
      />
    )
  }

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null)
    try {
      const result = await placeOrder.mutateAsync({
        idempotencyKey,
        body: {
          items: cart.items,
          contactEmail: values.contactEmail,
          shippingAddress: values.shippingAddress,
          paymentMethod: values.paymentMethod,
          customerNote: values.customerNote ? values.customerNote : null,
        },
      })
      setPlaced(true)
      saveLastOrder(result)
      if (isUser) {
        await queryClient.invalidateQueries({ queryKey: cartKeys.mine })
        await queryClient.invalidateQueries({ queryKey: ['me', 'orders'] })
      } else {
        useGuestCartStore.getState().clear()
      }
      router.replace(`/dat-hang-thanh-cong?orderNumber=${encodeURIComponent(result.orderNumber)}`)
    } catch (error) {
      if (error instanceof ApiError && error.status >= 400 && error.status < 500) setIdempotencyKey(newUuid())
      const unmatched = applyFieldErrors(error, setError, CHECKOUT_FIELDS)
      if (error instanceof ApiError && error.status === 409) void estimate.refetch()
      const message =
        unmatched.length > 0 && error instanceof ApiError && error.code === 'Validation.Failed'
          ? `${getErrorMessage(error)} ${unmatched.join(' ')}`
          : getErrorMessage(error)
      setFormError(message)
      toast.error(message)
    }
  })

  const blocked = !estimate.data?.canPlaceOrder

  return (
    <div className="grid gap-6">
      <h1 className="text-2xl font-bold">Thanh toán</h1>
      {cart.mode === 'guest' ? (
        <Alert>
          Bạn đang đặt hàng không cần tài khoản.{' '}
          <Link href="/dang-nhap?returnUrl=/thanh-toan" className="font-medium text-primary underline">
            Đăng nhập
          </Link>{' '}
          để dùng địa chỉ đã lưu và theo dõi đơn dễ hơn.
        </Alert>
      ) : null}
      <form onSubmit={onSubmit} noValidate className="grid gap-6 lg:grid-cols-[1fr_360px]">
        <div className="grid content-start gap-6">
          <Card>
            <CardTitle>Thông tin liên hệ</CardTitle>
            <FormField
              id="contactEmail"
              label="Email nhận thông báo đơn hàng"
              error={formState.errors.contactEmail?.message}
              required
            >
              <Input type="email" autoComplete="email" {...register('contactEmail')} />
            </FormField>
          </Card>

          <Card>
            <div className="flex flex-wrap items-center justify-between gap-2">
              <CardTitle>Địa chỉ giao hàng</CardTitle>
              {isUser && addresses.data && addresses.data.length > 0 ? (
                <div className="flex items-center gap-2">
                  <Label htmlFor="saved-address" className="text-muted-foreground">
                    Địa chỉ đã lưu
                  </Label>
                  <NativeSelect
                    id="saved-address"
                    className="w-auto max-w-56"
                    defaultValue=""
                    onChange={(e) => {
                      const a = addresses.data.find((x) => x.id === e.target.value)
                      if (a) setValue('shippingAddress', toAddressValues(a), { shouldValidate: true })
                    }}
                  >
                    <option value="" disabled>
                      Chọn địa chỉ
                    </option>
                    {addresses.data.map((a) => (
                      <option key={a.id} value={a.id}>
                        {a.label ?? a.fullName} - {a.line1}
                      </option>
                    ))}
                  </NativeSelect>
                </div>
              ) : null}
            </div>
            <AddressFields
              prefix="shippingAddress"
              register={register}
              errors={formState.errors}
              idPrefix="ship"
            />
          </Card>

          <Card>
            <CardTitle>Phương thức thanh toán</CardTitle>
            <fieldset className="grid gap-2">
              <legend className="sr-only">Phương thức thanh toán</legend>
              {(['Cod', 'BankTransfer'] as const).map((method) => (
                <label
                  key={method}
                  className="flex cursor-pointer items-start gap-3 rounded-lg border p-3 has-[:checked]:border-primary has-[:checked]:bg-primary/5"
                >
                  <input
                    type="radio"
                    value={method}
                    {...register('paymentMethod')}
                    className="mt-1 accent-primary"
                  />
                  <span className="grid gap-0.5">
                    <span className="font-medium">{paymentMethodLabels[method]}</span>
                    <span className="text-sm text-muted-foreground">
                      {method === 'Cod'
                        ? 'Thanh toán bằng tiền mặt khi nhận hàng.'
                        : 'Quét mã VietQR sau khi đặt hàng. Đơn được xác nhận khi shop nhận được tiền.'}
                    </span>
                  </span>
                </label>
              ))}
            </fieldset>
            <FormField
              id="customerNote"
              label="Ghi chú cho shop"
              error={formState.errors.customerNote?.message}
            >
              <Textarea rows={3} placeholder="Ví dụ: giao giờ hành chính" {...register('customerNote')} />
            </FormField>
          </Card>
        </div>

        <Card className="h-fit lg:sticky lg:top-24">
          <CardTitle>Đơn hàng</CardTitle>
          {estimate.isPending ? (
            <Skeleton className="h-40" />
          ) : estimate.isError ? (
            <ErrorState message={getErrorMessage(estimate.error)} onRetry={() => estimate.refetch()} />
          ) : (
            <>
              <ul className="grid gap-3">
                {estimate.data.items.map((line) => (
                  <li key={line.variantId} className="flex gap-3 text-sm">
                    <RemoteImage src={line.imageUrl} alt="" className="size-12 shrink-0 rounded-md" />
                    <div className="min-w-0 flex-1">
                      <p className="line-clamp-1 font-medium">{line.productName}</p>
                      <p className="text-muted-foreground">
                        {line.variantName} × {line.quantity}
                      </p>
                      {!line.isAvailable || line.quantity > line.available ? (
                        <p className="text-destructive">Không đủ hàng</p>
                      ) : null}
                    </div>
                    <p className="shrink-0 font-medium">{formatVnd(line.lineTotal)}</p>
                  </li>
                ))}
              </ul>
              <EstimateSummary estimate={estimate.data} />
            </>
          )}
          {estimate.data && blocked ? (
            <Alert variant="error">
              Một số sản phẩm không còn đủ hàng.{' '}
              <Link href="/gio-hang" className="underline">
                Quay lại giỏ hàng
              </Link>{' '}
              để điều chỉnh.
            </Alert>
          ) : null}
          {formError ? <Alert variant="error">{formError}</Alert> : null}
          <Button
            type="submit"
            size="lg"
            loading={placeOrder.isPending}
            disabled={blocked || estimate.isFetching}
          >
            {paymentMethod === 'BankTransfer' ? 'Đặt hàng và chuyển khoản' : 'Đặt hàng'}
          </Button>
          <p className="text-xs text-muted-foreground">
            Tổng tiền cuối cùng do hệ thống tính lại khi đặt hàng.
          </p>
        </Card>
      </form>
    </div>
  )
}
