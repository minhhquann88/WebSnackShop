import Link from 'next/link'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { cn } from '@/lib/utils'
import { buttonVariants } from './button'

type PaginationBaseProps = {
  page: number
  totalPages: number
  className?: string
}

function pageWindow(page: number, totalPages: number): number[] {
  const start = Math.max(1, Math.min(page - 2, totalPages - 4))
  const end = Math.min(totalPages, start + 4)
  return Array.from({ length: end - start + 1 }, (_, i) => start + i)
}

/** Link-based pagination for server-rendered listings (SEO-friendly). */
export function LinkPagination({
  page,
  totalPages,
  className,
  hrefForPage,
}: PaginationBaseProps & { hrefForPage: (page: number) => string }) {
  if (totalPages <= 1) return null
  return (
    <nav aria-label="Phân trang" className={cn('flex items-center justify-center gap-1', className)}>
      {page > 1 ? (
        <Link
          href={hrefForPage(page - 1)}
          className={buttonVariants({ variant: 'outline', size: 'icon-sm' })}
          aria-label="Trang trước"
        >
          <ChevronLeft aria-hidden />
        </Link>
      ) : null}
      {pageWindow(page, totalPages).map((p) => (
        <Link
          key={p}
          href={hrefForPage(p)}
          aria-current={p === page ? 'page' : undefined}
          className={buttonVariants({ variant: p === page ? 'default' : 'outline', size: 'icon-sm' })}
        >
          {p}
        </Link>
      ))}
      {page < totalPages ? (
        <Link
          href={hrefForPage(page + 1)}
          className={buttonVariants({ variant: 'outline', size: 'icon-sm' })}
          aria-label="Trang sau"
        >
          <ChevronRight aria-hidden />
        </Link>
      ) : null}
    </nav>
  )
}

/** Button-based pagination for client-side (TanStack Query) listings. */
export function Pagination({
  page,
  totalPages,
  className,
  onPageChange,
}: PaginationBaseProps & { onPageChange: (page: number) => void }) {
  if (totalPages <= 1) return null
  return (
    <nav aria-label="Phân trang" className={cn('flex items-center justify-center gap-1', className)}>
      <button
        type="button"
        className={buttonVariants({ variant: 'outline', size: 'icon-sm' })}
        onClick={() => onPageChange(page - 1)}
        disabled={page <= 1}
        aria-label="Trang trước"
      >
        <ChevronLeft aria-hidden />
      </button>
      {pageWindow(page, totalPages).map((p) => (
        <button
          type="button"
          key={p}
          onClick={() => onPageChange(p)}
          aria-current={p === page ? 'page' : undefined}
          className={buttonVariants({ variant: p === page ? 'default' : 'outline', size: 'icon-sm' })}
        >
          {p}
        </button>
      ))}
      <button
        type="button"
        className={buttonVariants({ variant: 'outline', size: 'icon-sm' })}
        onClick={() => onPageChange(page + 1)}
        disabled={page >= totalPages}
        aria-label="Trang sau"
      >
        <ChevronRight aria-hidden />
      </button>
    </nav>
  )
}

/** totalPages is optional in the generated schema; derive it when missing. */
export function totalPagesOf(paged: { totalPages?: number; totalCount: number; pageSize: number }): number {
  return paged.totalPages ?? Math.max(1, Math.ceil(paged.totalCount / Math.max(1, paged.pageSize)))
}
