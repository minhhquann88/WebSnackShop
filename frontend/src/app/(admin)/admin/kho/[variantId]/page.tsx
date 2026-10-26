'use client'

import { Suspense, useState } from 'react'
import { useParams, useSearchParams } from 'next/navigation'
import { PageHeader } from '@/components/admin/page-header'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Pagination, totalPagesOf } from '@/components/ui/pagination'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useStockMovementsQuery } from '@/features/admin/inventory/queries'
import { getErrorMessage } from '@/lib/errors'
import { formatDateTime, formatNumber } from '@/lib/format'
import { stockMovementLabels } from '@/lib/labels'

function Movements() {
  const { variantId } = useParams<{ variantId: string }>()
  const name = useSearchParams().get('name')
  const [page, setPage] = useState(1)
  const query = useStockMovementsQuery(variantId, page)

  return (
    <>
      <PageHeader
        title="Lịch sử kho"
        description={name ?? undefined}
        backHref="/admin/kho"
        backLabel="Kho hàng"
      />
      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : query.data.items.length === 0 ? (
        <EmptyState title="Chưa có biến động kho" />
      ) : (
        <div className="grid gap-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Thời gian</TableHead>
                <TableHead>Loại</TableHead>
                <TableHead className="text-right">Số lượng</TableHead>
                <TableHead className="text-right">Tồn sau</TableHead>
                <TableHead className="text-right">Giữ sau</TableHead>
                <TableHead>Ghi chú</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data.items.map((m) => (
                <TableRow key={m.id}>
                  <TableCell className="whitespace-nowrap">{formatDateTime(m.createdAt)}</TableCell>
                  <TableCell>{stockMovementLabels[m.type]}</TableCell>
                  <TableCell className="text-right tabular-nums">
                    {m.quantity > 0 ? '+' : ''}
                    {formatNumber(m.quantity)}
                  </TableCell>
                  <TableCell className="text-right tabular-nums">{formatNumber(m.onHandAfter)}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatNumber(m.reservedAfter)}</TableCell>
                  <TableCell className="max-w-xs text-sm">
                    {m.note ?? '—'}
                    {m.referenceType ? (
                      <p className="text-xs text-muted-foreground">
                        {m.referenceType === 'Order' ? 'Theo đơn hàng' : m.referenceType}
                      </p>
                    ) : null}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Pagination page={page} totalPages={totalPagesOf(query.data)} onPageChange={setPage} />
        </div>
      )}
    </>
  )
}

export default function StockMovementsPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <Movements />
    </Suspense>
  )
}
