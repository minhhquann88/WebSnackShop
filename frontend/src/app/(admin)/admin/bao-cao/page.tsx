'use client'

import { useState } from 'react'
import { AdminOnly } from '@/components/admin/admin-only'
import { SimpleBarChart, compactVnd } from '@/components/admin/charts'
import { PageHeader } from '@/components/admin/page-header'
import { StatTile } from '@/components/admin/stat-tile'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useRevenueQuery, useTopProductsQuery } from '@/features/admin/reports/queries'
import { getErrorMessage } from '@/lib/errors'
import { formatNumber, formatShortDay, formatVnd, toShopDateString } from '@/lib/format'

const DAY_MS = 86_400_000

function daysAgo(days: number): string {
  return toShopDateString(new Date(Date.now() - days * DAY_MS))
}

const PRESETS = [
  { label: '7 ngày', days: 6 },
  { label: '30 ngày', days: 29 },
  { label: '90 ngày', days: 89 },
]

function Reports() {
  const today = toShopDateString(new Date())
  const [from, setFrom] = useState(() => daysAgo(29))
  const [to, setTo] = useState(today)
  const valid = from !== '' && to !== '' && from <= to

  const revenue = useRevenueQuery(from, to, valid)
  const top = useTopProductsQuery(from, to, 10, valid)

  return (
    <>
      <PageHeader
        title="Báo cáo doanh thu"
        description="Doanh thu tính theo đơn đã giao, theo ngày giờ Việt Nam."
      />
      <div className="mb-6 flex flex-wrap items-end gap-3 rounded-xl border bg-card p-4">
        <div className="grid gap-1.5">
          <Label htmlFor="report-from">Từ ngày</Label>
          <Input
            id="report-from"
            type="date"
            value={from}
            max={to}
            onChange={(e) => setFrom(e.target.value)}
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="report-to">Đến ngày</Label>
          <Input
            id="report-to"
            type="date"
            value={to}
            min={from}
            max={today}
            onChange={(e) => setTo(e.target.value)}
          />
        </div>
        <div className="flex flex-wrap gap-2" role="group" aria-label="Khoảng thời gian nhanh">
          {PRESETS.map((p) => {
            const presetFrom = daysAgo(p.days)
            const active = from === presetFrom && to === today
            return (
              <Button
                key={p.days}
                size="sm"
                variant={active ? 'default' : 'outline'}
                aria-pressed={active}
                onClick={() => {
                  setFrom(presetFrom)
                  setTo(today)
                }}
              >
                {p.label}
              </Button>
            )
          })}
        </div>
      </div>

      {!valid ? (
        <EmptyState
          title="Khoảng ngày không hợp lệ"
          description="Ngày bắt đầu phải trước hoặc bằng ngày kết thúc."
        />
      ) : (
        <div className="grid gap-6">
          {revenue.isPending ? (
            <Spinner />
          ) : revenue.isError ? (
            <ErrorState message={getErrorMessage(revenue.error)} onRetry={() => void revenue.refetch()} />
          ) : (
            <>
              <div className="grid gap-4 sm:grid-cols-3">
                <StatTile label="Tổng doanh thu" value={formatVnd(revenue.data.totalRevenue)} />
                <StatTile label="Số đơn" value={formatNumber(revenue.data.orderCount)} />
                <StatTile
                  label="Giá trị trung bình / đơn"
                  value={
                    revenue.data.orderCount > 0
                      ? formatVnd(revenue.data.totalRevenue.amount / revenue.data.orderCount)
                      : '—'
                  }
                />
              </div>
              <Card>
                <CardHeader>
                  <CardTitle>Doanh thu theo ngày</CardTitle>
                  <CardDescription>
                    {formatShortDay(revenue.data.from)} – {formatShortDay(revenue.data.to)}
                  </CardDescription>
                </CardHeader>
                {revenue.data.days.length === 0 ? (
                  <EmptyState title="Chưa có doanh thu trong khoảng này" />
                ) : (
                  <>
                    <SimpleBarChart
                      data={revenue.data.days.map((d) => ({
                        day: formatShortDay(d.date),
                        revenue: d.revenue.amount,
                      }))}
                      xKey="day"
                      yKey="revenue"
                      seriesLabel="Doanh thu"
                      formatValue={formatVnd}
                      formatAxis={compactVnd}
                      height={300}
                    />
                    <details className="text-sm">
                      <summary className="cursor-pointer text-muted-foreground">Xem dạng bảng</summary>
                      <Table className="mt-2">
                        <TableHeader>
                          <TableRow>
                            <TableHead>Ngày</TableHead>
                            <TableHead className="text-right">Số đơn</TableHead>
                            <TableHead className="text-right">Doanh thu</TableHead>
                          </TableRow>
                        </TableHeader>
                        <TableBody>
                          {revenue.data.days.map((d) => (
                            <TableRow key={d.date}>
                              <TableCell>{formatShortDay(d.date)}</TableCell>
                              <TableCell className="text-right tabular-nums">
                                {formatNumber(d.orderCount)}
                              </TableCell>
                              <TableCell className="text-right tabular-nums">
                                {formatVnd(d.revenue)}
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </details>
                  </>
                )}
              </Card>
            </>
          )}

          <Card>
            <CardTitle>Sản phẩm bán chạy</CardTitle>
            {top.isPending ? (
              <Spinner />
            ) : top.isError ? (
              <ErrorState message={getErrorMessage(top.error)} onRetry={() => void top.refetch()} />
            ) : top.data.length === 0 ? (
              <EmptyState title="Chưa có sản phẩm nào được bán trong khoảng này" />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>#</TableHead>
                    <TableHead>Sản phẩm</TableHead>
                    <TableHead className="text-right">Số lượng</TableHead>
                    <TableHead className="text-right">Doanh thu</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {top.data.map((p, i) => (
                    <TableRow key={p.productId}>
                      <TableCell className="tabular-nums">{i + 1}</TableCell>
                      <TableCell className="font-medium">{p.productName}</TableCell>
                      <TableCell className="text-right tabular-nums">
                        {formatNumber(p.quantitySold)}
                      </TableCell>
                      <TableCell className="text-right tabular-nums">{formatVnd(p.revenue)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </Card>
        </div>
      )}
    </>
  )
}

export default function AdminReportsPage() {
  return (
    <AdminOnly>
      <Reports />
    </AdminOnly>
  )
}
