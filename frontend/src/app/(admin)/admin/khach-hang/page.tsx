'use client'

import { Suspense, useState, type FormEvent } from 'react'
import Link from 'next/link'
import { Search } from 'lucide-react'
import { PageHeader } from '@/components/admin/page-header'
import { useUrlFilters } from '@/components/admin/use-url-filters'
import { Button } from '@/components/ui/button'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Pagination, totalPagesOf } from '@/components/ui/pagination'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useCustomersQuery } from '@/features/admin/people/queries'
import { getErrorMessage } from '@/lib/errors'
import { formatDate, formatDateTime, formatNumber, formatVnd } from '@/lib/format'

const PAGE_SIZE = 20

function Customers() {
  const { get, set, page } = useUrlFilters()
  const search = get('search')
  const [searchInput, setSearchInput] = useState(search)
  const query = useCustomersQuery({ search: search || undefined, page, pageSize: PAGE_SIZE })

  const onSearch = (e: FormEvent) => {
    e.preventDefault()
    set({ search: searchInput.trim() })
  }

  return (
    <>
      <PageHeader
        title="Khách hàng"
        description={query.data ? `${formatNumber(query.data.totalCount)} khách có tài khoản` : undefined}
      />
      <form onSubmit={onSearch} className="mb-4 grid max-w-xl gap-1.5">
        <Label htmlFor="customer-search">Tìm kiếm</Label>
        <div className="flex gap-2">
          <Input
            id="customer-search"
            placeholder="Tên, email hoặc số điện thoại"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          <Button type="submit" variant="outline" size="icon" aria-label="Tìm">
            <Search aria-hidden />
          </Button>
        </div>
      </form>
      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : query.data.items.length === 0 ? (
        <EmptyState title="Không tìm thấy khách hàng" />
      ) : (
        <div className="grid gap-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Khách hàng</TableHead>
                <TableHead>Điện thoại</TableHead>
                <TableHead className="text-right">Số đơn</TableHead>
                <TableHead className="text-right">Tổng chi tiêu</TableHead>
                <TableHead>Đơn gần nhất</TableHead>
                <TableHead>Tham gia</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data.items.map((c) => (
                <TableRow key={c.id}>
                  <TableCell>
                    <Link
                      href={`/admin/khach-hang/${c.id}`}
                      className="font-medium text-primary hover:underline"
                    >
                      {c.fullName}
                    </Link>
                    <p className="text-xs break-all text-muted-foreground">{c.email}</p>
                  </TableCell>
                  <TableCell>{c.phone ?? '—'}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatNumber(c.orderCount)}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatVnd(c.totalSpent)}</TableCell>
                  <TableCell className="whitespace-nowrap">{formatDateTime(c.lastOrderAt)}</TableCell>
                  <TableCell>{formatDate(c.createdAt)}</TableCell>
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
    </>
  )
}

export default function AdminCustomersPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <Customers />
    </Suspense>
  )
}
