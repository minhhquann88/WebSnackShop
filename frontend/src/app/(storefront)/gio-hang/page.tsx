'use client'

import Link from 'next/link'
import { Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { Button, buttonVariants } from '@/components/ui/button'
import { Card, CardTitle } from '@/components/ui/card'
import { Alert, EmptyState, ErrorState, Skeleton } from '@/components/ui/feedback'
import { RemoteImage } from '@/components/ui/remote-image'
import { EstimateSummary } from '@/components/storefront/order-summary'
import { QuantityInput } from '@/components/storefront/quantity-input'
import { useCart, useEstimate } from '@/features/cart/use-cart'
import type { PricedLine } from '@/lib/api/types'
import { getErrorMessage } from '@/lib/errors'
import { formatVnd } from '@/lib/format'
import { MAX_LINE_QUANTITY, useGuestCartStore } from '@/stores/cart.store'

type DisplayLine = {
  variantId: string
  productName: string
  productSlug: string
  variantName: string
  imageUrl: string | null
  quantity: number
  unitPrice: number | null
  lineTotal: number | null
  available: number
  isAvailable: boolean
}

function fromPriced(line: PricedLine): DisplayLine {
  return {
    variantId: line.variantId,
    productName: line.productName,
    productSlug: line.productSlug,
    variantName: line.variantName,
    imageUrl: line.imageUrl,
    quantity: line.quantity,
    unitPrice: line.unitPrice.amount,
    lineTotal: line.lineTotal.amount,
    available: line.available,
    isAvailable: line.isAvailable,
  }
}

export default function CartPage() {
  const cart = useCart()
  const guestLines = useGuestCartStore((s) => s.lines)
  const estimate = useEstimate(cart.items, cart.mode !== 'loading')

  if (cart.isLoading) {
    return (
      <div className="grid gap-3">
        <h1 className="text-2xl font-bold">Giỏ hàng</h1>
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
      </div>
    )
  }
  if (cart.mode === 'user' && cart.myCart.isError) {
    return <ErrorState message={getErrorMessage(cart.myCart.error)} onRetry={() => cart.myCart.refetch()} />
  }
  if (cart.items.length === 0) {
    return (
      <div className="grid gap-4">
        <h1 className="text-2xl font-bold">Giỏ hàng</h1>
        <EmptyState
          title="Giỏ hàng đang trống"
          description="Khám phá các món ăn vặt hấp dẫn và thêm vào giỏ nhé."
          action={
            <Link href="/san-pham" className={buttonVariants()}>
              Tiếp tục mua sắm
            </Link>
          }
        />
      </div>
    )
  }

  // Customers: lines come from /me/cart. Guests: priced by /orders/estimate, with the local
  // snapshot as fallback for variants the API no longer sells.
  let lines: DisplayLine[]
  if (cart.mode === 'user') {
    lines = (cart.myCart.data?.items ?? []).map(fromPriced)
  } else {
    const priced = new Map((estimate.data?.items ?? []).map((l) => [l.variantId, l]))
    const unavailable = new Set(estimate.data?.unavailableVariantIds ?? [])
    lines = guestLines.map((g) => {
      const p = priced.get(g.variantId)
      if (p) return fromPriced(p)
      return {
        ...g,
        unitPrice: estimate.data ? null : g.unitPrice,
        lineTotal: estimate.data ? null : g.unitPrice * g.quantity,
        available: estimate.data ? 0 : MAX_LINE_QUANTITY,
        isAvailable: !unavailable.has(g.variantId) && !estimate.data,
      }
    })
  }

  const hasProblems = lines.some((l) => !l.isAvailable || l.quantity > l.available)

  async function run(action: () => Promise<unknown>) {
    try {
      await action()
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  return (
    <div className="grid gap-6">
      <h1 className="text-2xl font-bold">Giỏ hàng ({cart.itemCount})</h1>
      <div className="grid gap-6 lg:grid-cols-[1fr_340px]">
        <ul className="grid content-start gap-3" aria-label="Sản phẩm trong giỏ">
          {lines.map((line) => {
            const problem = !line.isAvailable
              ? 'Sản phẩm không còn bán'
              : line.quantity > line.available
                ? `Chỉ còn ${line.available} sản phẩm`
                : null
            return (
              <li key={line.variantId} className="flex gap-3 rounded-xl border bg-card p-3">
                <Link href={`/san-pham/${line.productSlug}`} className="shrink-0">
                  <RemoteImage
                    src={line.imageUrl}
                    alt={line.productName}
                    className="size-20 rounded-lg sm:size-24"
                  />
                </Link>
                <div className="flex min-w-0 flex-1 flex-col gap-2">
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <Link
                        href={`/san-pham/${line.productSlug}`}
                        className="line-clamp-2 font-medium hover:text-primary"
                      >
                        {line.productName}
                      </Link>
                      <p className="text-sm text-muted-foreground">{line.variantName}</p>
                    </div>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label={`Xóa ${line.productName} khỏi giỏ`}
                      onClick={() => run(() => cart.remove(line.variantId))}
                      disabled={cart.isMutating}
                    >
                      <Trash2 aria-hidden />
                    </Button>
                  </div>
                  {problem ? <p className="text-sm font-medium text-destructive">{problem}</p> : null}
                  <div className="mt-auto flex flex-wrap items-center justify-between gap-2">
                    <QuantityInput
                      value={line.quantity}
                      max={Math.max(1, Math.min(MAX_LINE_QUANTITY, line.available || MAX_LINE_QUANTITY))}
                      onChange={(q) =>
                        run(() => cart.setQuantity({ variantId: line.variantId, quantity: q }))
                      }
                      disabled={cart.isMutating || !line.isAvailable}
                      label={`Số lượng ${line.productName}`}
                    />
                    <div className="text-right">
                      <p className="font-semibold">{formatVnd(line.lineTotal)}</p>
                      {line.unitPrice !== null && line.quantity > 1 ? (
                        <p className="text-xs text-muted-foreground">{formatVnd(line.unitPrice)} / sp</p>
                      ) : null}
                    </div>
                  </div>
                </div>
              </li>
            )
          })}
          <li>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => run(() => cart.clear())}
              disabled={cart.isMutating}
            >
              Xóa toàn bộ giỏ hàng
            </Button>
          </li>
        </ul>

        <Card className="h-fit lg:sticky lg:top-24">
          <CardTitle>Tóm tắt đơn hàng</CardTitle>
          {estimate.isPending ? (
            <Skeleton className="h-28" />
          ) : estimate.isError ? (
            <ErrorState message={getErrorMessage(estimate.error)} onRetry={() => estimate.refetch()} />
          ) : (
            <EstimateSummary estimate={estimate.data} />
          )}
          {hasProblems ? (
            <Alert variant="error">
              Một số sản phẩm không đủ hàng. Vui lòng điều chỉnh trước khi thanh toán.
            </Alert>
          ) : null}
          <Link
            href="/thanh-toan"
            aria-disabled={hasProblems || !estimate.data?.canPlaceOrder}
            className={buttonVariants({
              size: 'lg',
              className: hasProblems || !estimate.data?.canPlaceOrder ? 'pointer-events-none opacity-50' : '',
            })}
          >
            Tiến hành thanh toán
          </Link>
          <Link href="/san-pham" className={buttonVariants({ variant: 'link', size: 'sm' })}>
            Tiếp tục mua sắm
          </Link>
        </Card>
      </div>
    </div>
  )
}
