import { z } from 'zod'
import type { Schemas } from '@/lib/api/types'

/** Form values are kept as strings (what inputs produce) and converted to API shapes on submit. */
const optionalInt = (label: string, min = 0) =>
  z
    .string()
    .trim()
    .refine(
      (v) => v === '' || (/^-?\d+$/.test(v) && Number(v) >= min),
      `${label} phải là số nguyên ≥ ${min}.`,
    )

const requiredMoney = z
  .string()
  .trim()
  .min(1, 'Vui lòng nhập giá.')
  .refine((v) => /^\d+$/.test(v) && Number(v) > 0, 'Giá phải là số nguyên dương (VND).')

const optionalMoney = z
  .string()
  .trim()
  .refine((v) => v === '' || (/^\d+$/.test(v) && Number(v) > 0), 'Giá phải là số nguyên dương (VND).')

export const slugField = z
  .string()
  .trim()
  .refine(
    (v) => v === '' || /^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(v),
    'Chỉ dùng chữ thường không dấu, số và dấu gạch ngang.',
  )

export const productDetailsSchema = z.object({
  categoryId: z.string().min(1, 'Vui lòng chọn danh mục.'),
  name: z.string().trim().min(1, 'Vui lòng nhập tên sản phẩm.').max(200, 'Tên quá dài.'),
  slug: slugField,
  shortDescription: z.string().trim().max(500, 'Mô tả ngắn quá dài.'),
  description: z.string().trim().max(10000, 'Mô tả quá dài.'),
  ingredients: z.string().trim().max(2000, 'Nội dung quá dài.'),
  origin: z.string().trim().max(200, 'Nội dung quá dài.'),
  brand: z.string().trim().max(200, 'Nội dung quá dài.'),
  shelfLifeDays: optionalInt('Hạn sử dụng', 1),
})
export type ProductDetailsValues = z.infer<typeof productDetailsSchema>

export const variantSchema = z
  .object({
    name: z.string().trim().max(100, 'Tên quá dài.'),
    sku: z
      .string()
      .trim()
      .min(1, 'Vui lòng nhập SKU.')
      .regex(/^[A-Za-z0-9-]{3,50}$/, 'SKU gồm 3-50 ký tự chữ, số hoặc dấu gạch ngang.'),
    price: requiredMoney,
    compareAtPrice: optionalMoney,
    weightGrams: optionalInt('Khối lượng', 1),
    sortOrder: optionalInt('Thứ tự'),
  })
  .refine((v) => v.compareAtPrice === '' || Number(v.compareAtPrice) > Number(v.price), {
    path: ['compareAtPrice'],
    message: 'Giá gốc phải lớn hơn giá bán.',
  })
export type VariantValues = z.infer<typeof variantSchema>

export const emptyVariant: VariantValues = {
  name: '',
  sku: '',
  price: '',
  compareAtPrice: '',
  weightGrams: '',
  sortOrder: '0',
}

export const emptyProductDetails: ProductDetailsValues = {
  categoryId: '',
  name: '',
  slug: '',
  shortDescription: '',
  description: '',
  ingredients: '',
  origin: '',
  brand: '',
  shelfLifeDays: '',
}

const nullIfEmpty = (v: string): string | null => (v.trim() === '' ? null : v.trim())
const intOrNull = (v: string): number | null => (v.trim() === '' ? null : Number(v))
const vnd = (amount: number): Schemas['MoneyRequest'] => ({ amount, currency: 'VND' })

export function toVariantRequest(v: VariantValues): Schemas['VariantRequest'] {
  return {
    name: nullIfEmpty(v.name),
    sku: v.sku.trim(),
    price: vnd(Number(v.price)),
    compareAtPrice: v.compareAtPrice.trim() === '' ? null : vnd(Number(v.compareAtPrice)),
    weightGrams: intOrNull(v.weightGrams),
    sortOrder: intOrNull(v.sortOrder) ?? 0,
  }
}

export function toProductRequest(v: ProductDetailsValues): Schemas['UpdateProductRequest'] {
  return {
    categoryId: v.categoryId,
    name: v.name.trim(),
    slug: nullIfEmpty(v.slug),
    shortDescription: nullIfEmpty(v.shortDescription),
    description: nullIfEmpty(v.description),
    ingredients: nullIfEmpty(v.ingredients),
    origin: nullIfEmpty(v.origin),
    brand: nullIfEmpty(v.brand),
    shelfLifeDays: intOrNull(v.shelfLifeDays),
  }
}

export function fromProduct(p: Schemas['AdminProductDetailResponse']): ProductDetailsValues {
  return {
    categoryId: p.categoryId,
    name: p.name,
    slug: p.slug,
    shortDescription: p.shortDescription ?? '',
    description: p.description ?? '',
    ingredients: p.ingredients ?? '',
    origin: p.origin ?? '',
    brand: p.brand ?? '',
    shelfLifeDays: p.shelfLifeDays?.toString() ?? '',
  }
}

export function fromVariant(v: Schemas['AdminVariantResponse']): VariantValues {
  return {
    name: v.name,
    sku: v.sku,
    price: String(Math.round(v.price.amount)),
    compareAtPrice: v.compareAtPrice ? String(Math.round(v.compareAtPrice.amount)) : '',
    weightGrams: v.weightGrams?.toString() ?? '',
    sortOrder: String(v.sortOrder),
  }
}

export const PRODUCT_FIELDS = Object.keys(emptyProductDetails)
export const VARIANT_FIELDS = Object.keys(emptyVariant)
