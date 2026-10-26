import { NextResponse, type NextRequest } from 'next/server'
import { REFRESH_COOKIE } from '@/lib/auth/constants'

/**
 * Cheap gate for protected areas: no refresh cookie → redirect to login.
 * Roles are checked again in the admin layout and, authoritatively, by the API.
 */
export function proxy(request: NextRequest): NextResponse {
  if (request.cookies.has(REFRESH_COOKIE)) return NextResponse.next()
  const url = request.nextUrl.clone()
  url.pathname = '/dang-nhap'
  url.search = `?returnUrl=${encodeURIComponent(request.nextUrl.pathname + request.nextUrl.search)}`
  return NextResponse.redirect(url)
}

export const config = {
  matcher: ['/admin/:path*', '/tai-khoan/:path*'],
}
