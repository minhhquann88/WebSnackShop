import * as React from 'react'
import { cn } from '@/lib/utils'

/** The counter list: a white carton with hairline rows; the header row is printed in caps. */
export function Table({ className, ...props }: React.ComponentProps<'table'>) {
  return (
    <div className="relative w-full overflow-x-auto rounded-2xl border bg-card">
      <table className={cn('w-full caption-bottom text-sm', className)} {...props} />
    </div>
  )
}

export function TableHeader({ className, ...props }: React.ComponentProps<'thead'>) {
  return <thead className={cn('bg-cream-2/70 [&_tr]:border-b', className)} {...props} />
}

export function TableBody({ className, ...props }: React.ComponentProps<'tbody'>) {
  return <tbody className={cn('[&_tr:last-child]:border-0', className)} {...props} />
}

export function TableRow({ className, ...props }: React.ComponentProps<'tr'>) {
  return <tr className={cn('border-b transition-colors hover:bg-cream/80', className)} {...props} />
}

export function TableHead({ className, ...props }: React.ComponentProps<'th'>) {
  return (
    <th
      className={cn(
        'h-10 px-3 text-left align-middle text-[11px] font-extrabold tracking-wider whitespace-nowrap text-muted-foreground uppercase',
        className,
      )}
      {...props}
    />
  )
}

export function TableCell({ className, ...props }: React.ComponentProps<'td'>) {
  return <td className={cn('px-3 py-2.5 align-middle', className)} {...props} />
}

/** Row number in the first column: `<TableRowNumber index={i} page={page} pageSize={n} />`. */
export function TableRowNumber({
  index,
  page = 1,
  pageSize = 0,
}: {
  index: number
  page?: number
  pageSize?: number
}) {
  return (
    <td className="w-10 px-3 py-2.5 align-middle text-xs font-bold text-muted-foreground tabular-nums">
      {(page - 1) * pageSize + index + 1}
    </td>
  )
}

export function TableHeadNumber() {
  return (
    <th className="w-10 px-3">
      <span className="sr-only">STT</span>
    </th>
  )
}
