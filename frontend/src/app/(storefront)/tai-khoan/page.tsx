'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { ErrorState, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'
import { useMeQuery, useUpdateProfileMutation } from '@/features/account/queries'
import { profileSchema, type ProfileFormValues } from '@/features/auth/schemas'
import type { User } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { roleLabels } from '@/lib/labels'

export default function ProfilePage() {
  const me = useMeQuery()
  if (me.isPending) return <Spinner />
  if (me.isError) return <ErrorState message={getErrorMessage(me.error)} onRetry={() => me.refetch()} />
  return <ProfileForm user={me.data} />
}

function ProfileForm({ user }: { user: User }) {
  const update = useUpdateProfileMutation()
  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: { fullName: user.fullName, phoneNumber: user.phoneNumber ?? '' },
  })
  const { errors, isSubmitting, isDirty } = form.formState

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await update.mutateAsync({ fullName: values.fullName, phoneNumber: values.phoneNumber || null })
      form.reset(values)
      toast.success('Đã cập nhật hồ sơ')
    } catch (e) {
      applyFieldErrors(e, form.setError, ['fullName', 'phoneNumber'])
      toast.error(getErrorMessage(e))
    }
  })

  return (
    <Card>
      <CardHeader>
        <CardTitle>
          <span className="sr-only">Tài khoản - </span>Hồ sơ của tôi
        </CardTitle>
        <CardDescription>
          {user.email} · {user.roles.map((r) => roleLabels[r] ?? r).join(', ')}
        </CardDescription>
      </CardHeader>
      <form onSubmit={onSubmit} noValidate className="grid max-w-lg gap-4">
        <FormField id="fullName" label="Họ tên" error={errors.fullName?.message} required>
          <Input autoComplete="name" {...form.register('fullName')} />
        </FormField>
        <FormField id="phoneNumber" label="Số điện thoại" error={errors.phoneNumber?.message}>
          <Input type="tel" inputMode="numeric" autoComplete="tel" {...form.register('phoneNumber')} />
        </FormField>
        <FormField id="email" label="Email" hint="Email không thể thay đổi.">
          <Input type="email" value={user.email} readOnly disabled />
        </FormField>
        <Button type="submit" loading={isSubmitting} disabled={!isDirty} className="w-fit">
          Lưu thay đổi
        </Button>
      </form>
    </Card>
  )
}
