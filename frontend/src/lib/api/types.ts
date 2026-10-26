import type { components } from './schema'

export type Schemas = components['schemas']

export type Money = Schemas['MoneyResponse']
export type OrderStatus = Schemas['OrderStatus']
export type PaymentMethod = Schemas['PaymentMethod']
export type PaymentStatus = Schemas['PaymentStatus']
export type ActorType = NonNullable<Schemas['ActorType']>
export type StockMovementType = Schemas['StockMovementType']
export type User = Schemas['UserResponse']
export type CategorySummary = Schemas['CategoryResponse']
export type ProductSummary = Schemas['ProductSummaryResponse']
export type ProductDetail = Schemas['ProductDetailResponse']
export type ProductVariant = Schemas['ProductVariantResponse']
export type PricedLine = Schemas['PricedLineResponse']
export type CartLine = Schemas['CartLineRequest']
export type OrderEstimate = Schemas['OrderEstimateResponse']
export type PlaceOrderResponse = Schemas['PlaceOrderResponse']
export type BankTransferInstructions = Schemas['BankTransferInstructionsResponse']
export type OrderDetail = Schemas['OrderDetailResponse']
export type OrderSummary = Schemas['OrderSummaryResponse']
export type SavedAddress = Schemas['SavedAddressResponse']
export type Address = Schemas['AddressRequest']

/** Shape shared by every PagedResponseOf* schema. */
export interface Paged<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages?: number
  hasNextPage?: boolean
  hasPreviousPage?: boolean
}

export const ROLES = { Customer: 'Customer', Staff: 'Staff', Admin: 'Admin' } as const
export type Role = (typeof ROLES)[keyof typeof ROLES]
