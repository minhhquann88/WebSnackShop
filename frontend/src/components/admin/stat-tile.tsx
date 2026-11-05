import type { ReactNode } from 'react'
import Link from 'next/link'
import { cn } from '@/lib/utils'

/** A row of figure cartons on the counter. Wrap `StatTile`s in it. */
export function StatLedger({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <ul className={cn('grid gap-3 sm:grid-cols-2 xl:grid-cols-4', className)} data-slot="stat-ledger">
      {children}
    </ul>
  )
}

/** One figure carton: label on top, the figure printed large; warning tone gets a mango field. */
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
      <span className="text-sm font-semibold">{label}</span>
      <span
        className={cn(
          'font-display text-3xl leading-none font-extrabold tabular-nums',
          tone === 'warning' ? 'text-foreground' : 'text-chili-deep',
        )}
      >
        {value}
      </span>
      {hint ? <span className="text-xs text-muted-foreground">{hint}</span> : null}
    </>
  )
  const box = cn(
    'grid h-full content-start gap-2 rounded-2xl p-4',
    tone === 'warning' ? 'bg-mango' : 'border bg-card',
  )
  return (
    <li className="min-w-0">
      {href ? (
        <Link
          href={href}
          className={cn(
            box,
            'transition-colors lift focus-visible:ring-[3px] focus-visible:ring-ring/40 focus-visible:outline-none',
          )}
        >
          {body}
        </Link>
      ) : (
        <div className={box}>{body}</div>
      )}
    </li>
  )
}
