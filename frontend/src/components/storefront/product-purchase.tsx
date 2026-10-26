'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'
import { toast } from 'sonner'
import { ShoppingBag } from 'lucide-react'
import { Button, buttonVariants } from '@/components/ui/button'
import { useCart } from '@/features/cart/use-cart'
import type { ProductDetail } from '@/lib/api/types'
import { getErrorMessage } from '@/lib/errors'
import { formatNumber } from '@/lib/format'
import { cn } from '@/lib/utils'
import { MAX_LINE_QUANTITY } from '@/stores/cart.store'
import { PriceTag } from './price-tag'
import { QuantityInput } from './quantity-input'

/** Variant picker + quantity + add to cart. Stock shown is the API's `availableQuantity`. */
export function ProductPurchase({ product }: { product: ProductDetail }) {
  const router = useRouter()
  const cart = useCart()
  const firstAvailable = product.variants.find((v) => v.availableQuantity > 0) ?? product.variants[0]
  const [variantId, setVariantId] = useState(firstAvailable?.id ?? '')
  const [quantity, setQuantity] = useState(1)
  const variant = product.variants.find((v) => v.id === variantId)

  if (!variant) return <p className="text-muted-foreground">Sản phẩm tạm ngừng kinh doanh.</p>

  const maxQuantity = Math.max(1, Math.min(MAX_LINE_QUANTITY, variant.availableQuantity))
  const inStock = variant.availableQuantity > 0
  const primaryImage = product.images.find((i) => i.isPrimary) ?? product.images[0]

  async function addToCart(goToCart: boolean) {
    if (!variant) return
    try {
      await cart.add({
        variantId: variant.id,
        quantity: Math.min(quantity, maxQuantity),
        productName: product.name,
        productSlug: product.slug,
        variantName: variant.name,
        imageUrl: primaryImage?.url ?? null,
        unitPrice: variant.price.amount,
      })
      if (goToCart) router.push('/gio-hang')
      else
        toast.success('Đã thêm vào giỏ hàng', {
          action: { label: 'Xem giỏ', onClick: () => router.push('/gio-hang') },
        })
    } catch (error) {
      toast.error(getErrorMessage(error))
    }
  }

  return (
    <div className="grid gap-5">
      <PriceTag price={variant.price} compareAtPrice={variant.compareAtPrice} size="lg" />

      {product.variants.length > 1 ? (
        <fieldset className="grid gap-2">
          <legend className="mb-2 text-sm font-medium">Phân loại</legend>
          <div className="flex flex-wrap gap-2">
            {product.variants.map((v) => {
              const selected = v.id === variantId
              const soldOut = v.availableQuantity <= 0
              return (
                <label
                  key={v.id}
                  className={cn(
                    'cursor-pointer rounded-lg border px-3 py-2 text-sm transition-colors has-[:focus-visible]:ring-2 has-[:focus-visible]:ring-ring',
                    selected
                      ? 'border-primary bg-primary/5 font-medium text-primary'
                      : 'hover:border-foreground/30',
                    soldOut && 'opacity-60',
                  )}
                >
                  <input
                    type="radio"
                    name="variant"
                    value={v.id}
                    checked={selected}
                    onChange={() => {
                      setVariantId(v.id)
                      setQuantity(1)
                    }}
                    className="sr-only"
                  />
                  {v.name}
                  {v.weightGrams ? <span className="text-muted-foreground"> · {v.weightGrams}g</span> : null}
                  {soldOut ? <span className="text-muted-foreground"> (hết hàng)</span> : null}
                </label>
              )
            })}
          </div>
        </fieldset>
      ) : null}

      <div className="flex flex-wrap items-center gap-3">
        <QuantityInput
          value={Math.min(quantity, maxQuantity)}
          onChange={setQuantity}
          max={maxQuantity}
          disabled={!inStock}
          label="Số lượng"
        />
        <p
          className={cn('text-sm', inStock ? 'text-muted-foreground' : 'font-medium text-destructive')}
          aria-live="polite"
        >
          {inStock ? `Còn ${formatNumber(variant.availableQuantity)} sản phẩm` : 'Tạm hết hàng'}
        </p>
      </div>

      <div className="flex flex-col gap-2 sm:flex-row">
        <Button
          size="lg"
          variant="outline"
          disabled={!inStock}
          loading={cart.isMutating}
          onClick={() => addToCart(false)}
        >
          <ShoppingBag aria-hidden />
          Thêm vào giỏ
        </Button>
        <Button size="lg" disabled={!inStock || cart.isMutating} onClick={() => addToCart(true)}>
          Mua ngay
        </Button>
      </div>
      <p className="text-xs text-muted-foreground">
        Không cần tài khoản để đặt hàng.{' '}
        <Link href="/tra-cuu-don" className={buttonVariants({ variant: 'link', size: 'sm' })}>
          Tra cứu đơn
        </Link>
      </p>
    </div>
  )
}
