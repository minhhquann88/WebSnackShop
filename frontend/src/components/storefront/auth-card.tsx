import type { ReactNode } from 'react'
import { Card, CardDescription, CardHeader } from '@/components/ui/card'

type AuthCardProps = {
  title: string
  description?: string
  children: ReactNode
  footer?: ReactNode
}

export function AuthCard({ title, description, children, footer }: AuthCardProps) {
  return (
    <div className="mx-auto grid w-full max-w-md gap-4 py-4">
      <Card>
        <CardHeader>
          <h1 className="text-2xl font-semibold">{title}</h1>
          {description ? <CardDescription>{description}</CardDescription> : null}
        </CardHeader>
        {children}
      </Card>
      {footer ? <div className="text-center text-sm text-muted-foreground">{footer}</div> : null}
    </div>
  )
}
