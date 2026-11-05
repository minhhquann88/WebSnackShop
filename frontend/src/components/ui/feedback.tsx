import * as React from 'react'
import { AlertCircle, CheckCircle2, Info, Loader2, PackageOpen } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from './button'

export function Skeleton({ className, ...props }: React.ComponentProps<'div'>) {
  return <div className={cn('animate-pulse rounded-lg bg-cream-2', className)} aria-hidden {...props} />
}

export function Spinner({ label = 'Đang tải...', className }: { label?: string; className?: string }) {
  return (
    <div
      role="status"
      className={cn('flex items-center justify-center gap-2 py-10 text-muted-foreground', className)}
    >
      <Loader2 className="size-5 animate-spin text-chili" aria-hidden />
      <span className="text-sm">{label}</span>
    </div>
  )
}

type AlertProps = React.ComponentProps<'div'> & { variant?: 'info' | 'error' | 'success'; title?: string }

/** A printed notice: tinted field in the matching packaging colour, icon, no stripe. */
export function Alert({ variant = 'info', title, className, children, ...props }: AlertProps) {
  const Icon = variant === 'error' ? AlertCircle : variant === 'success' ? CheckCircle2 : Info
  return (
    <div
      role={variant === 'error' ? 'alert' : 'status'}
      className={cn(
        'flex gap-3 rounded-xl p-3 text-sm',
        variant === 'error' && 'bg-chili-tint text-chili-deep',
        variant === 'success' && 'bg-pandan-tint text-pandan-deep',
        variant === 'info' && 'bg-cobalt-tint text-cobalt-deep',
        className,
      )}
      {...props}
    >
      <Icon className="mt-0.5 size-4 shrink-0" aria-hidden />
      <div className="grid gap-1">
        {title ? <p className="font-bold">{title}</p> : null}
        {children ? <div className="text-foreground/85">{children}</div> : null}
      </div>
    </div>
  )
}

export function EmptyState({
  title,
  description,
  action,
  className,
}: {
  title: string
  description?: string
  action?: React.ReactNode
  className?: string
}) {
  return (
    <div
      className={cn(
        'flex flex-col items-center gap-3 rounded-2xl border border-dashed border-foreground/25 bg-card/60 p-10 text-center',
        className,
      )}
    >
      <PackageOpen className="size-10 text-chili" aria-hidden />
      <p className="font-display text-xl font-extrabold">{title}</p>
      {description ? <p className="max-w-sm text-sm text-muted-foreground">{description}</p> : null}
      {action}
    </div>
  )
}

export function ErrorState({
  message,
  onRetry,
  className,
}: {
  message: string
  onRetry?: () => void
  className?: string
}) {
  return (
    <div
      className={cn(
        'flex flex-col items-center gap-3 rounded-2xl bg-chili-tint p-8 text-center text-chili-deep',
        className,
      )}
    >
      <AlertCircle className="size-8" aria-hidden />
      <p className="text-sm font-semibold">{message}</p>
      {onRetry ? (
        <Button variant="outline" size="sm" onClick={onRetry}>
          Thử lại
        </Button>
      ) : null}
    </div>
  )
}
