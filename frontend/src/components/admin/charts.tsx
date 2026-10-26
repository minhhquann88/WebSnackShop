'use client'

import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'

/**
 * Single-series bar chart (one hue, no legend — the card title names the series).
 * Thin bars with rounded data ends, recessive grid, hover tooltip. Pair with a table view.
 */
export function SimpleBarChart({
  data,
  xKey,
  yKey,
  seriesLabel,
  formatValue = (v) => String(v),
  formatAxis,
  height = 260,
}: {
  data: Record<string, string | number>[]
  xKey: string
  yKey: string
  seriesLabel: string
  formatValue?: (value: number) => string
  formatAxis?: (value: number) => string
  height?: number
}) {
  return (
    <div style={{ height }} className="w-full">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: 0 }} barCategoryGap="25%">
          <CartesianGrid vertical={false} stroke="var(--border)" strokeDasharray="3 3" />
          <XAxis
            dataKey={xKey}
            tickLine={false}
            axisLine={{ stroke: 'var(--border)' }}
            tick={{ fill: 'var(--muted-foreground)', fontSize: 12 }}
            interval="preserveStartEnd"
            minTickGap={8}
          />
          <YAxis
            tickLine={false}
            axisLine={false}
            width={formatAxis ? 56 : 32}
            allowDecimals={false}
            tick={{ fill: 'var(--muted-foreground)', fontSize: 12 }}
            tickFormatter={formatAxis}
          />
          <Tooltip
            cursor={{ fill: 'var(--muted)' }}
            formatter={(value) => [formatValue(Number(value)), seriesLabel]}
            contentStyle={{
              borderRadius: 8,
              border: '1px solid var(--border)',
              background: 'var(--popover)',
              color: 'var(--popover-foreground)',
              fontSize: 13,
            }}
          />
          <Bar dataKey={yKey} fill="var(--chart-1)" radius={[4, 4, 0, 0]} maxBarSize={40} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}

/** Compact axis labels for VND amounts: 1.200.000 → "1,2tr", 45.000 → "45k". */
export function compactVnd(value: number): string {
  if (Math.abs(value) >= 1_000_000)
    return `${(value / 1_000_000).toLocaleString('vi-VN', { maximumFractionDigits: 1 })}tr`
  if (Math.abs(value) >= 1_000) return `${Math.round(value / 1_000)}k`
  return String(value)
}
