'use client'

import { Copy } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { RemoteImage } from '@/components/ui/remote-image'
import type { BankTransferInstructions } from '@/lib/api/types'
import { formatDateTime, formatVnd } from '@/lib/format'

function CopyRow({ label, value, display }: { label: string; value: string; display?: string }) {
  async function copy() {
    try {
      await navigator.clipboard.writeText(value)
      toast.success(`Đã sao chép ${label.toLowerCase()}`)
    } catch {
      toast.error('Không sao chép được, vui lòng chép thủ công.')
    }
  }
  return (
    <div className="flex items-center justify-between gap-2 border-b py-2 last:border-0">
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="flex items-center gap-1 text-right font-medium break-all">
        {display ?? value}
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          onClick={copy}
          aria-label={`Sao chép ${label.toLowerCase()}`}
        >
          <Copy aria-hidden />
        </Button>
      </dd>
    </div>
  )
}

/** VietQR + transfer details returned by the API for bank-transfer orders. */
export function BankTransferBox({ info }: { info: BankTransferInstructions }) {
  return (
    <div className="grid gap-4 rounded-xl border-2 border-primary/30 bg-primary/5 p-4 sm:grid-cols-[220px_1fr] sm:p-6">
      <div className="mx-auto w-full max-w-[220px]">
        <RemoteImage
          src={info.qrImageUrl}
          alt="Mã QR chuyển khoản"
          className="aspect-square w-full rounded-lg bg-white object-contain"
        />
        <p className="mt-2 text-center text-xs text-muted-foreground">Quét bằng ứng dụng ngân hàng</p>
      </div>
      <div className="grid content-start gap-2">
        <p className="font-semibold">Thông tin chuyển khoản</p>
        <dl>
          <div className="flex justify-between gap-2 border-b py-2">
            <dt className="text-sm text-muted-foreground">Ngân hàng</dt>
            <dd className="text-right font-medium">{info.bankName}</dd>
          </div>
          <CopyRow label="Số tài khoản" value={info.accountNumber} />
          <div className="flex justify-between gap-2 border-b py-2">
            <dt className="text-sm text-muted-foreground">Chủ tài khoản</dt>
            <dd className="text-right font-medium">{info.accountName}</dd>
          </div>
          <CopyRow
            label="Số tiền"
            value={String(Math.round(info.amount.amount))}
            display={formatVnd(info.amount)}
          />
          <CopyRow label="Nội dung chuyển khoản" value={info.transferContent} />
        </dl>
        <p className="text-sm">
          Vui lòng chuyển khoản <strong>đúng nội dung</strong> trước{' '}
          <strong>{formatDateTime(info.payBefore)}</strong>, đơn chưa thanh toán sau thời hạn sẽ tự động hủy.
        </p>
      </div>
    </div>
  )
}
