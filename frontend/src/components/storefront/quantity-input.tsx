'use client'

import { Minus, Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

type QuantityInputProps = {
  value: number
  onChange: (value: number) => void
  min?: number
  max: number
  disabled?: boolean
  label: string
  className?: string
}

export function QuantityInput({
  value,
  onChange,
  min = 1,
  max,
  disabled,
  label,
  className,
}: QuantityInputProps) {
  const clamp = (n: number) => Math.max(min, Math.min(max, Number.isFinite(n) ? Math.floor(n) : min))
  return (
    <div
      className={cn('inline-flex items-center rounded-md border', className)}
      role="group"
      aria-label={label}
    >
      <Button
        type="button"
        variant="ghost"
        size="icon-sm"
        onClick={() => onChange(clamp(value - 1))}
        disabled={disabled || value <= min}
        aria-label="Giảm số lượng"
      >
        <Minus aria-hidden />
      </Button>
      <input
        type="number"
        inputMode="numeric"
        min={min}
        max={max}
        value={value}
        disabled={disabled}
        aria-label={label}
        onChange={(e) => onChange(clamp(e.target.valueAsNumber))}
        className="h-8 w-12 [appearance:textfield] bg-transparent text-center text-sm outline-none [&::-webkit-inner-spin-button]:appearance-none"
      />
      <Button
        type="button"
        variant="ghost"
        size="icon-sm"
        onClick={() => onChange(clamp(value + 1))}
        disabled={disabled || value >= max}
        aria-label="Tăng số lượng"
      >
        <Plus aria-hidden />
      </Button>
    </div>
  )
}
