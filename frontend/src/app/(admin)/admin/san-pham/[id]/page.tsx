'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useParams, useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { ExternalLink, Pencil, Plus, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { AdminOnly } from '@/components/admin/admin-only'
import { ConfirmDialog } from '@/components/admin/confirm-dialog'
import { PageHeader } from '@/components/admin/page-header'
import { ProductDetailsFields } from '@/components/admin/product-fields'
import { ProductImages } from '@/components/admin/product-images'
import { VariantDialog, type VariantSubmit } from '@/components/admin/variant-dialog'
import { Badge } from '@/components/ui/badge'
import { Button, buttonVariants } from '@/components/ui/button'
import { Card, CardTitle } from '@/components/ui/card'
import { Alert, ErrorState, Spinner } from '@/components/ui/feedback'
import { Checkbox } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import {
  useAdminCategoriesQuery,
  useAdminProductQuery,
  useProductMutations,
} from '@/features/admin/catalog/queries'
import {
  PRODUCT_FIELDS,
  fromProduct,
  productDetailsSchema,
  toProductRequest,
  type ProductDetailsValues,
} from '@/features/admin/catalog/schemas'
import type { Schemas } from '@/lib/api/types'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'
import { formatDateTime, formatNumber, formatVnd } from '@/lib/format'

type Product = Schemas['AdminProductDetailResponse']
type Mutations = ReturnType<typeof useProductMutations>

function DetailsForm({
  product,
  categories,
  mutations,
}: {
  product: Product
  categories: Schemas['AdminCategoryResponse'][]
  mutations: Mutations
}) {
  const [formError, setFormError] = useState<string | null>(null)
  const form = useForm<ProductDetailsValues>({
    resolver: zodResolver(productDetailsSchema),
    defaultValues: fromProduct(product),
  })
  const onSubmit = form.handleSubmit((values) => {
    setFormError(null)
    mutations.update.mutate(toProductRequest(values), {
      onSuccess: () => {
        toast.success('Đã lưu thông tin sản phẩm')
        form.reset(values)
      },
      onError: (e) => {
        const rest = applyFieldErrors(e, form.setError, PRODUCT_FIELDS)
        setFormError([getErrorMessage(e), ...rest].join(' '))
      },
    })
  })
  return (
    <Card>
      <form onSubmit={onSubmit} noValidate className="grid gap-4">
        <CardTitle>Thông tin sản phẩm</CardTitle>
        <ProductDetailsFields form={form} categories={categories} />
        {formError ? <Alert variant="error">{formError}</Alert> : null}
        <Button
          type="submit"
          className="w-fit"
          loading={mutations.update.isPending}
          disabled={!form.formState.isDirty}
        >
          Lưu thay đổi
        </Button>
      </form>
    </Card>
  )
}

