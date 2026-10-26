'use client'

import { useRef, useState, type FormEvent } from 'react'
import { ArrowDown, ArrowUp, Star, Trash2, Upload } from 'lucide-react'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { EmptyState } from '@/components/ui/feedback'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { RemoteImage } from '@/components/ui/remote-image'
import type { useProductMutations } from '@/features/admin/catalog/queries'
import type { Schemas } from '@/lib/api/types'
import { getErrorMessage } from '@/lib/errors'

const MAX_BYTES = 5 * 1024 * 1024
const ACCEPT = ['image/jpeg', 'image/png', 'image/webp']

type Mutations = ReturnType<typeof useProductMutations>

export function ProductImages({
  productName,
  images,
  mutations,
}: {
  productName: string
  images: Schemas['ProductImageResponse'][]
  mutations: Mutations
}) {
  const fileRef = useRef<HTMLInputElement>(null)
  const [altText, setAltText] = useState('')
  const sorted = [...images].sort((a, b) => a.sortOrder - b.sortOrder)
  const busy =
    mutations.uploadImage.isPending ||
    mutations.deleteImage.isPending ||
    mutations.setPrimaryImage.isPending ||
    mutations.reorderImages.isPending

  const onUpload = (e: FormEvent) => {
    e.preventDefault()
    const file = fileRef.current?.files?.[0]
    if (!file) return toast.error('Vui lòng chọn ảnh.')
    if (!ACCEPT.includes(file.type)) return toast.error('Chỉ nhận ảnh JPEG, PNG hoặc WebP.')
    if (file.size > MAX_BYTES) return toast.error('Ảnh tối đa 5 MB.')
    mutations.uploadImage.mutate(
      { file, altText: altText.trim() || null },
      {
        onSuccess: () => {
          toast.success('Đã tải ảnh lên')
          setAltText('')
          if (fileRef.current) fileRef.current.value = ''
        },
        onError: (err) => toast.error(getErrorMessage(err)),
      },
    )
  }

  const move = (index: number, delta: -1 | 1) => {
    const ids = sorted.map((i) => i.id)
    const target = index + delta
    if (target < 0 || target >= ids.length) return
    ;[ids[index], ids[target]] = [ids[target]!, ids[index]!]
    mutations.reorderImages.mutate(ids, { onError: (err) => toast.error(getErrorMessage(err)) })
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Hình ảnh</CardTitle>
        <CardDescription>
          JPEG, PNG hoặc WebP, tối đa 5 MB. Ảnh chính hiển thị ở danh sách sản phẩm.
        </CardDescription>
      </CardHeader>
      {sorted.length === 0 ? (
        <EmptyState title="Chưa có ảnh" />
      ) : (
        <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-4">
          {sorted.map((img, index) => (
            <li key={img.id} className="grid gap-2 rounded-lg border p-2">
              <div className="relative">
                <RemoteImage
                  src={img.url}
                  alt={img.altText ?? productName}
                  className="aspect-square w-full rounded-md"
                />
                {img.isPrimary ? <Badge className="absolute top-1 left-1">Ảnh chính</Badge> : null}
              </div>
              <div className="flex flex-wrap justify-between gap-1">
                <div className="flex gap-1">
                  <Button
                    variant="outline"
                    size="icon-sm"
                    aria-label="Đưa lên trước"
                    disabled={busy || index === 0}
                    onClick={() => move(index, -1)}
                  >
                    <ArrowUp aria-hidden />
                  </Button>
                  <Button
                    variant="outline"
                    size="icon-sm"
                    aria-label="Đưa xuống sau"
                    disabled={busy || index === sorted.length - 1}
                    onClick={() => move(index, 1)}
                  >
                    <ArrowDown aria-hidden />
                  </Button>
                </div>
                <div className="flex gap-1">
                  {!img.isPrimary ? (
                    <Button
                      variant="outline"
                      size="icon-sm"
                      aria-label="Đặt làm ảnh chính"
                      disabled={busy}
                      onClick={() =>
                        mutations.setPrimaryImage.mutate(img.id, {
                          onSuccess: () => toast.success('Đã đặt ảnh chính'),
                          onError: (err) => toast.error(getErrorMessage(err)),
                        })
                      }
                    >
                      <Star aria-hidden />
                    </Button>
                  ) : null}
                  <Button
                    variant="outline"
                    size="icon-sm"
                    aria-label="Xóa ảnh"
                    disabled={busy}
                    onClick={() => {
                      if (!window.confirm('Xóa ảnh này?')) return
                      mutations.deleteImage.mutate(img.id, {
                        onSuccess: () => toast.success('Đã xóa ảnh'),
                        onError: (err) => toast.error(getErrorMessage(err)),
                      })
                    }}
                  >
                    <Trash2 aria-hidden />
                  </Button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
      <form onSubmit={onUpload} className="grid gap-3 border-t pt-4 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
        <div className="grid gap-1.5">
          <Label htmlFor="image-file">Chọn ảnh</Label>
          <Input
            id="image-file"
            ref={fileRef}
            type="file"
            accept={ACCEPT.join(',')}
            className="h-auto py-1.5"
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="image-alt">Mô tả ảnh (alt)</Label>
          <Input
            id="image-alt"
            value={altText}
            maxLength={200}
            onChange={(e) => setAltText(e.target.value)}
          />
        </div>
        <Button type="submit" loading={mutations.uploadImage.isPending}>
          <Upload aria-hidden />
          Tải lên
        </Button>
      </form>
    </Card>
  )
}
