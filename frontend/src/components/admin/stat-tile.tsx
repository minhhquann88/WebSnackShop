import type { ReactNode } from 'react'
import Link from 'next/link'
import { cn } from '@/lib/utils'

export function StatTile({
  label,
  value,
  hint,
  href,
  tone = 'default',
}: {
  label: string
  value: ReactNode
  hint?: string
  href?: string
  tone?: 'default' | 'warning'
}) {
  const body = (
    <>
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className={cn('text-2xl font-bold tabular-nums', tone === 'warning' && 'text-[oklch(0.5_0.14_60)]')}>
        {value}
      </p>
      {hint ? <p className="text-xs text-muted-foreground">{hint}</p> : null}
    </>
  )
  const className = 'grid gap-1 rounded-xl border bg-card p-4 shadow-xs'
  return href ? (
    <Link href={href} className={cn(className, 'transition-colors hover:border-primary/50')}>
      {body}
    </Link>
  ) : (
    <div className={className}>{body}</div>
  )
}
