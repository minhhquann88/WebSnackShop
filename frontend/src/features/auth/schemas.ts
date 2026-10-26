import { z } from 'zod'
import { PHONE_PATTERN } from '@/features/orders/schemas'

export const passwordSchema = z
  .string()
  .min(8, 'Mật khẩu tối thiểu 8 ký tự.')
  .max(128, 'Mật khẩu tối đa 128 ký tự.')
  .regex(/\p{L}/u, 'Mật khẩu phải có ít nhất một chữ cái.')
  .regex(/\d/, 'Mật khẩu phải có ít nhất một chữ số.')

const email = z.string().trim().min(1, 'Vui lòng nhập email.').email('Email không hợp lệ.')

export const loginSchema = z.object({
  email,
  password: z.string().min(1, 'Vui lòng nhập mật khẩu.'),
})
export type LoginFormValues = z.infer<typeof loginSchema>

export const registerSchema = z
  .object({
    fullName: z.string().trim().min(1, 'Vui lòng nhập họ tên.').max(100, 'Họ tên quá dài.'),
    email,
    phoneNumber: z
      .string()
      .trim()
      .refine(
        (v) => v === '' || PHONE_PATTERN.test(v),
        'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.',
      ),
    password: passwordSchema,
    confirmPassword: z.string(),
  })
  .refine((v) => v.password === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Mật khẩu nhập lại không khớp.',
  })
export type RegisterFormValues = z.infer<typeof registerSchema>

export const forgotPasswordSchema = z.object({ email })
export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>

export const resetPasswordSchema = z
  .object({ newPassword: passwordSchema, confirmPassword: z.string() })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Mật khẩu nhập lại không khớp.',
  })
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>

export const changePasswordSchema = z
  .object({ currentPassword: z.string(), newPassword: passwordSchema, confirmPassword: z.string() })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Mật khẩu nhập lại không khớp.',
  })
export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>

export const profileSchema = z.object({
  fullName: z.string().trim().min(1, 'Vui lòng nhập họ tên.').max(100, 'Họ tên quá dài.'),
  phoneNumber: z
    .string()
    .trim()
    .refine((v) => v === '' || PHONE_PATTERN.test(v), 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.'),
})
export type ProfileFormValues = z.infer<typeof profileSchema>
