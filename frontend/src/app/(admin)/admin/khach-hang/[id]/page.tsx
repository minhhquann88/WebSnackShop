'use client'

import Link from 'next/link'
import { useParams } from 'next/navigation'
import { PageHeader } from '@/components/admin/page-header'
import { StatTile } from '@/components/admin/stat-tile'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/status-badges'
import { Badge } from '@/components/ui/badge'
import { Card, CardTitle } from '@/components/ui/card'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useCustomerQuery } from '@/features/admin/people/queries'
import { getErrorMessage } from '@/lib/errors'
import { formatDate, formatDateTime, formatNumber, formatVnd } from '@/lib/format'

export default function AdminCustomerDetailPage() {
  const { id } = useParams<{ id: string }>()
  const query = useCustomerQuery(id)

  return (
    <>
      <PageHeader
        backHref="/admin/khach-hang"
        backLabel="Danh sách khách hàng"
        title={query.data?.fullName ?? 'Khách hàng'}
        description={query.data ? `${query.data.email} · ${query.data.phone ?? 'chưa có SĐT'}` : undefined}
      />
      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : (
        <div className="grid gap-6">
          <div className="grid gap-4 sm:grid-cols-3">
            <StatTile label="Số đơn" value={formatNumber(query.data.orderCount)} />
            <StatTile label="Tổng chi tiêu" value={formatVnd(query.data.totalSpent)} />
            <StatTile label="Tham gia" value={formatDate(query.data.createdAt)} />
          </div>
          <div className="grid gap-6 xl:grid-cols-3">
            <Card className="xl:col-span-2">
              <CardTitle>Đơn hàng gần đây</CardTitle>
              {query.data.recentOrders.length === 0 ? (
                <EmptyState title="Chưa có đơn hàng" />
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Mã đơn</TableHead>
                      <TableHead>Trạng thái</TableHead>
                      <TableHead>Thanh toán</TableHead>
                      <TableHead className="text-right">Tổng</TableHead>
                      <TableHead>Đặt lúc</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {query.data.recentOrders.map((o) => (
                      <TableRow key={o.id}>
                        <TableCell>
                          <Link
                            href={`/admin/don-hang/${o.id}`}
                            className="font-medium text-primary hover:underline"
                          >
                            {o.orderNumber}
                          </Link>
                          {o.firstItemName ? (
                            <p className="text-xs text-muted-foreground">
                              {o.firstItemName}
                              {o.itemCount > 1 ? ` và ${o.itemCount - 1} sản phẩm khác` : ''}
                            </p>
                          ) : null}
                        </TableCell>
                        <TableCell>
                          <OrderStatusBadge status={o.status} />
                        </TableCell>
                        <TableCell>
                          <PaymentStatusBadge status={o.paymentStatus} />
                        </TableCell>
                        <TableCell className="text-right tabular-nums">{formatVnd(o.total)}</TableCell>
                        <TableCell className="whitespace-nowrap">{formatDateTime(o.placedAt)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </Card>
            <Card>
              <CardTitle>Sổ địa chỉ</CardTitle>
              {query.data.addresses.length === 0 ? (
                <p className="text-sm text-muted-foreground">Chưa lưu địa chỉ nào.</p>
              ) : (
                <ul className="grid gap-3">
                  {query.data.addresses.map((a) => (
                    <li key={a.id} className="rounded-lg border p-3 text-sm">
                      <p className="flex flex-wrap items-center gap-2 font-medium">
                        {a.label ?? a.fullName}
                        {a.isDefault ? <Badge variant="secondary">Mặc định</Badge> : null}
                      </p>
                      <p>
                        {a.fullName} · {a.phone}
                      </p>
                      <p className="text-muted-foreground">
                        {a.line1}, {a.ward}, {a.district}, {a.province}
                      </p>
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          </div>
        </div>
      )}
    </>
  )
}
