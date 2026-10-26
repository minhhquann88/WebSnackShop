'use client'

import { get, type FieldValues, type Path, type UseFormReturn } from 'react-hook-form'
import { FormField } from '@/components/ui/form-field'
import { Input, NativeSelect, Textarea } from '@/components/ui/input'
import type { Schemas } from '@/lib/api/types'

function errorAt<T extends FieldValues>(form: UseFormReturn<T>, path: string): string | undefined {
  const e = get(form.formState.errors, path) as { message?: string } | undefined
  return e?.message
}

/** Product detail inputs (category, name, slug, descriptions...). Fields live at the root of the form. */
export function ProductDetailsFields<T extends FieldValues>({
  form,
  categories,
  idPrefix = 'product',
}: {
  form: UseFormReturn<T>
  categories: Schemas['AdminCategoryResponse'][]
  idPrefix?: string
}) {
  const reg = (name: string) => form.register(name as Path<T>)
  const err = (name: string) => errorAt(form, name)
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <FormField
        id={`${idPrefix}-name`}
        label="Tên sản phẩm"
        required
        error={err('name')}
        className="sm:col-span-2"
      >
        <Input {...reg('name')} />
      </FormField>
      <FormField id={`${idPrefix}-category`} label="Danh mục" required error={err('categoryId')}>
        <NativeSelect {...reg('categoryId')}>
          <option value="">— Chọn danh mục —</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
              {c.isActive ? '' : ' (đang ẩn)'}
            </option>
          ))}
        </NativeSelect>
      </FormField>
      <FormField
        id={`${idPrefix}-slug`}
        label="Đường dẫn (slug)"
        error={err('slug')}
        hint="Để trống để tự tạo từ tên, ví dụ kho-ga-la-chanh."
      >
        <Input {...reg('slug')} />
      </FormField>
      <FormField
        id={`${idPrefix}-short`}
        label="Mô tả ngắn"
        error={err('shortDescription')}
        className="sm:col-span-2"
      >
        <Input {...reg('shortDescription')} />
      </FormField>
      <FormField
        id={`${idPrefix}-desc`}
        label="Mô tả chi tiết"
        error={err('description')}
        className="sm:col-span-2"
      >
        <Textarea rows={5} {...reg('description')} />
      </FormField>
      <FormField
        id={`${idPrefix}-ingredients`}
        label="Thành phần"
        error={err('ingredients')}
        className="sm:col-span-2"
      >
        <Textarea rows={2} {...reg('ingredients')} />
      </FormField>
      <FormField id={`${idPrefix}-brand`} label="Thương hiệu" error={err('brand')}>
        <Input {...reg('brand')} />
      </FormField>
      <FormField id={`${idPrefix}-origin`} label="Xuất xứ" error={err('origin')}>
        <Input {...reg('origin')} />
      </FormField>
      <FormField id={`${idPrefix}-shelf`} label="Hạn sử dụng (ngày)" error={err('shelfLifeDays')}>
        <Input inputMode="numeric" {...reg('shelfLifeDays')} />
      </FormField>
    </div>
  )
}

/** Variant inputs. `prefix` is the path of the variant object inside the form ('' = root). */
export function VariantFields<T extends FieldValues>({
  form,
  prefix = '',
  idPrefix = 'variant',
}: {
  form: UseFormReturn<T>
  prefix?: string
  idPrefix?: string
}) {
  const p = prefix ? `${prefix}.` : ''
  const reg = (name: string) => form.register(`${p}${name}` as Path<T>)
  const err = (name: string) => errorAt(form, `${p}${name}`)
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <FormField
        id={`${idPrefix}-name`}
        label="Tên phân loại"
        error={err('name')}
        hint="Ví dụ: Gói 200g, Hũ 500g"
      >
        <Input {...reg('name')} />
      </FormField>
      <FormField id={`${idPrefix}-sku`} label="SKU" required error={err('sku')}>
        <Input className="font-mono" {...reg('sku')} />
      </FormField>
      <FormField id={`${idPrefix}-price`} label="Giá bán (₫)" required error={err('price')}>
        <Input inputMode="numeric" {...reg('price')} />
      </FormField>
      <FormField
        id={`${idPrefix}-compare`}
        label="Giá gốc (₫)"
        error={err('compareAtPrice')}
        hint="Để trống nếu không giảm giá"
      >
        <Input inputMode="numeric" {...reg('compareAtPrice')} />
      </FormField>
      <FormField id={`${idPrefix}-weight`} label="Khối lượng (gram)" error={err('weightGrams')}>
        <Input inputMode="numeric" {...reg('weightGrams')} />
      </FormField>
      <FormField id={`${idPrefix}-sort`} label="Thứ tự hiển thị" error={err('sortOrder')}>
        <Input inputMode="numeric" {...reg('sortOrder')} />
      </FormField>
    </div>
  )
}
