import createClient from 'openapi-fetch'
import { publicApiBaseUrl } from '@/lib/env'
import { ApiError } from '@/lib/errors'
import { getAccessToken, refreshSession } from '@/lib/auth/session'
import { useAuthStore } from '@/lib/auth/auth.store'
import type { paths } from './schema'

function withBearer(request: Request, token: string | null): Request {
  if (!token) return request
  const headers = new Headers(request.headers)
  headers.set('Authorization', `Bearer ${token}`)
  return new Request(request, { headers })
}

function redirectToLogin(): void {
  if (typeof window === 'undefined') return
  const { pathname, search } = window.location
  if (pathname.startsWith('/dang-nhap')) return
  const returnUrl = encodeURIComponent(pathname + search)
  // Hard navigation (outside React): drops all in-memory state of the expired session.
  window.location.assign(new URL(`/dang-nhap?returnUrl=${returnUrl}`, window.location.origin).href)
}

/**
 * Browser fetch with auth: attaches the in-memory access token, and on 401 performs one
 * single-flight refresh through the BFF then retries once. If refresh fails the session is
 * cleared and the user is sent to the login page.
 */
async function authFetch(request: Request): Promise<Response> {
  const retryRequest = request.clone()
  const token = await getAccessToken()
  const response = await fetch(withBearer(request, token))
  if (response.status !== 401 || !token) return response

  const session = await refreshSession()
  if (!session) {
    useAuthStore.getState().clear()
    redirectToLogin()
    return response
  }
  return fetch(withBearer(retryRequest, session.accessToken))
}

/** Typed client generated from backend/openapi.json. Use only from Client Components / browser code. */
export const api = createClient<paths>({ baseUrl: publicApiBaseUrl, fetch: authFetch })

type FetchResult = { data?: unknown; error?: unknown; response: Response }

/**
 * Success payload of an openapi-fetch result. The error branch contributes `data?: never`,
 * so `undefined` is stripped; responses without a body (204) become `undefined`.
 */
type SuccessData<R extends FetchResult> = [Exclude<R['data'], undefined>] extends [never]
  ? undefined
  : Exclude<R['data'], undefined>

/** Awaits an openapi-fetch call and returns `data` or throws a normalised `ApiError`. */
export async function unwrap<R extends FetchResult>(call: Promise<R>): Promise<SuccessData<R>> {
  let result: R
  try {
    result = await call
  } catch (error) {
    if (error instanceof ApiError) throw error
    throw ApiError.network()
  }
  if (result.error !== undefined || !result.response.ok) {
    throw ApiError.fromProblem(result.response.status, result.error)
  }
  return result.data as SuccessData<R>
}
