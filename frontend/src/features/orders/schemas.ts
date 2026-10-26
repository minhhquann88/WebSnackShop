import { z } from 'zod'

export const PHONE_PATTERN = /^0\d{9}$/

export const phoneSchema = z
  .string()
  .trim()
  .regex(PHONE_PATTERN, 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.')

const required = (message: string, max = 200) =>
  z.string().trim().min(1, message).max(max, 'Nội dung quá dài.')

export const addressSchema = z.object({
  fullName: required('Vui lòng nhập họ tên người nhận.', 100),
  phone: phoneSchema,
  line1: required('Vui lòng nhập số nhà, tên đường.'),
  ward: required('Vui lòng nhập phường/xã.', 100),
  district: required('Vui lòng nhập quận/huyện.', 100),
  province: required('Vui lòng nhập tỉnh/thành phố.', 100),
})

export type AddressFormValues = z.infer<typeof addressSchema>

export const emptyAddress: AddressFormValues = {
  fullName: '',
  phone: '',
  line1: '',
  ward: '',
  district: '',
  province: '',
}

export const ADDRESS_FIELDS = ['fullName', 'phone', 'line1', 'ward', 'district', 'province'] as const

export const checkoutSchema = z.object({
  contactEmail: z.string().trim().min(1, 'Vui lòng nhập email.').email('Email không hợp lệ.'),
  shippingAddress: addressSchema,
  paymentMethod: z.enum(['Cod', 'BankTransfer']),
  customerNote: z.string().trim().max(500, 'Ghi chú tối đa 500 ký tự.'),
})

export type CheckoutFormValues = z.infer<typeof checkoutSchema>

export const CHECKOUT_FIELDS = [
  'contactEmail',
  'paymentMethod',
  'customerNote',
  ...ADDRESS_FIELDS.map((f) => `shippingAddress.${f}`),
] as const

export const trackSchema = z.object({
  orderNumber: z.string().trim().min(1, 'Vui lòng nhập mã đơn hàng.').max(40),
  phone: phoneSchema,
})

export type TrackFormValues = z.infer<typeof trackSchema>
