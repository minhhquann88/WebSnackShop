import type { Money } from '@/lib/api/types'

export const SHOP_TIME_ZONE = 'Asia/Ho_Chi_Minh'

const vndFormatter = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 })
const numberFormatter = new Intl.NumberFormat('vi-VN')
const datePartsFormatter = new Intl.DateTimeFormat('en-GB', {
  timeZone: SHOP_TIME_ZONE,
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

/** `formatVnd(45000) === "45.000 ₫"`. Accepts a number or a Money object; null/undefined → "—". */
export function formatVnd(value: number | Money | null | undefined): string {
  if (value === null || value === undefined) return '—'
  const amount = typeof value === 'number' ? value : value.amount
  return `${vndFormatter.format(Math.round(amount))} ₫`
}

export function formatNumber(value: number): string {
  return numberFormatter.format(value)
}

function shopParts(iso: string): Record<string, string> | null {
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return null
  return Object.fromEntries(datePartsFormatter.formatToParts(date).map((p) => [p.type, p.value]))
}

/** ISO UTC timestamp → Vietnam-local date `dd/MM/yyyy`. */
export function formatDate(iso: string | null | undefined): string {
  if (!iso) return '—'
  const p = shopParts(iso)
  return p ? `${p.day}/${p.month}/${p.year}` : '—'
}

/** ISO UTC timestamp → Vietnam-local `HH:mm dd/MM/yyyy`. */
export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '—'
  const p = shopParts(iso)
  return p ? `${p.hour}:${p.minute} ${p.day}/${p.month}/${p.year}` : '—'
}

/** `YYYY-MM-DD` of the given instant in Vietnam time (for report date filters). */
export function toShopDateString(date: Date): string {
  const p = shopParts(date.toISOString())!
  return `${p.year}-${p.month}-${p.day}`
}

/** `YYYY-MM-DD` (a calendar date from the API) → `dd/MM`. */
export function formatShortDay(isoDate: string): string {
  const [, month, day] = isoDate.split('-')
  return `${day}/${month}`
}

/** Percentage discount between a price and its compare-at price, or null. */
export function discountPercent(
  price: Money | null | undefined,
  compareAt: Money | null | undefined,
): number | null {
  if (!price || !compareAt || compareAt.amount <= price.amount || compareAt.amount <= 0) return null
  return Math.round((1 - price.amount / compareAt.amount) * 100)
}
