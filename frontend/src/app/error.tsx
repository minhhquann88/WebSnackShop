'use client'

import { Button } from '@/components/ui/button'

export default function GlobalError({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <main className="flex min-h-dvh flex-col items-center justify-center gap-4 px-4 text-center">
      <span className="h-3 w-40 rounded-full stripes-sm" aria-hidden />
      <h1 className="font-display text-3xl font-extrabold">Đã có lỗi xảy ra</h1>
      <p className="text-muted-foreground">Không tải được trang. Vui lòng thử lại sau ít phút.</p>
      <Button onClick={reset}>Thử lại</Button>
    </main>
  )
}
