import {
  guestItemCount,
  guestSubtotal,
  MAX_LINE_QUANTITY,
  useGuestCartStore,
  type GuestCartLine,
} from './cart.store'

const line = (overrides: Partial<GuestCartLine> = {}): GuestCartLine => ({
  variantId: 'v1',
  quantity: 1,
  productName: 'Khô gà lá chanh',
  productSlug: 'kho-ga-la-chanh',
  variantName: 'Gói tiêu chuẩn',
  imageUrl: null,
  unitPrice: 65000,
  ...overrides,
})

describe('guest cart store', () => {
  beforeEach(() => {
    localStorage.clear()
    useGuestCartStore.getState().clear()
  })

  it('adds lines and merges quantities for the same variant', () => {
    const { add } = useGuestCartStore.getState()
    add(line({ quantity: 2 }))
    add(line({ quantity: 3 }))
    add(line({ variantId: 'v2', unitPrice: 155000 }))
    const { lines } = useGuestCartStore.getState()
    expect(lines).toHaveLength(2)
    expect(lines.find((l) => l.variantId === 'v1')?.quantity).toBe(5)
    expect(guestItemCount(lines)).toBe(6)
    expect(guestSubtotal(lines)).toBe(5 * 65000 + 155000)
  })

  it('clamps quantities to 1..99', () => {
    const { add, setQuantity } = useGuestCartStore.getState()
    add(line({ quantity: 98 }))
    add(line({ quantity: 5 }))
    expect(useGuestCartStore.getState().lines[0]?.quantity).toBe(MAX_LINE_QUANTITY)
    setQuantity('v1', 0)
    expect(useGuestCartStore.getState().lines[0]?.quantity).toBe(1)
    setQuantity('v1', Number.NaN)
    expect(useGuestCartStore.getState().lines[0]?.quantity).toBe(1)
  })

  it('removes and clears lines', () => {
    const { add, remove, clear } = useGuestCartStore.getState()
    add(line())
    add(line({ variantId: 'v2' }))
    remove('v1')
    expect(useGuestCartStore.getState().lines.map((l) => l.variantId)).toEqual(['v2'])
    clear()
    expect(useGuestCartStore.getState().lines).toEqual([])
  })

  it('persists to localStorage', () => {
    useGuestCartStore.getState().add(line({ quantity: 4 }))
    const stored = JSON.parse(localStorage.getItem('snackshop-cart') ?? '{}') as {
      state?: { lines?: GuestCartLine[] }
    }
    expect(stored.state?.lines?.[0]).toMatchObject({ variantId: 'v1', quantity: 4 })
  })
})
