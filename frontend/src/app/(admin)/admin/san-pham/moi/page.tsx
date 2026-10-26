'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { AdminOnly } from '@/components/admin/admin-only'
import { PageHeader } from '@/components/admin/page-header'
import { ProductDetailsFields, VariantFields } from '@/components/admin/product-fields'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, ErrorState, Spinner } from '@/components/ui/feedback'
import { FormField } from '@/components/ui/form-field'
import { Checkbox, Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAdminCategoriesQuery, useCreateProductMutation } from '@/features/admin/catalog/queries'
import {
  PRODUCT_FIELDS,
  VARIANT_FIELDS,
  emptyProductDetails,
  emptyVariant,
  productDetailsSchema,
  toProductRequest,
  toVariantRequest,
  variantSchema,
} from '@/features/admin/catalog/schemas'
import { applyFieldErrors, getErrorMessage } from '@/lib/errors'

const createSchema = productDetailsSchema.extend({
  firstVariant: variantSchema,
  initialStock: z
    .string()
    .trim()
    .refine(
      (v) => v === '' || (/^\d+$/.test(v) && Number(v) <= 1_000_000),
      'Tồn ban đầu là số nguyên 0 - 1.000.000.',
    ),
  isActive: z.boolean(),
  isFeatured: z.boolean(),
})
type CreateValues = z.infer<typeof createSchema>

const KNOWN_FIELDS = [...PRODUCT_FIELDS, ...VARIANT_FIELDS.map((f) => `firstVariant.${f}`), 'initialStock']

function CreateProductForm() {
  const router = useRouter()
  const categories = useAdminCategoriesQuery()
  const create = useCreateProductMutation()
  const [formError, setFormError] = useState<string | null>(null)
  const form = useForm<CreateValues>({
    resolver: zodResolver(createSchema),
    defaultValues: {
      ...emptyProductDetails,
      firstVariant: emptyVariant,
      initialStock: '0',
      isActive: true,
      isFeatured: false,
    },
  })

  const onSubmit = form.handleSubmit((values) => {
    setFormError(null)
    create.mutate(
      {
        ...toProductRequest(values),
        isActive: values.isActive,
        isFeatured: values.isFeatured,
        firstVariant: toVariantRequest(values.firstVariant),
        initialStock: values.initialStock === '' ? 0 : Number(values.initialStock),
      },
      {
        onSuccess: (id) => {
          toast.success('Đã tạo sản phẩm')
          router.push(`/admin/san-pham/${id}`)
        },
        onError: (e) => {
          const rest = applyFieldErrors(e, form.setError, KNOWN_FIELDS, {
            'firstVariant.price.amount': 'firstVariant.price',
            'firstVariant.compareAtPrice.amount': 'firstVariant.compareAtPrice',
          })
          setFormError([getErrorMessage(e), ...rest].join(' '))
        },
      },
    )
  })

  if (categories.isPending) return <Spinner />
  if (categories.isError)
    return (
      <ErrorState message={getErrorMessage(categories.error)} onRetry={() => void categories.refetch()} />
    )

  return (
    <form onSubmit={onSubmit} noValidate className="grid gap-6 lg:grid-cols-3">
      <div className="grid content-start gap-6 lg:col-span-2">
        <Card>
          <CardTitle>Thông tin sản phẩm</CardTitle>
          <ProductDetailsFields form={form} categories={categories.data} />
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Phân loại đầu tiên</CardTitle>
            <CardDescription>Có thể thêm phân loại khác và ảnh sau khi tạo.</CardDescription>
          </CardHeader>
          <VariantFields form={form} prefix="firstVariant" idPrefix="first-variant" />
          <FormField
            id="initial-stock"
            label="Tồn kho ban đầu"
            error={form.formState.errors.initialStock?.message}
            className="sm:max-w-xs"
          >
            <Input inputMode="numeric" {...form.register('initialStock')} />
          </FormField>
        </Card>
      </div>
      <div className="grid content-start gap-6">
        <Card>
          <CardTitle>Hiển thị</CardTitle>
          <Label htmlFor="create-active" className="font-normal">
            <Checkbox id="create-active" {...form.register('isActive')} />
            Đang bán (hiện trên cửa hàng)
          </Label>
          <Label htmlFor="create-featured" className="font-normal">
            <Checkbox id="create-featured" {...form.register('isFeatured')} />
            Sản phẩm nổi bật (trang chủ)
          </Label>
        </Card>
        {formError ? <Alert variant="error">{formError}</Alert> : null}
        <Button type="submit" size="lg" loading={create.isPending}>
          Tạo sản phẩm
        </Button>
      </div>
    </form>
  )
}

export default function NewProductPage() {
  return (
    <AdminOnly>
      <PageHeader title="Thêm sản phẩm" backHref="/admin/san-pham" backLabel="Danh sách sản phẩm" />
      <CreateProductForm />
    </AdminOnly>
  )
}
