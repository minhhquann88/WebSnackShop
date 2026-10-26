'use client'

import { Button } from '@/components/ui/button'

export default function GlobalError({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <main className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center gap-4 px-4 text-center">
      <h1 className="text-xl font-semibold">Đã có lỗi xảy ra</h1>
      <p className="text-muted-foreground">Không tải được trang. Vui lòng thử lại sau ít phút.</p>
      <Button onClick={reset}>Thử lại</Button>
    </main>
  )
}
