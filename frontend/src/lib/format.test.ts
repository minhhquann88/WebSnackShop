import {
  discountPercent,
  formatDate,
  formatDateTime,
  formatShortDay,
  formatVnd,
  toShopDateString,
} from './format'

const nbsp = / /g

describe('formatVnd', () => {
  it('formats numbers with Vietnamese thousand separators and the dong sign', () => {
    expect(formatVnd(45000).replace(nbsp, ' ')).toBe('45.000 ₫')
    expect(formatVnd(1234567).replace(nbsp, ' ')).toBe('1.234.567 ₫')
    expect(formatVnd(0).replace(nbsp, ' ')).toBe('0 ₫')
  })

  it('accepts Money objects and rounds fractional amounts', () => {
    expect(formatVnd({ amount: 65000.0, currency: 'VND' }).replace(nbsp, ' ')).toBe('65.000 ₫')
    expect(formatVnd({ amount: 999.6, currency: 'VND' }).replace(nbsp, ' ')).toBe('1.000 ₫')
  })

  it('renders a dash for missing values', () => {
    expect(formatVnd(null)).toBe('—')
    expect(formatVnd(undefined)).toBe('—')
  })
})

describe('dates in Asia/Ho_Chi_Minh', () => {
  it('converts UTC instants to Vietnam local time (UTC+7)', () => {
    expect(formatDateTime('2026-10-22T03:15:00+00:00')).toBe('10:15 22/10/2026')
    // 20:30 UTC is already the next day in Vietnam.
    expect(formatDate('2026-10-22T20:30:00Z')).toBe('23/10/2026')
    expect(formatDateTime('2026-10-22T17:00:00Z')).toBe('00:00 23/10/2026')
  })

  it('handles missing or invalid input', () => {
    expect(formatDate(null)).toBe('—')
    expect(formatDateTime('not-a-date')).toBe('—')
  })

  it('produces YYYY-MM-DD in shop time', () => {
    expect(toShopDateString(new Date('2026-10-22T18:00:00Z'))).toBe('2026-10-23')
    expect(formatShortDay('2026-10-05')).toBe('05/10')
  })
})

describe('discountPercent', () => {
  it('computes the rounded discount only when compare-at is higher', () => {
    expect(discountPercent({ amount: 65000, currency: 'VND' }, { amount: 78000, currency: 'VND' })).toBe(17)
    expect(discountPercent({ amount: 65000, currency: 'VND' }, null)).toBeNull()
    expect(discountPercent({ amount: 65000, currency: 'VND' }, { amount: 60000, currency: 'VND' })).toBeNull()
  })
})
