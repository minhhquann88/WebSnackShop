'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'
import { toast } from 'sonner'
import { ShoppingBag } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useCart } from '@/features/cart/use-cart'
import type { ProductDetail } from '@/lib/api/types'
import { getErrorMessage } from '@/lib/errors'
import { formatNumber } from '@/lib/format'
import { toneClasses, type Tone } from '@/lib/tone'
import { cn } from '@/lib/utils'
import { MAX_LINE_QUANTITY } from '@/stores/cart.store'
import { PriceStamp } from './price-tag'
import { QuantityInput } from './quantity-input'

/**
 * Variant picker + quantity + add to cart, printed on the product's bag face: the price is a
 * round stamp, variants are white pills (filled when chosen), the buy button is mango.
 * Stock shown is the API's `availableQuantity`.
 */
export function ProductPurchase({ product, tone }: { product: ProductDetail; tone: Tone }) {
  const router = useRouter()
  const cart = useCart()
  const c = toneClasses[tone]
  const firstAvailable = product.variants.find((v) => v.availableQuantity > 0) ?? product.variants[0]
  const [variantId, setVariantId] = useState(firstAvailable?.id ?? '')
  const [quantity, setQuantity] = useState(1)
  const variant = product.variants.find((v) => v.id === variantId)

  if (!variant) return <p className={c.muted}>Sản phẩm tạm ngừng kinh doanh.</p>

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
      <div className="flex flex-wrap items-center gap-4">
        <PriceStamp
          price={variant.price}
          compareAtPrice={variant.compareAtPrice}
          detail
          className={cn('size-28 text-lg', c.rim)}
        />
        {variant.weightGrams ? (
          <span className={cn('text-sm font-bold', c.muted)}>Khối lượng tịnh {variant.weightGrams}g</span>
        ) : null}
      </div>

      {product.variants.length > 1 ? (
        <fieldset className="grid gap-2">
          <legend className="mb-2 text-sm font-bold">Phân loại</legend>
          <div className="flex flex-wrap gap-2">
            {product.variants.map((v) => {
              const selected = v.id === variantId
              const soldOut = v.availableQuantity <= 0
              return (
                <label
                  key={v.id}
                  className={cn(
                    'cursor-pointer rounded-full border-2 px-4 py-2 text-sm font-bold transition-colors has-[:focus-visible]:ring-[3px] has-[:focus-visible]:ring-white/50',
                    selected
                      ? cn('border-card bg-card', c.rim)
                      : 'border-white/50 bg-transparent hover:border-white',
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
                  {v.weightGrams ? <span className="opacity-80"> · {v.weightGrams}g</span> : null}
                  {soldOut ? <span className="opacity-80"> (hết hàng)</span> : null}
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
          className="rounded-full border-2 border-white/50 bg-card/15 [&_button]:text-current [&_button:hover]:bg-white/20 [&_input]:text-current"
        />
        <p
          className={cn(
            'text-sm font-semibold',
            inStock ? c.muted : 'rounded-full bg-card px-3 py-1 text-chili-deep',
          )}
          aria-live="polite"
        >
          {inStock ? `Còn ${formatNumber(variant.availableQuantity)} sản phẩm` : 'Tạm hết hàng'}
        </p>
      </div>

      <div className="flex flex-col gap-2 sm:flex-row">
        <Button
          size="lg"
          variant="outline"
          className="border-white/60 bg-transparent text-current hover:border-white hover:bg-white/15"
          disabled={!inStock}
          loading={cart.isMutating}
          onClick={() => addToCart(false)}
        >
          <ShoppingBag aria-hidden />
          Thêm vào giỏ
        </Button>
        <Button
          size="lg"
          variant={tone === 'mango' ? 'default' : 'secondary'}
          disabled={!inStock || cart.isMutating}
          onClick={() => addToCart(true)}
        >
          Mua ngay
        </Button>
      </div>
      <p className={cn('text-xs', c.muted)}>
        Không cần tài khoản để đặt hàng.{' '}
        <Link href="/tra-cuu-don" className="font-bold underline underline-offset-4">
          Tra cứu đơn
        </Link>
      </p>
    </div>
  )
}
