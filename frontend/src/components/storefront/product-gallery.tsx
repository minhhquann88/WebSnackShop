'use client'

import { useState } from 'react'
import { RemoteImage } from '@/components/ui/remote-image'
import type { Schemas } from '@/lib/api/types'
import { cn } from '@/lib/utils'

type Image = Schemas['ProductImageResponse']

/** The photo printed on the bag face: a big disc with a white rim, thumbnails as smaller discs. */
export function ProductGallery({ images, name }: { images: Image[]; name: string }) {
  const sorted = [...images].sort(
    (a, b) => Number(b.isPrimary) - Number(a.isPrimary) || a.sortOrder - b.sortOrder,
  )
  const [activeId, setActiveId] = useState(sorted[0]?.id)
  const active = sorted.find((i) => i.id === activeId) ?? sorted[0]

  return (
    <div className="grid justify-items-center gap-4">
      <RemoteImage
        src={active?.url}
        alt={active?.altText ?? name}
        className="aspect-square w-3/5 max-w-sm rounded-full bg-card object-cover ring-8 ring-white/60 sm:w-full"
      />
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
                  'size-16 overflow-hidden rounded-full bg-card ring-4 transition-[box-shadow] sm:size-20',
                  img.id === active?.id ? 'ring-white' : 'ring-white/30 hover:ring-white/70',
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
