'use client'

import Link from 'next/link'
import { PageHeader } from '@/components/admin/page-header'
import { StatLedger, StatTile } from '@/components/admin/stat-tile'
import { SimpleBarChart } from '@/components/admin/charts'
import { Badge } from '@/components/ui/badge'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { EmptyState, ErrorState, Skeleton } from '@/components/ui/feedback'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useDashboardQuery } from '@/features/admin/reports/queries'
import { getErrorMessage } from '@/lib/errors'
import { formatNumber, formatVnd } from '@/lib/format'
import { orderStatusLabels } from '@/lib/labels'

export default function AdminDashboardPage() {
  const { data, isPending, isError, error, refetch } = useDashboardQuery()

  return (
    <>
      <PageHeader title="Tổng quan" description="Số liệu theo giờ Việt Nam, tự cập nhật mỗi phút." />
      {isPending ? (
        <div className="grid gap-3">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-24" />
          ))}
        </div>
      ) : isError ? (
        <ErrorState message={getErrorMessage(error)} onRetry={() => void refetch()} />
      ) : (
        <div className="grid gap-6">
          <div className="grid gap-3 xl:grid-cols-[2fr_3fr]">
            {/* The one figure the owner looks at first; the rest of the revenue reads as a ledger under it. */}
            <section
              aria-labelledby="revenue-heading"
              className="grid content-start gap-4 rounded-2xl border bg-card p-5"
            >
              <h2 id="revenue-heading" className="text-sm font-semibold">
                Doanh thu hôm nay
              </h2>
              <p className="font-display text-5xl leading-none font-extrabold text-chili-deep tabular-nums">
                {formatVnd(data.revenueToday)}
              </p>
              <dl className="grid gap-1.5 border-t pt-3 text-sm">
                <div className="flex justify-between gap-3">
                  <dt className="text-muted-foreground">7 ngày qua</dt>
                  <dd className="font-bold tabular-nums">{formatVnd(data.revenueLast7Days)}</dd>
                </div>
                <div className="flex justify-between gap-3">
                  <dt className="text-muted-foreground">30 ngày qua</dt>
                  <dd className="font-bold tabular-nums">{formatVnd(data.revenueLast30Days)}</dd>
                </div>
                <div className="flex justify-between gap-3">
                  <dt className="text-muted-foreground">Đơn đặt hôm nay</dt>
                  <dd className="font-bold tabular-nums">
                    <Link href="/admin/don-hang" className="text-chili-deep hover:underline">
                      {formatNumber(data.ordersPlacedToday)}
                    </Link>
                  </dd>
                </div>
              </dl>
            </section>
            <StatLedger className="content-start sm:grid-cols-3 xl:grid-cols-3">
              <StatTile
                label="Đơn chờ xác nhận"
                value={formatNumber(data.pendingOrders)}
                href="/admin/don-hang?status=Pending"
                tone={data.pendingOrders > 0 ? 'warning' : 'default'}
              />
              <StatTile
                label="Chuyển khoản chưa thanh toán"
                value={formatNumber(data.unpaidBankTransfers)}
                href="/admin/don-hang?paymentMethod=BankTransfer&paymentStatus=Unpaid"
                tone={data.unpaidBankTransfers > 0 ? 'warning' : 'default'}
              />
              <StatTile
                label="Phân loại sắp hết hàng"
                value={formatNumber(data.lowStockCount)}
                href="/admin/kho?lowStockOnly=true"
                tone={data.lowStockCount > 0 ? 'warning' : 'default'}
              />
            </StatLedger>
          </div>

          <div className="grid gap-6 xl:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Đơn hàng theo trạng thái</CardTitle>
                <CardDescription>Số đơn hiện có ở mỗi trạng thái.</CardDescription>
              </CardHeader>
              <SimpleBarChart
                data={data.ordersByStatus.map((s) => ({
                  label: orderStatusLabels[s.status],
                  count: s.count,
                }))}
                xKey="label"
                yKey="count"
                seriesLabel="Số đơn"
                formatValue={formatNumber}
              />
              <details className="text-sm">
                <summary className="cursor-pointer text-muted-foreground">Xem dạng bảng</summary>
                <ul className="mt-2 grid gap-1">
                  {data.ordersByStatus.map((s) => (
                    <li key={s.status} className="flex justify-between border-b py-1">
                      <span>{orderStatusLabels[s.status]}</span>
                      <span className="tabular-nums">{formatNumber(s.count)}</span>
                    </li>
                  ))}
                </ul>
              </details>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Sắp hết hàng</CardTitle>
                <CardDescription>
                  <Link href="/admin/kho?lowStockOnly=true" className="text-primary hover:underline">
                    Mở trang kho
                  </Link>
                </CardDescription>
              </CardHeader>
              {data.lowStockItems.length === 0 ? (
                <EmptyState title="Không có phân loại nào sắp hết hàng" />
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Sản phẩm</TableHead>
                      <TableHead>SKU</TableHead>
                      <TableHead className="text-right">Có thể bán</TableHead>
                      <TableHead className="text-right">Ngưỡng</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {data.lowStockItems.map((item) => (
                      <TableRow key={item.variantId}>
                        <TableCell>
                          <p className="font-medium">{item.productName}</p>
                          <p className="text-xs text-muted-foreground">{item.variantName}</p>
                        </TableCell>
                        <TableCell className="font-mono text-xs">{item.sku}</TableCell>
                        <TableCell className="text-right tabular-nums">
                          <Badge variant={item.available <= 0 ? 'destructive' : 'warning'}>
                            {formatNumber(item.available)}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right tabular-nums">
                          {formatNumber(item.lowStockThreshold)}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </Card>
          </div>
        </div>
      )}
    </>
  )
}
