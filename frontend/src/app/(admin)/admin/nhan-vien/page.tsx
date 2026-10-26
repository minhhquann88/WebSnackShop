'use client'

import { Suspense, useState, type FormEvent } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, Search } from 'lucide-react'
import { toast } from 'sonner'
import { AdminOnly } from '@/components/admin/admin-only'
import { PageHeader } from '@/components/admin/page-header'
import { useUrlFilters } from '@/components/admin/use-url-filters'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Alert, EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Checkbox, Input, NativeSelect } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Pagination, totalPagesOf } from '@/components/ui/pagination'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useUserMutations, useUsersQuery } from '@/features/admin/people/queries'
import { useAuthStore } from '@/lib/auth/auth.store'
import { ROLES, type Schemas } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { formatDate, formatNumber } from '@/lib/format'
import { roleLabels } from '@/lib/labels'

type AdminUser = Schemas['AdminUserResponse']
const ALL_ROLES = [ROLES.Customer, ROLES.Staff, ROLES.Admin]
const PAGE_SIZE = 20

const staffSchema = z.object({
  email: z.email('Email không hợp lệ.'),
  fullName: z.string().trim().min(1, 'Vui lòng nhập họ tên.').max(100, 'Họ tên quá dài.'),
  password: z
    .string()
    .min(8, 'Mật khẩu tối thiểu 8 ký tự.')
    .max(100, 'Mật khẩu quá dài.')
    .regex(/[A-Za-z]/, 'Mật khẩu phải có chữ cái.')
    .regex(/[0-9]/, 'Mật khẩu phải có chữ số.'),
  isAdmin: z.boolean(),
})
type StaffValues = z.infer<typeof staffSchema>

function CreateStaffDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (o: boolean) => void }) {
  const { create } = useUserMutations()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<StaffValues>({
    resolver: zodResolver(staffSchema),
    defaultValues: { email: '', fullName: '', password: '', isAdmin: false },
  })
  const submit = form.handleSubmit(async (values) => {
    setError(null)
    try {
      await create.mutateAsync({ ...values, email: values.email.trim() })
      toast.success('Đã tạo tài khoản nhân viên')
      onOpenChange(false)
    } catch (e) {
      const rest = applyFieldErrors(e, form.setError, ['email', 'fullName', 'password'])
      setError([getErrorMessage(e), ...rest].join(' '))
    }
  })
  const err = form.formState.errors
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={submit} noValidate className="grid gap-4">
          <DialogHeader>
            <DialogTitle>Thêm nhân viên</DialogTitle>
            <DialogDescription>Nhân viên đăng nhập bằng email và mật khẩu này.</DialogDescription>
          </DialogHeader>
          <FormField id="staff-email" label="Email" required error={err.email?.message}>
            <Input type="email" autoComplete="off" {...form.register('email')} />
          </FormField>
          <FormField id="staff-name" label="Họ tên" required error={err.fullName?.message}>
            <Input {...form.register('fullName')} />
          </FormField>
          <FormField
            id="staff-password"
            label="Mật khẩu"
            required
            error={err.password?.message}
            hint="Tối thiểu 8 ký tự, gồm chữ và số."
          >
            <Input type="password" autoComplete="new-password" {...form.register('password')} />
          </FormField>
          <Label htmlFor="staff-admin" className="font-normal">
            <Checkbox id="staff-admin" {...form.register('isAdmin')} />
            Cấp quyền Admin (quản lý sản phẩm, nhân viên, cài đặt)
          </Label>
          {error ? <Alert variant="error">{error}</Alert> : null}
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Đóng
            </Button>
            <Button type="submit" loading={form.formState.isSubmitting}>
              Tạo tài khoản
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function RolesDialog({ user, onOpenChange }: { user: AdminUser; onOpenChange: (o: boolean) => void }) {
  const { setRoles } = useUserMutations()
  const [roles, setRolesState] = useState<string[]>(user.roles)
  const [error, setError] = useState<string | null>(null)
  const toggle = (role: string, on: boolean) =>
    setRolesState((r) => (on ? [...new Set([...r, role])] : r.filter((x) => x !== role)))
  const save = () => {
    setError(null)
    setRoles.mutate(
      { id: user.id, roles },
      {
        onSuccess: () => {
          toast.success('Đã cập nhật quyền')
          onOpenChange(false)
        },
        onError: (e) => setError(getErrorMessage(e)),
      },
    )
  }
  return (
    <Dialog open onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Phân quyền {user.fullName}</DialogTitle>
          <DialogDescription>{user.email}</DialogDescription>
        </DialogHeader>
        <fieldset className="grid gap-3">
          <legend className="sr-only">Vai trò</legend>
          {ALL_ROLES.map((role) => (
            <Label key={role} htmlFor={`role-${role}`} className="font-normal">
              <Checkbox
                id={`role-${role}`}
                checked={roles.includes(role)}
                onChange={(e) => toggle(role, e.target.checked)}
              />
              {roleLabels[role]}
            </Label>
          ))}
        </fieldset>
        {error ? <Alert variant="error">{error}</Alert> : null}
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Đóng
          </Button>
          <Button onClick={save} loading={setRoles.isPending} disabled={roles.length === 0}>
            Lưu
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

function Users() {
  const me = useAuthStore((s) => s.user)
  const { get, set, page } = useUrlFilters()
  const role = get('role') || 'Staff'
  const search = get('search')
  const [searchInput, setSearchInput] = useState(search)
  const [creating, setCreating] = useState(false)
  const [createKey, setCreateKey] = useState(0)
  const [editingRoles, setEditingRoles] = useState<AdminUser | null>(null)
  const { setLock } = useUserMutations()

  const query = useUsersQuery({
    role: role === 'all' ? undefined : role,
    search: search || undefined,
    page,
    pageSize: PAGE_SIZE,
  })

  const onSearch = (e: FormEvent) => {
    e.preventDefault()
    set({ search: searchInput.trim() })
  }

  const toggleLock = (u: AdminUser) => {
    if (
      !window.confirm(
        u.isLocked
          ? `Mở khóa tài khoản ${u.email}?`
          : `Khóa tài khoản ${u.email}? Người này sẽ bị đăng xuất.`,
      )
    )
      return
    setLock.mutate(
      { id: u.id, locked: !u.isLocked },
      {
        onSuccess: () => toast.success(u.isLocked ? 'Đã mở khóa' : 'Đã khóa tài khoản'),
        onError: (e) => toast.error(getErrorMessage(e)),
      },
    )
  }

  return (
    <>
      <PageHeader
        title="Nhân viên & tài khoản"
        description={query.data ? `${formatNumber(query.data.totalCount)} tài khoản` : undefined}
        actions={
          <Button
            onClick={() => {
              setCreateKey((k) => k + 1)
              setCreating(true)
            }}
          >
            <Plus aria-hidden />
            Thêm nhân viên
          </Button>
        }
      />
      <div className="mb-4 grid gap-3 rounded-xl border bg-card p-4 sm:grid-cols-3">
        <form onSubmit={onSearch} className="grid gap-1.5 sm:col-span-2">
          <Label htmlFor="user-search">Tìm kiếm</Label>
          <div className="flex gap-2">
            <Input
              id="user-search"
              placeholder="Tên hoặc email"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
            <Button type="submit" variant="outline" size="icon" aria-label="Tìm">
              <Search aria-hidden />
            </Button>
          </div>
        </form>
        <div className="grid gap-1.5">
          <Label htmlFor="user-role">Vai trò</Label>
          <NativeSelect id="user-role" value={role} onChange={(e) => set({ role: e.target.value })}>
            <option value="Staff">Nhân viên</option>
            <option value="Admin">Quản trị</option>
            <option value="Customer">Khách hàng</option>
            <option value="all">Tất cả</option>
          </NativeSelect>
        </div>
      </div>

      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : query.data.items.length === 0 ? (
        <EmptyState title="Không có tài khoản" />
      ) : (
        <div className="grid gap-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Tài khoản</TableHead>
                <TableHead>Vai trò</TableHead>
                <TableHead>Trạng thái</TableHead>
                <TableHead>Tạo lúc</TableHead>
                <TableHead>
                  <span className="sr-only">Thao tác</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data.items.map((u) => {
                const isMe = u.id === me?.id
                return (
                  <TableRow key={u.id}>
                    <TableCell>
                      <p className="font-medium">
                        {u.fullName}
                        {isMe ? <span className="text-xs text-muted-foreground"> (bạn)</span> : null}
                      </p>
                      <p className="text-xs break-all text-muted-foreground">{u.email}</p>
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-1">
                        {u.roles.map((r) => (
                          <Badge
                            key={r}
                            variant={r === 'Admin' ? 'default' : r === 'Staff' ? 'info' : 'muted'}
                          >
                            {roleLabels[r] ?? r}
                          </Badge>
                        ))}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-1">
                        {u.isLocked ? (
                          <Badge variant="destructive">Đã khóa</Badge>
                        ) : (
                          <Badge variant="success">Hoạt động</Badge>
                        )}
                        {!u.emailConfirmed ? <Badge variant="muted">Chưa xác nhận email</Badge> : null}
                      </div>
                    </TableCell>
                    <TableCell>{formatDate(u.createdAt)}</TableCell>
                    <TableCell>
                      <div className="flex justify-end gap-1">
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={isMe}
                          onClick={() => setEditingRoles(u)}
                        >
                          Phân quyền
                        </Button>
                        <Button
                          size="sm"
                          variant={u.isLocked ? 'outline' : 'destructive'}
                          disabled={isMe || setLock.isPending}
                          onClick={() => toggleLock(u)}
                        >
                          {u.isLocked ? 'Mở khóa' : 'Khóa'}
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
          <Pagination
            page={page}
            totalPages={totalPagesOf(query.data)}
            onPageChange={(p) => set({ page: p })}
          />
        </div>
      )}
      <CreateStaffDialog key={createKey} open={creating} onOpenChange={setCreating} />
      {editingRoles ? (
        <RolesDialog
          key={editingRoles.id}
          user={editingRoles}
          onOpenChange={(o) => (o ? undefined : setEditingRoles(null))}
        />
      ) : null}
    </>
  )
}

export default function AdminUsersPage() {
  return (
    <AdminOnly>
      <Suspense fallback={<Spinner />}>
        <Users />
      </Suspense>
    </AdminOnly>
  )
}
