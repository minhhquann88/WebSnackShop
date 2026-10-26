'use client'

import Link from 'next/link'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { Alert } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'
import { AuthCard } from '@/components/storefront/auth-card'
import { forgotPassword } from '@/features/account/api'
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '@/features/auth/schemas'
import { getErrorMessage } from '@/lib/errors'

export default function ForgotPasswordPage() {
  const [sentTo, setSentTo] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const form = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: '' },
  })

  const onSubmit = form.handleSubmit(async ({ email }) => {
    setError(null)
    try {
      await forgotPassword(email)
      setSentTo(email)
    } catch (e) {
      setError(getErrorMessage(e))
    }
  })

  return (
    <AuthCard
      title="Quên mật khẩu"
      description="Nhập email đã đăng ký, chúng tôi sẽ gửi liên kết đặt lại mật khẩu."
      footer={
        <Link href="/dang-nhap" className="font-medium text-primary hover:underline">
          Quay lại đăng nhập
        </Link>
      }
    >
      {sentTo ? (
        <Alert variant="success" title="Đã gửi email">
          Nếu {sentTo} đã đăng ký tài khoản, bạn sẽ nhận được email hướng dẫn đặt lại mật khẩu trong ít phút.
        </Alert>
      ) : (
        <form onSubmit={onSubmit} noValidate className="grid gap-4">
          {error ? <Alert variant="error">{error}</Alert> : null}
          <FormField id="email" label="Email" error={form.formState.errors.email?.message}>
            <Input type="email" autoComplete="email" {...form.register('email')} />
          </FormField>
          <Button type="submit" size="lg" loading={form.formState.isSubmitting}>
            Gửi liên kết
          </Button>
        </form>
      )}
    </AuthCard>
  )
}
