'use client'

import { usePathname, useRouter, useSearchParams } from 'next/navigation'
import { NativeSelect } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

type SortOption = { value: string; label: string }

export function SortSelect({ options, value }: { options: readonly SortOption[]; value: string }) {
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()

  function onChange(event: React.ChangeEvent<HTMLSelectElement>) {
    const next = new URLSearchParams(searchParams.toString())
    if (event.target.value) next.set('sort', event.target.value)
    else next.delete('sort')
    next.delete('page')
    const qs = next.toString()
    router.push(qs ? `${pathname}?${qs}` : pathname)
  }

  return (
    <div className="flex items-center gap-2">
      <Label htmlFor="sort" className="shrink-0 text-muted-foreground">
        Sắp xếp
      </Label>
      <NativeSelect id="sort" value={value} onChange={onChange} className="w-auto">
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </NativeSelect>
    </div>
  )
}
