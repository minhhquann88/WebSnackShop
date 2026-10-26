'use client'

import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'
import { useChangePasswordMutation } from '@/features/account/queries'
import { changePasswordSchema, type ChangePasswordFormValues } from '@/features/auth/schemas'
import { logout } from '@/lib/auth/session'
import { applyFieldErrors, getErrorMessage, isApiError } from '@/lib/errors'

export default function ChangePasswordPage() {
  const router = useRouter()
  const change = useChangePasswordMutation()
  const form = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  })
  const { errors, isSubmitting } = form.formState

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await change.mutateAsync({
        currentPassword: values.currentPassword || null,
        newPassword: values.newPassword,
      })
      // The API revokes every session after a password change: clear ours and sign in again.
      await logout().catch(() => undefined)
      toast.success('Đã đổi mật khẩu. Vui lòng đăng nhập lại.')
      router.replace('/dang-nhap?returnUrl=/tai-khoan')
    } catch (e) {
      if (isApiError(e, 'Auth.WrongPassword'))
        form.setError('currentPassword', { message: getErrorMessage(e) })
      else {
        applyFieldErrors(e, form.setError, ['currentPassword', 'newPassword'])
        toast.error(getErrorMessage(e))
      }
    }
  })

  return (
    <Card>
      <CardHeader>
        <CardTitle>Đổi mật khẩu</CardTitle>
        <CardDescription>
          Sau khi đổi mật khẩu, bạn sẽ được đăng xuất khỏi mọi thiết bị. Tài khoản đăng nhập bằng Google có
          thể để trống mật khẩu hiện tại để tạo mật khẩu mới.
        </CardDescription>
      </CardHeader>
      <form onSubmit={onSubmit} noValidate className="grid max-w-lg gap-4">
        <FormField id="currentPassword" label="Mật khẩu hiện tại" error={errors.currentPassword?.message}>
          <Input type="password" autoComplete="current-password" {...form.register('currentPassword')} />
        </FormField>
        <FormField
          id="newPassword"
          label="Mật khẩu mới"
          error={errors.newPassword?.message}
          hint="Tối thiểu 8 ký tự, gồm chữ và số."
          required
        >
          <Input type="password" autoComplete="new-password" {...form.register('newPassword')} />
        </FormField>
        <FormField
          id="confirmPassword"
          label="Nhập lại mật khẩu mới"
          error={errors.confirmPassword?.message}
          required
        >
          <Input type="password" autoComplete="new-password" {...form.register('confirmPassword')} />
        </FormField>
        <Button type="submit" loading={isSubmitting} className="w-fit">
          Đổi mật khẩu
        </Button>
      </form>
    </Card>
  )
}
