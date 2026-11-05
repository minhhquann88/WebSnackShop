import Link from 'next/link'
import { buttonVariants } from '@/components/ui/button'

export default function NotFound() {
  return (
    <main className="flex min-h-dvh flex-col items-center justify-center gap-4 px-4 text-center">
      <span className="h-3 w-40 rounded-full stripes-sm" aria-hidden />
      <p className="font-display text-8xl leading-none font-extrabold text-chili overprint">404</p>
      <h1 className="font-display text-2xl font-extrabold">Không tìm thấy trang</h1>
      <p className="text-muted-foreground">Trang bạn tìm không tồn tại hoặc đã bị gỡ.</p>
      <Link href="/" className={buttonVariants()}>
        Về trang chủ
      </Link>
    </main>
  )
}
