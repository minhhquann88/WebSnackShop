import Link from 'next/link'
import { buttonVariants } from '@/components/ui/button'

export default function NotFound() {
  return (
    <main className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center gap-4 px-4 text-center">
      <p className="text-5xl font-bold text-primary">404</p>
      <h1 className="text-xl font-semibold">Không tìm thấy trang</h1>
      <p className="text-muted-foreground">Trang bạn tìm không tồn tại hoặc đã bị gỡ.</p>
      <Link href="/" className={buttonVariants()}>
        Về trang chủ
      </Link>
    </main>
  )
}
