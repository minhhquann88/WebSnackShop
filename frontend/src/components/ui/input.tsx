import * as React from 'react'
import { cn } from '@/lib/utils'

/** Fields: white, rounded, a warm hairline that turns chili on focus. */
const fieldClass =
  'w-full min-w-0 rounded-lg border border-input/70 bg-card px-3 py-2 text-sm text-foreground transition-[border-color,box-shadow] outline-none placeholder:text-muted-foreground hover:border-input focus-visible:border-primary focus-visible:ring-[3px] focus-visible:ring-ring/25 disabled:cursor-not-allowed disabled:bg-muted disabled:opacity-70 aria-invalid:border-destructive aria-invalid:ring-destructive/20'

export function Input({ className, type, ...props }: React.ComponentProps<'input'>) {
  return <input type={type} data-slot="input" className={cn(fieldClass, 'h-10', className)} {...props} />
}

export function Textarea({ className, ...props }: React.ComponentProps<'textarea'>) {
  return <textarea data-slot="textarea" className={cn(fieldClass, 'min-h-20', className)} {...props} />
}

/** Native select styled like the inputs (accessible and mobile-friendly by default). */
export function NativeSelect({ className, children, ...props }: React.ComponentProps<'select'>) {
  return (
    <select data-slot="select" className={cn(fieldClass, 'h-10 pr-8', className)} {...props}>
      {children}
    </select>
  )
}

export function Checkbox({ className, ...props }: Omit<React.ComponentProps<'input'>, 'type'>) {
  return (
    <input
      type="checkbox"
      className={cn('size-4 shrink-0 rounded border-input accent-chili', className)}
      {...props}
    />
  )
}
