import { NextResponse, type NextRequest } from 'next/server'
import { backendApi } from '@/lib/api/server'
import type { Schemas } from '@/lib/api/types'
import { REFRESH_COOKIE } from '@/lib/auth/constants'

type AuthResponse = Schemas['AuthResponse']

export const dynamic = 'force-dynamic'

/** Secure in production unless explicitly disabled (AUTH_COOKIE_SECURE=false, e.g. plain-HTTP intranet test box). */
const cookieSecure = process.env.AUTH_COOKIE_SECURE
  ? process.env.AUTH_COOKIE_SECURE === 'true'
  : process.env.NODE_ENV === 'production'

/**
 * Short-lived memo of refresh results keyed by the refresh token that was presented.
 * Backend refresh tokens rotate and reusing one revokes every session, so if two requests
 * arrive with the same cookie (two tabs, a double click) the second gets the same result
 * instead of replaying the old token. Per-process only — enough for a single Next instance.
 */
const REFRESH_MEMO_MS = 30_000
interface RefreshMemo {
  promise: Promise<{ status: number; body: unknown }>
  createdAt: number
}
const refreshMemo = new Map<string, RefreshMemo>()

function pruneMemo(now: number): void {
  for (const [key, memo] of refreshMemo) if (now - memo.createdAt > REFRESH_MEMO_MS) refreshMemo.delete(key)
}

function forwardHeaders(request: NextRequest): HeadersInit {
  const forwardedFor = request.headers.get('x-forwarded-for')
  const userAgent = request.headers.get('user-agent')
  const headers: Record<string, string> = {}
  if (forwardedFor) headers['X-Forwarded-For'] = forwardedFor
  // Lets the API rate-limit sign-ins per shopper instead of per BFF server (RateLimiting:TrustedProxyKey).
  const proxyKey = process.env.BFF_PROXY_KEY
  const clientIp = forwardedFor?.split(',')[0]?.trim() || request.headers.get('x-real-ip')
  if (proxyKey && clientIp) {
    headers['X-Proxy-Key'] = proxyKey
    headers['X-Client-IP'] = clientIp
  }
  if (userAgent) headers['User-Agent'] = userAgent
  return headers
}

function problem(status: number, title: string): NextResponse {
  return NextResponse.json(
    { title, status },
    { status, headers: { 'Content-Type': 'application/problem+json', 'Cache-Control': 'no-store' } },
  )
}

function sessionResponse(auth: AuthResponse): NextResponse {
  const response = NextResponse.json(
    { accessToken: auth.accessToken, accessTokenExpiresAt: auth.accessTokenExpiresAt, user: auth.user },
    { headers: { 'Cache-Control': 'no-store' } },
  )
  response.cookies.set(REFRESH_COOKIE, auth.refreshToken, {
    httpOnly: true,
    sameSite: 'lax',
    secure: cookieSecure,
    path: '/',
    expires: new Date(auth.refreshTokenExpiresAt),
  })
  return response
}

function relay(status: number, body: unknown, clearCookie = false): NextResponse {
  const response = NextResponse.json(body ?? { title: `Http.${status}`, status }, {
    status,
    headers: { 'Content-Type': 'application/problem+json', 'Cache-Control': 'no-store' },
  })
  if (clearCookie) response.cookies.delete({ name: REFRESH_COOKIE, path: '/' })
  return response
}

async function readBody(request: NextRequest): Promise<unknown> {
  try {
    return await request.json()
  } catch {
    return null
  }
}

async function handleCredentials(
  action: 'login' | 'register' | 'google',
  request: NextRequest,
): Promise<NextResponse> {
  const body = await readBody(request)
  if (body === null || typeof body !== 'object') return problem(400, 'Request.Malformed')
  const init = { headers: forwardHeaders(request) }
  const result =
    action === 'login'
      ? await backendApi.POST('/api/v1/auth/login', { ...init, body: body as Schemas['LoginCommand'] })
      : action === 'register'
        ? await backendApi.POST('/api/v1/auth/register', {
            ...init,
            body: body as Schemas['RegisterCommand'],
          })
        : await backendApi.POST('/api/v1/auth/google', {
            ...init,
            body: body as Schemas['LoginWithGoogleCommand'],
          })
  if (result.data) return sessionResponse(result.data)
  return relay(result.response.status, result.error)
}

async function handleRefresh(request: NextRequest): Promise<NextResponse> {
  const refreshToken = request.cookies.get(REFRESH_COOKIE)?.value
  if (!refreshToken) return problem(401, 'Auth.NotAuthenticated')

  const now = Date.now()
  pruneMemo(now)
  let memo = refreshMemo.get(refreshToken)
  if (!memo) {
    memo = {
      createdAt: now,
      promise: backendApi
        .POST('/api/v1/auth/refresh', { headers: forwardHeaders(request), body: { refreshToken } })
        .then((r) => ({ status: r.response.status, body: r.data ?? r.error })),
    }
    refreshMemo.set(refreshToken, memo)
  }
  const { status, body } = await memo.promise
  if (status === 200) return sessionResponse(body as AuthResponse)
  refreshMemo.delete(refreshToken)
  return relay(status, body, status === 400 || status === 401)
}

async function handleLogout(request: NextRequest): Promise<NextResponse> {
  const refreshToken = request.cookies.get(REFRESH_COOKIE)?.value
  if (refreshToken) {
    try {
      await backendApi.POST('/api/v1/auth/logout', {
        headers: forwardHeaders(request),
        body: { refreshToken },
      })
    } catch {
      // Best effort: the cookie is cleared regardless.
    }
  }
  const response = new NextResponse(null, { status: 204 })
  response.cookies.delete({ name: REFRESH_COOKIE, path: '/' })
  return response
}

export async function POST(
  request: NextRequest,
  ctx: RouteContext<'/api/auth/[action]'>,
): Promise<NextResponse> {
  const { action } = await ctx.params
  try {
    switch (action) {
      case 'login':
      case 'register':
      case 'google':
        return await handleCredentials(action, request)
      case 'refresh':
        return await handleRefresh(request)
      case 'logout':
        return await handleLogout(request)
      default:
        return problem(404, 'Http.404')
    }
  } catch {
    return problem(502, 'Network')
  }
}
