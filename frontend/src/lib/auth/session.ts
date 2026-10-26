import { ApiError } from '@/lib/errors'
import { useAuthStore, type BrowserSession } from './auth.store'

/** Refresh a little before expiry so requests rarely hit a 401. */
const EXPIRY_SKEW_MS = 30_000
const REFRESH_LOCK = 'snackshop-auth-refresh'

let inflightRefresh: Promise<BrowserSession | null> | null = null

async function readJson(response: Response): Promise<unknown> {
  try {
    return await response.json()
  } catch {
    return null
  }
}

async function postBff(action: string, body?: unknown): Promise<Response> {
  try {
    return await fetch(`/api/auth/${action}`, {
      method: 'POST',
      credentials: 'same-origin',
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
      cache: 'no-store',
    })
  } catch {
    throw ApiError.network()
  }
}

/** Runs `fn` while holding a cross-tab lock (Web Locks API) so two tabs never rotate the same refresh token. */
async function withCrossTabLock<T>(fn: () => Promise<T>): Promise<T> {
  if (typeof navigator !== 'undefined' && 'locks' in navigator && navigator.locks) {
    return navigator.locks.request(REFRESH_LOCK, fn)
  }
  return fn()
}

async function doRefresh(): Promise<BrowserSession | null> {
  const store = useAuthStore.getState()
  let response: Response
  try {
    response = await postBff('refresh')
  } catch {
    // Network failure: keep any existing session (it may still be valid), otherwise become anonymous.
    if (!store.accessToken) store.clear()
    return null
  }
  if (response.ok) {
    const session = (await response.json()) as BrowserSession
    useAuthStore.getState().setSession(session)
    return session
  }
  useAuthStore.getState().clear()
  return null
}

/**
 * Single-flight refresh: concurrent callers (e.g. several requests receiving 401 at once)
 * share one in-flight BFF call. Backend refresh tokens rotate and reuse revokes every session,
 * so this must never run twice in parallel.
 */
export function refreshSession(): Promise<BrowserSession | null> {
  inflightRefresh ??= withCrossTabLock(doRefresh).finally(() => {
    inflightRefresh = null
  })
  return inflightRefresh
}

/** Returns a usable access token, refreshing first if it is about to expire. Null for anonymous users. */
export async function getAccessToken(): Promise<string | null> {
  const state = useAuthStore.getState()
  if (state.status === 'loading') {
    const session = await refreshSession()
    return session?.accessToken ?? null
  }
  if (state.status === 'anonymous' || !state.accessToken) return null
  if (state.expiresAt - Date.now() > EXPIRY_SKEW_MS) return state.accessToken
  const session = await refreshSession()
  return session?.accessToken ?? null
}

async function authenticate(action: 'login' | 'register' | 'google', body: unknown): Promise<BrowserSession> {
  const response = await postBff(action, body)
  const payload = await readJson(response)
  if (!response.ok) throw ApiError.fromProblem(response.status, payload)
  const session = payload as BrowserSession
  useAuthStore.getState().setSession(session)
  return session
}

export function loginWithPassword(email: string, password: string): Promise<BrowserSession> {
  return authenticate('login', { email, password })
}

export function registerAccount(input: {
  email: string
  password: string
  fullName: string
  phoneNumber: string | null
}): Promise<BrowserSession> {
  return authenticate('register', input)
}

export function loginWithGoogle(idToken: string): Promise<BrowserSession> {
  return authenticate('google', { idToken })
}

/** Revokes the refresh token on the backend (via BFF) and clears the cookie and in-memory session. */
export async function logout(): Promise<void> {
  try {
    await postBff('logout')
  } finally {
    useAuthStore.getState().clear()
  }
}
