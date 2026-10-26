'use client'

import Link from 'next/link'
import { useSearchParams } from 'next/navigation'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { Alert } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'
import { AuthCard } from '@/components/storefront/auth-card'
import { GoogleSignInButton } from '@/features/auth/google-sign-in-button'
import { loginSchema, type LoginFormValues } from '@/features/auth/schemas'
import { useAuthRedirect } from '@/features/auth/use-auth-redirect'
import { loginWithGoogle, loginWithPassword } from '@/lib/auth/session'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { googleClientId } from '@/lib/env'

export function LoginForm() {
  const redirect = useAuthRedirect()
  const returnUrl = useSearchParams().get('returnUrl')
  const [error, setError] = useState<string | null>(null)
  const [googleBusy, setGoogleBusy] = useState(false)
  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })
  const { errors, isSubmitting } = form.formState

  const onSubmit = form.handleSubmit(async (values) => {
    setError(null)
    try {
      const session = await loginWithPassword(values.email, values.password)
      redirect(session.user)
    } catch (e) {
      applyFieldErrors(e, form.setError, ['email', 'password'])
      setError(getErrorMessage(e))
    }
  })

  async function onGoogle(idToken: string) {
    setError(null)
    setGoogleBusy(true)
    try {
      const session = await loginWithGoogle(idToken)
      redirect(session.user)
    } catch (e) {
      setError(getErrorMessage(e))
    } finally {
      setGoogleBusy(false)
    }
  }

  const registerHref = returnUrl ? `/dang-ky?returnUrl=${encodeURIComponent(returnUrl)}` : '/dang-ky'

  return (
    <AuthCard
      title="Đăng nhập"
      description="Đăng nhập để theo dõi đơn hàng và lưu địa chỉ giao hàng."
      footer={
        <>
          Chưa có tài khoản?{' '}
          <Link href={registerHref} className="font-medium text-primary hover:underline">
            Đăng ký ngay
          </Link>
        </>
      }
    >
      <form onSubmit={onSubmit} noValidate className="grid gap-4">
        {error ? <Alert variant="error">{error}</Alert> : null}
        <FormField id="email" label="Email" error={errors.email?.message}>
          <Input type="email" autoComplete="email" {...form.register('email')} />
        </FormField>
        <FormField id="password" label="Mật khẩu" error={errors.password?.message}>
          <Input type="password" autoComplete="current-password" {...form.register('password')} />
        </FormField>
        <div className="flex justify-end">
          <Link href="/quen-mat-khau" className="text-sm text-primary hover:underline">
            Quên mật khẩu?
          </Link>
        </div>
        <Button type="submit" size="lg" loading={isSubmitting}>
          Đăng nhập
        </Button>
      </form>
      {googleClientId ? (
        <div className="grid gap-3">
          <div className="flex items-center gap-3 text-xs text-muted-foreground">
            <span className="h-px flex-1 bg-border" />
            hoặc
            <span className="h-px flex-1 bg-border" />
          </div>
          <div aria-busy={googleBusy}>
            <GoogleSignInButton onCredential={onGoogle} />
          </div>
        </div>
      ) : null}
    </AuthCard>
  )
}
