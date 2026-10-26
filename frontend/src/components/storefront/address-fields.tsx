'use client'

import { get, type FieldErrors, type FieldValues, type Path, type UseFormRegister } from 'react-hook-form'
import { FormField } from '@/components/ui/form-field'
import { Input } from '@/components/ui/input'

type AddressFieldsProps<T extends FieldValues> = {
  /** Path prefix in the form, e.g. `shippingAddress` or `address`. */
  prefix: string
  register: UseFormRegister<T>
  errors: FieldErrors<T>
  idPrefix: string
}

const FIELDS = [
  { name: 'fullName', label: 'Họ tên người nhận', autoComplete: 'name', span: false },
  { name: 'phone', label: 'Số điện thoại', autoComplete: 'tel', span: false, type: 'tel' },
  { name: 'line1', label: 'Số nhà, tên đường', autoComplete: 'address-line1', span: true },
  { name: 'ward', label: 'Phường/Xã', autoComplete: 'address-level3', span: false },
  { name: 'district', label: 'Quận/Huyện', autoComplete: 'address-level2', span: false },
  { name: 'province', label: 'Tỉnh/Thành phố', autoComplete: 'address-level1', span: true },
] as const

/** Vietnamese shipping address inputs (matches `AddressRequest`). */
export function AddressFields<T extends FieldValues>({
  prefix,
  register,
  errors,
  idPrefix,
}: AddressFieldsProps<T>) {
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      {FIELDS.map((f) => {
        const path = `${prefix}.${f.name}` as Path<T>
        const error = get(errors, path) as { message?: string } | undefined
        return (
          <FormField
            key={f.name}
            id={`${idPrefix}-${f.name}`}
            label={f.label}
            error={error?.message}
            required
            className={f.span ? 'sm:col-span-2' : undefined}
          >
            <Input
              type={'type' in f ? f.type : 'text'}
              autoComplete={f.autoComplete}
              inputMode={f.name === 'phone' ? 'numeric' : undefined}
              {...register(path)}
            />
          </FormField>
        )
      })}
    </div>
  )
}
