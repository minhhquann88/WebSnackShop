import type { Money } from '@/lib/api/types'
import { discountPercent, formatVnd } from '@/lib/format'
import { cn } from '@/lib/utils'

type PriceTagProps = {
  price: Money | null | undefined
  compareAtPrice?: Money | null | undefined
  size?: 'sm' | 'lg'
  className?: string
}

/** Price printed in display type; a discounted price shows the old price struck and a chili "-x%" tag. */
export function PriceTag({ price, compareAtPrice, size = 'sm', className }: PriceTagProps) {
  const percent = discountPercent(price, compareAtPrice)
  const discounted = percent !== null && !!compareAtPrice
  return (
    <div className={cn('flex flex-wrap items-baseline gap-x-2 gap-y-0.5', className)}>
      <span
        className={cn(
          'font-display leading-none font-extrabold text-chili-deep tabular-nums',
          size === 'lg' ? 'text-4xl' : 'text-xl',
        )}
      >
        {price ? formatVnd(price) : 'Liên hệ'}
      </span>
      {discounted ? (
        <span className="inline-flex items-baseline gap-2 whitespace-nowrap">
          <span className="text-sm text-muted-foreground line-through">
            <span className="sr-only">Giá gốc </span>
            {formatVnd(compareAtPrice)}
          </span>
          <span className="rounded-md bg-chili px-1.5 py-0.5 text-xs font-extrabold text-primary-foreground">
            -{percent}%
          </span>
        </span>
      ) : null}
    </div>
  )
}

/** Round price stamp for a bag: white disc with the bag's colour as rim. */
export function PriceStamp({
  price,
  compareAtPrice,
  detail = false,
  className,
}: {
  price: Money | null | undefined
  compareAtPrice?: Money | null | undefined
  /** Print the struck compare price as well; the shelf stamp prints only the percent. */
  detail?: boolean
  className?: string
}) {
  const percent = discountPercent(price, compareAtPrice)
  return (
    <span className={cn('stamp font-display text-sm text-foreground', className)}>
      <span className="tabular-nums">{price ? formatVnd(price) : 'Liên hệ'}</span>
      {percent !== null && compareAtPrice ? (
        <span className="hidden text-[11px] font-bold text-chili-deep sm:block">
          {detail ? (
            <span className="text-muted-foreground line-through">{formatVnd(compareAtPrice)} </span>
          ) : null}
          -{percent}%
        </span>
      ) : null}
    </span>
  )
}
