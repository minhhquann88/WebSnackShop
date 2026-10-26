import type { Metadata } from 'next'
import { AccountShell } from './account-shell'

export const metadata: Metadata = { title: 'Tài khoản', robots: { index: false } }

export default function AccountLayout({ children }: { children: React.ReactNode }) {
  return <AccountShell>{children}</AccountShell>
}
