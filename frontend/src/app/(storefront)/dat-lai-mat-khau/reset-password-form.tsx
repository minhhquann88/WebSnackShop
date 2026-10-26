'use client'

import Link from 'next/link'
import { useSearchParams } from 'next/navigation'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button, buttonVariants } from '@/components/ui/button'
import { Alert } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'
import { AuthCard } from '@/components/storefront/auth-card'
import { resetPassword } from '@/features/account/api'
import { resetPasswordSchema, type ResetPasswordFormValues } from '@/features/auth/schemas'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'

export function ResetPasswordForm() {
  const searchParams = useSearchParams()
  const email = searchParams.get('email') ?? ''
  const token = searchParams.get('token') ?? ''
  const [done, setDone] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: '', confirmPassword: '' },
  })

  const onSubmit = form.handleSubmit(async ({ newPassword }) => {
    setError(null)
    try {
      await resetPassword({ email, token, newPassword })
      setDone(true)
    } catch (e) {
      applyFieldErrors(e, form.setError, ['newPassword'])
      setError(getErrorMessage(e))
    }
  })

  if (!email || !token) {
    return (
      <AuthCard title="Đặt lại mật khẩu">
        <Alert variant="error">
          Liên kết không hợp lệ. Vui lòng mở lại liên kết trong email hoặc yêu cầu liên kết mới.
        </Alert>
        <Link href="/quen-mat-khau" className={buttonVariants()}>
          Yêu cầu liên kết mới
        </Link>
      </AuthCard>
    )
  }

  return (
    <AuthCard title="Đặt lại mật khẩu" description={`Tạo mật khẩu mới cho ${email}.`}>
      {done ? (
        <>
          <Alert variant="success" title="Đã đặt lại mật khẩu">
            Bạn có thể đăng nhập bằng mật khẩu mới.
          </Alert>
          <Link href="/dang-nhap" className={buttonVariants({ size: 'lg' })}>
            Đăng nhập
          </Link>
        </>
      ) : (
        <form onSubmit={onSubmit} noValidate className="grid gap-4">
          {error ? <Alert variant="error">{error}</Alert> : null}
          <FormField
            id="newPassword"
            label="Mật khẩu mới"
            error={form.formState.errors.newPassword?.message}
            hint="Tối thiểu 8 ký tự, gồm chữ và số."
          >
            <Input type="password" autoComplete="new-password" {...form.register('newPassword')} />
          </FormField>
          <FormField
            id="confirmPassword"
            label="Nhập lại mật khẩu mới"
            error={form.formState.errors.confirmPassword?.message}
          >
            <Input type="password" autoComplete="new-password" {...form.register('confirmPassword')} />
          </FormField>
          <Button type="submit" size="lg" loading={form.formState.isSubmitting}>
            Đặt lại mật khẩu
          </Button>
        </form>
      )}
    </AuthCard>
  )
}