function Variants({ product, mutations }: { product: Product; mutations: Mutations }) {
  const [editing, setEditing] = useState<Schemas['AdminVariantResponse'] | null>(null)
  const [open, setOpen] = useState(false)
  const [dialogKey, setDialogKey] = useState(0)
  const [removing, setRemoving] = useState<Schemas['AdminVariantResponse'] | null>(null)

  const openDialog = (variant: Schemas['AdminVariantResponse'] | null) => {
    setEditing(variant)
    setDialogKey((k) => k + 1)
    setOpen(true)
  }

  const submit = async (value: VariantSubmit) => {
    if (value.mode === 'add') await mutations.addVariant.mutateAsync(value.body)
    else await mutations.updateVariant.mutateAsync({ variantId: value.variantId, body: value.body })
    toast.success(value.mode === 'add' ? 'Đã thêm phân loại' : 'Đã lưu phân loại')
  }

  const sorted = [...product.variants].sort((a, b) => a.sortOrder - b.sortOrder)

  return (
    <Card>
      <div className="flex items-center justify-between gap-2">
        <CardTitle>Phân loại</CardTitle>
        <Button size="sm" onClick={() => openDialog(null)}>
          <Plus aria-hidden />
          Thêm phân loại
        </Button>
      </div>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Phân loại</TableHead>
            <TableHead className="text-right">Giá</TableHead>
            <TableHead className="text-right">Tồn / giữ</TableHead>
            <TableHead>Trạng thái</TableHead>
            <TableHead>
              <span className="sr-only">Thao tác</span>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {sorted.map((v) => (
            <TableRow key={v.id}>
              <TableCell>
                <p className="font-medium">{v.name || '—'}</p>
                <p className="font-mono text-xs text-muted-foreground">{v.sku}</p>
              </TableCell>
              <TableCell className="text-right tabular-nums">
                {formatVnd(v.price)}
                {v.compareAtPrice ? (
                  <p className="text-xs text-muted-foreground line-through">{formatVnd(v.compareAtPrice)}</p>
                ) : null}
              </TableCell>
              <TableCell className="text-right tabular-nums">
                {formatNumber(v.quantityOnHand)} / {formatNumber(v.quantityReserved)}
              </TableCell>
              <TableCell>
                <Badge variant={v.isActive ? 'success' : 'muted'}>{v.isActive ? 'Đang bán' : 'Đã tắt'}</Badge>
              </TableCell>
              <TableCell>
                <div className="flex justify-end gap-1">
                  <Button
                    variant="outline"
                    size="icon-sm"
                    aria-label={`Sửa ${v.sku}`}
                    onClick={() => openDialog(v)}
                  >
                    <Pencil aria-hidden />
                  </Button>
                  <Button
                    variant="outline"
                    size="icon-sm"
                    aria-label={`Xóa ${v.sku}`}
                    onClick={() => setRemoving(v)}
                  >
                    <Trash2 aria-hidden />
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <p className="text-xs text-muted-foreground">
        Nhập/điều chỉnh tồn kho tại{' '}
        <Link
          href={`/admin/kho?search=${encodeURIComponent(product.name)}`}
          className="text-primary hover:underline"
        >
          trang Kho
        </Link>
        .
      </p>
      <VariantDialog key={dialogKey} open={open} onOpenChange={setOpen} variant={editing} onSubmit={submit} />
      <ConfirmDialog
        open={removing !== null}
        onOpenChange={(o) => (o ? undefined : setRemoving(null))}
        title={`Xóa phân loại ${removing?.sku ?? ''}?`}
        description="Phân loại sẽ không còn bán. Đơn hàng cũ vẫn giữ thông tin."
        confirmLabel="Xóa"
        destructive
        loading={mutations.removeVariant.isPending}
        onConfirm={() =>
          removing &&
          mutations.removeVariant.mutate(removing.id, {
            onSuccess: () => {
              toast.success('Đã xóa phân loại')
              setRemoving(null)
            },
            onError: (e) => toast.error(getErrorMessage(e)),
          })
        }
      />
    </Card>
  )
}

function Visibility({ product, mutations }: { product: Product; mutations: Mutations }) {
  const set = (body: Schemas['ProductVisibilityRequest']) =>
    mutations.visibility.mutate(body, {
      onSuccess: () => toast.success('Đã cập nhật hiển thị'),
      onError: (e) => toast.error(getErrorMessage(e)),
    })
  return (
    <Card>
      <CardTitle>Hiển thị</CardTitle>
      <Label htmlFor="vis-active" className="font-normal">
        <Checkbox
          id="vis-active"
          checked={product.isActive}
          disabled={mutations.visibility.isPending}
          onChange={(e) => set({ isActive: e.target.checked, isFeatured: product.isFeatured })}
        />
        Đang bán (hiện trên cửa hàng)
      </Label>
      <Label htmlFor="vis-featured" className="font-normal">
        <Checkbox
          id="vis-featured"
          checked={product.isFeatured}
          disabled={mutations.visibility.isPending}
          onChange={(e) => set({ isActive: product.isActive, isFeatured: e.target.checked })}
        />
        Sản phẩm nổi bật (trang chủ)
      </Label>
      <dl className="grid gap-1 text-sm text-muted-foreground">
        <div className="flex justify-between">
          <dt>Đã bán</dt>
          <dd className="tabular-nums">{formatNumber(product.soldCount)}</dd>
        </div>
        <div className="flex justify-between">
          <dt>Tạo lúc</dt>
          <dd>{formatDateTime(product.createdAt)}</dd>
        </div>
        <div className="flex justify-between">
          <dt>Cập nhật</dt>
          <dd>{formatDateTime(product.updatedAt)}</dd>
        </div>
      </dl>
    </Card>
  )
}

function ProductEditor({ id }: { id: string }) {
  const router = useRouter()
  const product = useAdminProductQuery(id)
  const categories = useAdminCategoriesQuery()
  const mutations = useProductMutations(id)
  const [confirmDelete, setConfirmDelete] = useState(false)

  if (product.isPending || categories.isPending) return <Spinner />
  if (product.isError)
    return <ErrorState message={getErrorMessage(product.error)} onRetry={() => void product.refetch()} />
  if (categories.isError)
    return (
      <ErrorState message={getErrorMessage(categories.error)} onRetry={() => void categories.refetch()} />
    )
  const p = product.data

  return (
    <>
      <PageHeader
        backHref="/admin/san-pham"
        backLabel="Danh sách sản phẩm"
        title={p.name}
        description={
          <span className="flex flex-wrap items-center gap-2">
            <Badge variant={p.isActive ? 'success' : 'muted'}>{p.isActive ? 'Đang bán' : 'Đang ẩn'}</Badge>
            {p.isFeatured ? <Badge variant="info">Nổi bật</Badge> : null}
            <span className="font-mono text-xs">/{p.slug}</span>
          </span>
        }
        actions={
          <>
            {p.isActive ? (
              <a
                href={`/san-pham/${p.slug}`}
                target="_blank"
                rel="noreferrer"
                className={buttonVariants({ variant: 'outline' })}
              >
                <ExternalLink aria-hidden />
                Xem trên cửa hàng
              </a>
            ) : null}
            <Button variant="destructive" onClick={() => setConfirmDelete(true)}>
              <Trash2 aria-hidden />
              Xóa sản phẩm
            </Button>
          </>
        }
      />
      <div className="grid gap-6 xl:grid-cols-3">
        <div className="grid content-start gap-6 xl:col-span-2">
          <DetailsForm
            key={p.updatedAt ?? p.createdAt}
            product={p}
            categories={categories.data}
            mutations={mutations}
          />
          <Variants product={p} mutations={mutations} />
          <ProductImages productName={p.name} images={p.images} mutations={mutations} />
        </div>
        <div className="grid content-start gap-6">
          <Visibility product={p} mutations={mutations} />
        </div>
      </div>
      <ConfirmDialog
        open={confirmDelete}
        onOpenChange={setConfirmDelete}
        title={`Xóa sản phẩm “${p.name}”?`}
        description="Sản phẩm sẽ bị gỡ khỏi cửa hàng. Đơn hàng cũ vẫn giữ thông tin."
        confirmLabel="Xóa sản phẩm"
        destructive
        loading={mutations.remove.isPending}
        onConfirm={() =>
          mutations.remove.mutate(undefined, {
            onSuccess: () => {
              toast.success('Đã xóa sản phẩm')
              router.replace('/admin/san-pham')
            },
            onError: (e) => toast.error(getErrorMessage(e)),
          })
        }
      />
    </>
  )
}

export default function EditProductPage() {
  const { id } = useParams<{ id: string }>()
  return (
    <AdminOnly>
      <ProductEditor id={id} />
    </AdminOnly>
  )
}
