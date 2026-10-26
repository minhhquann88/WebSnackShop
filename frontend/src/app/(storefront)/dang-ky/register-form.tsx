'use client'

import Link from 'next/link'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Alert } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'
import { AuthCard } from '@/components/storefront/auth-card'
import { GoogleSignInButton } from '@/features/auth/google-sign-in-button'
import { registerSchema, type RegisterFormValues } from '@/features/auth/schemas'
import { useAuthRedirect } from '@/features/auth/use-auth-redirect'
import { loginWithGoogle, registerAccount } from '@/lib/auth/session'
import { applyFieldErrors, getErrorMessage, isApiError } from '@/lib/errors'

export function RegisterForm() {
  const redirect = useAuthRedirect()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { fullName: '', email: '', phoneNumber: '', password: '', confirmPassword: '' },
  })
  const { errors, isSubmitting } = form.formState

  const onSubmit = form.handleSubmit(async (values) => {
    setError(null)
    try {
      const session = await registerAccount({
        email: values.email,
        password: values.password,
        fullName: values.fullName,
        phoneNumber: values.phoneNumber || null,
      })
      toast.success('Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.')
      redirect(session.user)
    } catch (e) {
      if (isApiError(e, 'Auth.EmailTaken')) form.setError('email', { message: getErrorMessage(e) })
      applyFieldErrors(e, form.setError, ['fullName', 'email', 'phoneNumber', 'password'])
      setError(getErrorMessage(e))
    }
  })

  async function onGoogle(idToken: string) {
    try {
      const session = await loginWithGoogle(idToken)
      redirect(session.user)
    } catch (e) {
      setError(getErrorMessage(e))
    }
  }

  return (
    <AuthCard
      title="Tạo tài khoản"
      description="Lưu địa chỉ, xem lịch sử đơn hàng và thanh toán nhanh hơn."
      footer={
        <>
          Đã có tài khoản?{' '}
          <Link href="/dang-nhap" className="font-medium text-primary hover:underline">
            Đăng nhập
          </Link>
        </>
      }
    >
      <form onSubmit={onSubmit} noValidate className="grid gap-4">
        {error ? <Alert variant="error">{error}</Alert> : null}
        <FormField id="fullName" label="Họ tên" error={errors.fullName?.message} required>
          <Input autoComplete="name" {...form.register('fullName')} />
        </FormField>
        <FormField id="email" label="Email" error={errors.email?.message} required>
          <Input type="email" autoComplete="email" {...form.register('email')} />
        </FormField>
        <FormField
          id="phoneNumber"
          label="Số điện thoại"
          error={errors.phoneNumber?.message}
          hint="Không bắt buộc"
        >
          <Input type="tel" inputMode="numeric" autoComplete="tel" {...form.register('phoneNumber')} />
        </FormField>
        <FormField
          id="password"
          label="Mật khẩu"
          error={errors.password?.message}
          hint="Tối thiểu 8 ký tự, gồm chữ và số."
          required
        >
          <Input type="password" autoComplete="new-password" {...form.register('password')} />
        </FormField>
        <FormField
          id="confirmPassword"
          label="Nhập lại mật khẩu"
          error={errors.confirmPassword?.message}
          required
        >
          <Input type="password" autoComplete="new-password" {...form.register('confirmPassword')} />
        </FormField>
        <Button type="submit" size="lg" loading={isSubmitting}>
          Đăng ký
        </Button>
      </form>
      <GoogleSignInButton onCredential={onGoogle} />
    </AuthCard>
  )
}
