import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'

export const MAX_LINE_QUANTITY = 99

/** Guest cart line: what we send to the API plus a display snapshot (prices are re-checked by the API). */
export interface GuestCartLine {
  variantId: string
  quantity: number
  productName: string
  productSlug: string
  variantName: string
  imageUrl: string | null
  unitPrice: number
}

interface CartState {
  lines: GuestCartLine[]
  add: (line: GuestCartLine) => void
  setQuantity: (variantId: string, quantity: number) => void
  remove: (variantId: string) => void
  clear: () => void
}

const clampQuantity = (quantity: number): number =>
  Math.max(1, Math.min(MAX_LINE_QUANTITY, Math.floor(Number.isFinite(quantity) ? quantity : 1)))

/** Guest cart, persisted to localStorage. Signed-in users use `/me/cart` instead (see features/cart). */
export const useGuestCartStore = create<CartState>()(
  persist(
    (set) => ({
      lines: [],
      add: (line) =>
        set((state) => {
          const existing = state.lines.find((l) => l.variantId === line.variantId)
          if (!existing)
            return { lines: [...state.lines, { ...line, quantity: clampQuantity(line.quantity) }] }
          return {
            lines: state.lines.map((l) =>
              l.variantId === line.variantId
                ? { ...line, quantity: clampQuantity(l.quantity + line.quantity) }
                : l,
            ),
          }
        }),
      setQuantity: (variantId, quantity) =>
        set((state) => ({
          lines: state.lines.map((l) =>
            l.variantId === variantId ? { ...l, quantity: clampQuantity(quantity) } : l,
          ),
        })),
      remove: (variantId) =>
        set((state) => ({ lines: state.lines.filter((l) => l.variantId !== variantId) })),
      clear: () => set({ lines: [] }),
    }),
    {
      name: 'snackshop-cart',
      version: 1,
      storage: createJSONStorage(() => localStorage),
    },
  ),
)

/** Display-only subtotal from snapshots. The real total always comes from `POST /orders/estimate`. */
export function guestSubtotal(lines: readonly GuestCartLine[]): number {
  return lines.reduce((sum, l) => sum + l.unitPrice * l.quantity, 0)
}

export function guestItemCount(lines: readonly GuestCartLine[]): number {
  return lines.reduce((sum, l) => sum + l.quantity, 0)
}
