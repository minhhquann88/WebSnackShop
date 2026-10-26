import * as React from 'react'
import { ImageOff } from 'lucide-react'
import { cn } from '@/lib/utils'

type RemoteImageProps = Omit<React.ComponentProps<'img'>, 'src' | 'alt'> & {
  src: string | null | undefined
  alt: string
}

/**
 * Plain <img> for API-provided absolute URLs (placehold.co seed images, `/uploads/*` on the API host).
 * next/image blocks localhost/private IPs by default in Next 16 and the upload host is
 * deployment-specific, so optimisation is left to the CDN in front of the API.
 */
export function RemoteImage({ src, alt, className, ...props }: RemoteImageProps) {
  if (!src) {
    return (
      <div
        role="img"
        aria-label={alt}
        className={cn('flex items-center justify-center bg-muted text-muted-foreground', className)}
      >
        <ImageOff className="size-6" aria-hidden />
      </div>
    )
  }
  return (
    <img
      src={src}
      alt={alt}
      loading="lazy"
      decoding="async"
      className={cn('object-cover', className)}
      {...props}
    />
  )
}
