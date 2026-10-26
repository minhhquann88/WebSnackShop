'use client'

import { useState } from 'react'
import { RemoteImage } from '@/components/ui/remote-image'
import type { Schemas } from '@/lib/api/types'
import { cn } from '@/lib/utils'

type Image = Schemas['ProductImageResponse']

export function ProductGallery({ images, name }: { images: Image[]; name: string }) {
  const sorted = [...images].sort(
    (a, b) => Number(b.isPrimary) - Number(a.isPrimary) || a.sortOrder - b.sortOrder,
  )
  const [activeId, setActiveId] = useState(sorted[0]?.id)
  const active = sorted.find((i) => i.id === activeId) ?? sorted[0]

  return (
    <div className="grid gap-3">
      <div className="aspect-square overflow-hidden rounded-xl border bg-muted">
        <RemoteImage src={active?.url} alt={active?.altText ?? name} className="size-full" />
      </div>
      {sorted.length > 1 ? (
        <ul className="flex gap-2 overflow-x-auto" aria-label="Ảnh sản phẩm">
          {sorted.map((img, index) => (
            <li key={img.id}>
              <button
                type="button"
                onClick={() => setActiveId(img.id)}
                aria-label={`Xem ảnh ${index + 1}`}
                aria-pressed={img.id === active?.id}
                className={cn(
                  'size-16 overflow-hidden rounded-lg border-2 sm:size-20',
                  img.id === active?.id ? 'border-primary' : 'border-transparent',
                )}
              >
                <RemoteImage src={img.url} alt="" className="size-full" />
              </button>
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
