import type { Money } from '@/lib/api/types'
import { discountPercent, formatVnd } from '@/lib/format'
import { cn } from '@/lib/utils'

type PriceTagProps = {
  price: Money | null | undefined
  compareAtPrice?: Money | null | undefined
  size?: 'sm' | 'lg'
  className?: string
}

export function PriceTag({ price, compareAtPrice, size = 'sm', className }: PriceTagProps) {
  const percent = discountPercent(price, compareAtPrice)
  return (
    <div className={cn('flex flex-wrap items-baseline gap-x-2 gap-y-0.5', className)}>
      <span className={cn('font-semibold text-primary', size === 'lg' ? 'text-2xl' : 'text-base')}>
        {price ? formatVnd(price) : 'Liên hệ'}
      </span>
      {percent !== null && compareAtPrice ? (
        <>
          <span className="text-sm text-muted-foreground line-through">
            <span className="sr-only">Giá gốc </span>
            {formatVnd(compareAtPrice)}
          </span>
          <span className="rounded bg-primary/10 px-1.5 text-xs font-semibold text-primary">-{percent}%</span>
        </>
      ) : null}
    </div>
  )
}
