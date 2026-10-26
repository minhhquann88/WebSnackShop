'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { ArrowDown, ArrowUp, Pencil, Plus, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { AdminOnly } from '@/components/admin/admin-only'
import { ConfirmDialog } from '@/components/admin/confirm-dialog'
import { PageHeader } from '@/components/admin/page-header'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Alert, EmptyState, ErrorState, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Checkbox, Input, Textarea } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminCategoriesQuery, useCategoryMutations } from '@/features/admin/catalog/queries'
import { slugField } from '@/features/admin/catalog/schemas'
import type { Schemas } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { formatNumber } from '@/lib/format'

type Category = Schemas['AdminCategoryResponse']

const schema = z.object({
  name: z.string().trim().min(1, 'Vui lòng nhập tên danh mục.').max(100, 'Tên quá dài.'),
  slug: slugField,
  description: z.string().trim().max(500, 'Mô tả quá dài.'),
  imageUrl: z
    .string()
    .trim()
    .refine((v) => v === '' || /^https?:\/\/\S+$/.test(v), 'URL ảnh không hợp lệ.'),
  isActive: z.boolean(),
})
type Values = z.infer<typeof schema>
const FIELDS = ['name', 'slug', 'description', 'imageUrl']

function CategoryDialog({
  category,
  nextSortOrder,
  open,
  onOpenChange,
}: {
  category: Category | null
  nextSortOrder: number
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const { create, update } = useCategoryMutations()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: category?.name ?? '',
      slug: category?.slug ?? '',
      description: category?.description ?? '',
      imageUrl: category?.imageUrl ?? '',
      isActive: category?.isActive ?? true,
    },
  })
  const submit = form.handleSubmit(async (v) => {
    setError(null)
    const base = {
      name: v.name,
      slug: v.slug || null,
      description: v.description || null,
      imageUrl: v.imageUrl || null,
    }
    try {
      if (category) {
        await update.mutateAsync({
          id: category.id,
          body: { ...base, sortOrder: category.sortOrder, isActive: v.isActive },
        })
      } else {
        await create.mutateAsync({ ...base, sortOrder: nextSortOrder })
      }
      toast.success(category ? 'Đã lưu danh mục' : 'Đã tạo danh mục')
      onOpenChange(false)
    } catch (e) {
      const rest = applyFieldErrors(e, form.setError, FIELDS)
      setError([getErrorMessage(e), ...rest].join(' '))
    }
  })
  const err = form.formState.errors
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={submit} noValidate className="grid gap-4">
          <DialogHeader>
            <DialogTitle>{category ? `Sửa danh mục “${category.name}”` : 'Thêm danh mục'}</DialogTitle>
          </DialogHeader>
          <FormField id="cat-name" label="Tên danh mục" required error={err.name?.message}>
            <Input {...form.register('name')} />
          </FormField>
          <FormField
            id="cat-slug"
            label="Đường dẫn (slug)"
            error={err.slug?.message}
            hint="Để trống để tự tạo từ tên."
          >
            <Input {...form.register('slug')} />
          </FormField>
          <FormField id="cat-desc" label="Mô tả" error={err.description?.message}>
            <Textarea {...form.register('description')} />
          </FormField>
          <FormField id="cat-image" label="URL ảnh" error={err.imageUrl?.message}>
            <Input type="url" {...form.register('imageUrl')} />
          </FormField>
          {category ? (
            <Label htmlFor="cat-active" className="font-normal">
              <Checkbox id="cat-active" {...form.register('isActive')} />
              Đang hiển thị
            </Label>
          ) : null}
          {error ? <Alert variant="error">{error}</Alert> : null}
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Đóng
            </Button>
            <Button type="submit" loading={form.formState.isSubmitting}>
              Lưu
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function Categories() {
  const query = useAdminCategoriesQuery()
  const { remove, reorder } = useCategoryMutations()
  const [editing, setEditing] = useState<Category | null>(null)
  const [open, setOpen] = useState(false)
  const [dialogKey, setDialogKey] = useState(0)
  const [deleting, setDeleting] = useState<Category | null>(null)

  const openDialog = (c: Category | null) => {
    setEditing(c)
    setDialogKey((k) => k + 1)
    setOpen(true)
  }

  const sorted = [...(query.data ?? [])].sort((a, b) => a.sortOrder - b.sortOrder)

  const move = (index: number, delta: -1 | 1) => {
    const ids = sorted.map((c) => c.id)
    const target = index + delta
    if (target < 0 || target >= ids.length) return
    ;[ids[index], ids[target]] = [ids[target]!, ids[index]!]
    reorder.mutate(ids, { onError: (e) => toast.error(getErrorMessage(e)) })
  }

  return (
    <>
      <PageHeader
        title="Danh mục"
        actions={
          <Button onClick={() => openDialog(null)}>
            <Plus aria-hidden />
            Thêm danh mục
          </Button>
        }
      />
      {query.isPending ? (
        <Spinner />
      ) : query.isError ? (
        <ErrorState message={getErrorMessage(query.error)} onRetry={() => void query.refetch()} />
      ) : sorted.length === 0 ? (
        <EmptyState title="Chưa có danh mục" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Thứ tự</TableHead>
              <TableHead>Danh mục</TableHead>
              <TableHead className="text-right">Sản phẩm</TableHead>
              <TableHead>Trạng thái</TableHead>
              <TableHead>
                <span className="sr-only">Thao tác</span>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {sorted.map((c, index) => (
              <TableRow key={c.id}>
                <TableCell>
                  <div className="flex gap-1">
                    <Button
                      variant="outline"
                      size="icon-sm"
                      aria-label={`Đưa ${c.name} lên`}
                      disabled={reorder.isPending || index === 0}
                      onClick={() => move(index, -1)}
                    >
                      <ArrowUp aria-hidden />
                    </Button>
                    <Button
                      variant="outline"
                      size="icon-sm"
                      aria-label={`Đưa ${c.name} xuống`}
                      disabled={reorder.isPending || index === sorted.length - 1}
                      onClick={() => move(index, 1)}
                    >
                      <ArrowDown aria-hidden />
                    </Button>
                  </div>
                </TableCell>
                <TableCell>
                  <p className="font-medium">{c.name}</p>
                  <p className="font-mono text-xs text-muted-foreground">/{c.slug}</p>
                </TableCell>
                <TableCell className="text-right tabular-nums">{formatNumber(c.productCount)}</TableCell>
                <TableCell>
                  <Badge variant={c.isActive ? 'success' : 'muted'}>
                    {c.isActive ? 'Hiển thị' : 'Đang ẩn'}
                  </Badge>
                </TableCell>
                <TableCell>
                  <div className="flex justify-end gap-1">
                    <Button
                      variant="outline"
                      size="icon-sm"
                      aria-label={`Sửa ${c.name}`}
                      onClick={() => openDialog(c)}
                    >
                      <Pencil aria-hidden />
                    </Button>
                    <Button
                      variant="outline"
                      size="icon-sm"
                      aria-label={`Xóa ${c.name}`}
                      onClick={() => setDeleting(c)}
                    >
                      <Trash2 aria-hidden />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      <CategoryDialog
        key={dialogKey}
        category={editing}
        nextSortOrder={sorted.length === 0 ? 0 : Math.max(...sorted.map((c) => c.sortOrder)) + 1}
        open={open}
        onOpenChange={setOpen}
      />
      <ConfirmDialog
        open={deleting !== null}
        onOpenChange={(o) => (o ? undefined : setDeleting(null))}
        title={`Xóa danh mục “${deleting?.name ?? ''}”?`}
        description="Chỉ xóa được danh mục không còn sản phẩm."
        confirmLabel="Xóa"
        destructive
        loading={remove.isPending}
        onConfirm={() =>
          deleting &&
          remove.mutate(deleting.id, {
            onSuccess: () => {
              toast.success('Đã xóa danh mục')
              setDeleting(null)
            },
            onError: (e) => toast.error(getErrorMessage(e)),
          })
        }
      />
    </>
  )
}

export default function AdminCategoriesPage() {
  return (
    <AdminOnly>
      <Categories />
    </AdminOnly>
  )
}
