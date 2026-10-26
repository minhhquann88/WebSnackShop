'use client'

import { Suspense, useState, type FormEvent } from 'react'
import Link from 'next/link'
import { Search } from 'lucide-react'
import { PageHeader } from '@/components/admin/page-header'
import { useUrlFilters } from '@/components/admin/use-url-filters'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/status-badges'
import { Button } from '@/components/ui/button'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Input, NativeSelect } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Pagination, totalPagesOf } from '@/components/ui/pagination'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminOrdersQuery } from '@/features/admin/orders/queries'
import type { OrderStatus, PaymentMethod, PaymentStatus } from '@/lib/api/types'
import { getErrorMessage } from '@/lib/errors'
import { formatDateTime, formatNumber, formatVnd } from '@/lib/format'
import {
  ORDER_STATUSES,
  PAYMENT_METHODS,
  PAYMENT_STATUSES,
  orderStatusLabels,
  paymentMethodShortLabels,
  paymentStatusLabels,
} from '@/lib/labels'

const PAGE_SIZE = 20

function pick<T extends string>(value: string, allowed: readonly T[]): T | undefined {
  return (allowed as readonly string[]).includes(value) ? (value as T) : undefined
}

function OrdersList() {
  const { get, set, page } = useUrlFilters()
  const status = pick<OrderStatus>(get('status'), ORDER_STATUSES)
  const paymentMethod = pick<PaymentMethod>(get('paymentMethod'), PAYMENT_METHODS)
  const paymentStatus = pick<PaymentStatus>(get('paymentStatus'), PAYMENT_STATUSES)
  const from = get('from')
  const to = get('to')
  const search = get('search')
  const [searchInput, setSearchInput] = useState(search)

  const query = useAdminOrdersQuery({
    status,
    paymentMethod,
    paymentStatus,
    from: from || undefined,
    to: to || undefined,
    search: search || undefined,
    page,
    pageSize: PAGE_SIZE,
  })

  const onSearch = (e: FormEvent) => {
    e.preventDefault()
    set({ search: searchInput.trim() })
  }

  const hasFilters = !!(status || paymentMethod || paymentStatus || from || to || search)

  return (
    <>
      <PageHeader
        title="Đơn hàng"
        description={query.data ? `${formatNumber(query.data.totalCount)} đơn` : undefined}
      />

      <div className="mb-4 grid gap-3 rounded-xl border bg-card p-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        <form onSubmit={onSearch} className="grid gap-1.5 sm:col-span-2 lg:col-span-3 xl:col-span-2">
          <Label htmlFor="order-search">Tìm kiếm</Label>
          <div className="flex gap-2">
            <Input
              id="order-search"
              placeholder="Mã đơn, SĐT hoặc email"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
            <Button type="submit" variant="outline" size="icon" aria-label="Tìm">
              <Search aria-hidden />
            </Button>
          </div>
        </form>
        <div className="grid gap-1.5">
          <Label htmlFor="order-status">Trạng thái</Label>
          <NativeSelect
            id="order-status"
            value={status ?? ''}
            onChange={(e) => set({ status: e.target.value })}
          >
            <option value="">Tất cả</option>
            {ORDER_STATUSES.map((s) => (
              <option key={s} value={s}>
                {orderStatusLabels[s]}
              </option>
            ))}
          </NativeSelect>
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="order-pm">Thanh toán</Label>
          <NativeSelect
            id="order-pm"
            value={paymentMethod ?? ''}
            onChange={(e) => set({ paymentMethod: e.target.value })}
          >
            <option value="">Tất cả</option>
            {PAYMENT_METHODS.map((m) => (
              <option key={m} value={m}>
                {paymentMethodShortLabels[m]}
              </option>
            ))}
          </NativeSelect>
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="order-ps">Trạng thái thanh toán</Label>
          <NativeSelect
            id="order-ps"
            value={paymentStatus ?? ''}
            onChange={(e) => set({ paymentStatus: e.target.value })}
          >
            <option value="">Tất cả</option>
            {PAYMENT_STATUSES.map((s) => (
              <option key={s} value={s}>
                {paymentStatusLabels[s]}
              </option>
            ))}
          </NativeSelect>
        </div>
        <div className="grid grid-cols-2 gap-2 sm:col-span-2 lg:col-span-1 xl:col-span-1 xl:grid-cols-1">
          <div className="grid gap-1.5">
            <Label htmlFor="order-from">Từ ngày</Label>
            <Input
              id="order-from"
              type="date"
              value={from}
              max={to || undefined}
              onChange={(e) => set({ from: e.target.value })}
            />
          </div>
          <div className="grid gap-1.5">
            <Label htmlFor="order-to">Đến ngày</Label>
            <Input
              id="order-to"
              type="date"
              value={to}
              min={from || undefined}
              onChange={(e) => set({ to: e.target.value })}
            />
          </div>
        </div>
        {hasFilters ? (
          <div className="flex items-end sm:col-span-2 lg:col-span-3 xl:col-span-6">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setSearchInput('')
                set({
                  status: null,
                  paymentMethod: null,
                  paymentStatus: null,
                  from: null,
                  to: null,
                  search: null,
                })
              }}
            >
              Xóa bộ lọc
            </Button>
          </div>
        ) : null}
      </div>

      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : query.data.items.length === 0 ? (
        <EmptyState title="Không có đơn hàng" description={hasFilters ? 'Thử đổi bộ lọc.' : undefined} />
      ) : (
        <div className="grid gap-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Mã đơn</TableHead>
                <TableHead>Khách hàng</TableHead>
                <TableHead>Trạng thái</TableHead>
                <TableHead>Thanh toán</TableHead>
                <TableHead className="text-right">Tổng tiền</TableHead>
                <TableHead>Đặt lúc</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data.items.map((o) => (
                <TableRow key={o.id}>
                  <TableCell>
                    <Link
                      href={`/admin/don-hang/${o.id}`}
                      className="font-medium text-primary hover:underline"
                    >
                      {o.orderNumber}
                    </Link>
                    <p className="text-xs text-muted-foreground">{o.itemCount} sản phẩm</p>
                  </TableCell>
                  <TableCell>
                    <p>{o.customerName}</p>
                    <p className="text-xs text-muted-foreground">
                      {o.phone}
                      {o.customerId ? '' : ' · Khách vãng lai'}
                    </p>
                  </TableCell>
                  <TableCell>
                    <OrderStatusBadge status={o.status} />
                  </TableCell>
                  <TableCell>
                    <div className="grid gap-1">
                      <span className="text-xs">{paymentMethodShortLabels[o.paymentMethod]}</span>
                      <PaymentStatusBadge status={o.paymentStatus} />
                    </div>
                  </TableCell>
                  <TableCell className="text-right font-medium tabular-nums">{formatVnd(o.total)}</TableCell>
                  <TableCell className="whitespace-nowrap">{formatDateTime(o.placedAt)}</TableCell>
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

export default function AdminOrdersPage() {
  return (
    <Suspense fallback={<Spinner />}>
      <OrdersList />
    </Suspense>
  )
}
