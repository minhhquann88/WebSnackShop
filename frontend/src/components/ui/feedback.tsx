import * as React from 'react'
import { AlertCircle, CheckCircle2, Info, Loader2, PackageOpen } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from './button'

export function Skeleton({ className, ...props }: React.ComponentProps<'div'>) {
  return <div className={cn('animate-pulse rounded-md bg-muted', className)} aria-hidden {...props} />
}

export function Spinner({ label = 'Đang tải...', className }: { label?: string; className?: string }) {
  return (
    <div
      role="status"
      className={cn('flex items-center justify-center gap-2 py-10 text-muted-foreground', className)}
    >
      <Loader2 className="size-5 animate-spin" aria-hidden />
      <span className="text-sm">{label}</span>
    </div>
  )
}

type AlertProps = React.ComponentProps<'div'> & { variant?: 'info' | 'error' | 'success'; title?: string }

export function Alert({ variant = 'info', title, className, children, ...props }: AlertProps) {
  const Icon = variant === 'error' ? AlertCircle : variant === 'success' ? CheckCircle2 : Info
  return (
    <div
      role={variant === 'error' ? 'alert' : 'status'}
      className={cn(
        'flex gap-3 rounded-lg border p-3 text-sm',
        variant === 'error' && 'border-destructive/30 bg-destructive/5 text-destructive',
        variant === 'success' && 'border-success/30 bg-success/5 text-success',
        variant === 'info' && 'bg-muted/50',
        className,
      )}
      {...props}
    >
      <Icon className="mt-0.5 size-4 shrink-0" aria-hidden />
      <div className="grid gap-1">
        {title ? <p className="font-medium">{title}</p> : null}
        {children ? (
          <div className={cn(variant === 'info' && 'text-muted-foreground')}>{children}</div>
        ) : null}
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
        'flex flex-col items-center gap-3 rounded-xl border border-dashed p-10 text-center',
        className,
      )}
    >
      <PackageOpen className="size-10 text-muted-foreground" aria-hidden />
      <p className="font-medium">{title}</p>
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
        'flex flex-col items-center gap-3 rounded-xl border border-destructive/30 p-8 text-center',
        className,
      )}
    >
      <AlertCircle className="size-8 text-destructive" aria-hidden />
      <p className="text-sm">{message}</p>
      {onRetry ? (
        <Button variant="outline" size="sm" onClick={onRetry}>
          Thử lại
        </Button>
      ) : null}
    </div>
  )
}
