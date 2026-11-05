import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

/**
 * Two printed labels: a `tag` (small rounded label, tinted) and a `stamp` (solid colour pill,
 * uppercase, for states that were set: order and payment status).
 */
export const badgeVariants = cva('inline-flex w-fit shrink-0 items-center gap-1 whitespace-nowrap', {
  variants: {
    shape: {
      sticker: 'rounded-md px-2 py-0.5 text-xs font-bold',
      stamp: 'rounded-full px-2.5 py-1 text-[11px] leading-none font-extrabold tracking-wide uppercase',
    },
    variant: {
      default: '',
      secondary: '',
      outline: '',
      success: '',
      warning: '',
      destructive: '',
      info: '',
      muted: '',
    },
  },
  compoundVariants: [
    { shape: 'sticker', variant: 'default', class: 'bg-chili text-primary-foreground' },
    { shape: 'sticker', variant: 'secondary', class: 'bg-mango text-foreground' },
    { shape: 'sticker', variant: 'outline', class: 'border border-foreground/30 text-foreground' },
    { shape: 'sticker', variant: 'success', class: 'bg-pandan-tint text-pandan-deep' },
    { shape: 'sticker', variant: 'warning', class: 'bg-mango-tint text-mango-text' },
    { shape: 'sticker', variant: 'destructive', class: 'bg-chili-tint text-chili-deep' },
    { shape: 'sticker', variant: 'info', class: 'bg-cobalt-tint text-cobalt-deep' },
    { shape: 'sticker', variant: 'muted', class: 'bg-cream-2 text-muted-foreground' },
    { shape: 'stamp', variant: 'default', class: 'bg-chili text-primary-foreground' },
    { shape: 'stamp', variant: 'secondary', class: 'bg-mango text-foreground' },
    { shape: 'stamp', variant: 'outline', class: 'border border-foreground/40 text-foreground' },
    { shape: 'stamp', variant: 'success', class: 'bg-pandan text-primary-foreground' },
    { shape: 'stamp', variant: 'warning', class: 'bg-mango text-foreground' },
    { shape: 'stamp', variant: 'destructive', class: 'bg-chili-deep text-primary-foreground' },
    { shape: 'stamp', variant: 'info', class: 'bg-cobalt text-primary-foreground' },
    { shape: 'stamp', variant: 'muted', class: 'bg-cream-2 text-muted-foreground' },
  ],
  defaultVariants: { variant: 'default', shape: 'sticker' },
})

export type BadgeVariant = NonNullable<VariantProps<typeof badgeVariants>['variant']>

export function Badge({
  className,
  variant,
  shape,
  ...props
}: React.ComponentProps<'span'> & VariantProps<typeof badgeVariants>) {
  return <span data-slot="badge" className={cn(badgeVariants({ variant, shape }), className)} {...props} />
}
