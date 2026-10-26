'use client'

import { Suspense, useState, type FormEvent } from 'react'
import Link from 'next/link'
import { Search } from 'lucide-react'
import { useIsAdmin } from '@/components/admin/admin-only'
import { PageHeader } from '@/components/admin/page-header'
import { StockDialog, type StockDialogKind } from '@/components/admin/stock-dialog'
import { useUrlFilters } from '@/components/admin/use-url-filters'
import { Badge } from '@/components/ui/badge'
import { Button, buttonVariants } from '@/components/ui/button'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Checkbox, Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Pagination, totalPagesOf } from '@/components/ui/pagination'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useStockQuery } from '@/features/admin/inventory/queries'
import type { Schemas } from '@/lib/api/types'
import { getErrorMessage } from '@/lib/errors'
import { formatNumber } from '@/lib/format'

const PAGE_SIZE = 20

function Inventory() {
  const admin = useIsAdmin()
  const { get, set, page } = useUrlFilters()
  const search = get('search')
  const lowStockOnly = get('lowStockOnly') === 'true'
  const [searchInput, setSearchInput] = useState(search)
  const [dialog, setDialog] = useState<{
    kind: StockDialogKind
    stock: Schemas['StockLevelResponse']
    key: number
  } | null>(null)

  const query = useStockQuery({ search: search || undefined, lowStockOnly, page, pageSize: PAGE_SIZE })

  const onSearch = (e: FormEvent) => {
    e.preventDefault()
    set({ search: searchInput.trim() })
  }
  const open = (kind: StockDialogKind, stock: Schemas['StockLevelResponse']) =>
    setDialog((d) => ({ kind, stock, key: (d?.key ?? 0) + 1 }))

  return (
    <>
      <PageHeader
        title="Kho hàng"
        description={query.data ? `${formatNumber(query.data.totalCount)} phân loại` : undefined}
      />
      <div className="mb-4 flex flex-col gap-3 rounded-xl border bg-card p-4 sm:flex-row sm:items-end">
        <form onSubmit={onSearch} className="grid flex-1 gap-1.5">
          <Label htmlFor="stock-search">Tìm kiếm</Label>
          <div className="flex gap-2">
            <Input
              id="stock-search"
              placeholder="Tên sản phẩm hoặc SKU"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
            <Button type="submit" variant="outline" size="icon" aria-label="Tìm">
              <Search aria-hidden />
            </Button>
          </div>
        </form>
        <Label htmlFor="low-only" className="h-10 font-normal">
          <Checkbox
            id="low-only"
            checked={lowStockOnly}
            onChange={(e) => set({ lowStockOnly: e.target.checked })}
          />
          Chỉ hàng sắp hết
        </Label>
      </div>

      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : query.data.items.length === 0 ? (
        <EmptyState title="Không có dữ liệu tồn kho" />
      ) : (
        <div className="grid gap-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Sản phẩm</TableHead>
                <TableHead>SKU</TableHead>
                <TableHead className="text-right">Tồn</TableHead>
                <TableHead className="text-right">Đang giữ</TableHead>
                <TableHead className="text-right">Có thể bán</TableHead>
                <TableHead className="text-right">Ngưỡng</TableHead>
                <TableHead>
                  <span className="sr-only">Thao tác</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data.items.map((s) => (
                <TableRow key={s.variantId}>
                  <TableCell>
                    <p className="font-medium">{s.productName}</p>
                    <p className="text-xs text-muted-foreground">{s.variantName}</p>
                  </TableCell>
                  <TableCell className="font-mono text-xs">{s.sku}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatNumber(s.quantityOnHand)}</TableCell>
                  <TableCell className="text-right tabular-nums">
                    {formatNumber(s.quantityReserved)}
                  </TableCell>
                  <TableCell className="text-right">
                    <Badge variant={s.available <= 0 ? 'destructive' : s.isLowStock ? 'warning' : 'muted'}>
                      {formatNumber(s.available)}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right tabular-nums">
                    {formatNumber(s.lowStockThreshold)}
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-wrap justify-end gap-1">
                      <Button size="sm" variant="outline" onClick={() => open('receive', s)}>
                        Nhập
                      </Button>
                      {admin ? (
                        <>
                          <Button size="sm" variant="outline" onClick={() => open('adjust', s)}>
                            Điều chỉnh
                          </Button>
                          <Button size="sm" variant="outline" onClick={() => open('threshold', s)}>
                            Ngưỡng
                          </Button>
                        </>
                      ) : null}
                      <Link
                        href={`/admin/kho/${s.variantId}?name=${encodeURIComponent(`${s.productName} · ${s.variantName}`)}`}
                        className={buttonVariants({ size: 'sm', variant: 'ghost' })}
                      >
                        Lịch sử
                      </Link>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Pagination
            page={page}
            totalPages={totalPagesOf(query.data)}
            onPageChange={(p) => set({ page: p })}
          />
        </div>
      )}
      {dialog ? (
        <StockDialog
          key={dialog.key}
          kind={dialog.kind}
          stock={dialog.stock}
          onOpenChange={(o) => (o ? undefined : setDialog(null))}
        />
      ) : null}
    </>
  )
}

export default function AdminInventoryPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <Inventory />
    </Suspense>
  )
}
