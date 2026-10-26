import type { ReactNode } from 'react'
import Link from 'next/link'
import { ChevronLeft } from 'lucide-react'

export function PageHeader({
  title,
  description,
  actions,
  backHref,
  backLabel = 'Quay lại',
}: {
  title: string
  description?: ReactNode
  actions?: ReactNode
  backHref?: string
  backLabel?: string
}) {
  return (
    <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
      <div className="grid gap-1">
        {backHref ? (
          <Link
            href={backHref}
            className="flex w-fit items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
          >
            <ChevronLeft className="size-4" aria-hidden />
            {backLabel}
          </Link>
        ) : null}
        <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
        {description ? <div className="text-sm text-muted-foreground">{description}</div> : null}
      </div>
      {actions ? <div className="flex flex-wrap gap-2">{actions}</div> : null}
    </div>
  )
}
