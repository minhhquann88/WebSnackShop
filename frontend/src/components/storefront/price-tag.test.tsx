import { render, screen } from '@testing-library/react'
import { PriceTag } from './price-tag'

const vnd = (amount: number) => ({ amount, currency: 'VND' })

describe('PriceTag', () => {
  it('shows price, struck-through compare-at price and discount badge', () => {
    render(<PriceTag price={vnd(65000)} compareAtPrice={vnd(78000)} />)
    expect(screen.getByText(/65\.000/)).toBeInTheDocument()
    expect(screen.getByText(/78\.000/)).toHaveClass('line-through')
    expect(screen.getByText('-17%')).toBeInTheDocument()
  })

  it('hides the discount when there is no compare-at price', () => {
    render(<PriceTag price={vnd(65000)} />)
    expect(screen.queryByText(/%/)).not.toBeInTheDocument()
  })

  it('shows "Liên hệ" when the product has no price', () => {
    render(<PriceTag price={null} />)
    expect(screen.getByText('Liên hệ')).toBeInTheDocument()
  })
})
