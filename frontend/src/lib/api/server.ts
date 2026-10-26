import 'server-only'
import createClient from 'openapi-fetch'
import { serverApiBaseUrl } from '@/lib/env'
import type { paths } from './schema'

/**
 * Server-side client for public catalog data in Server Components.
 * Uses Next's data cache with a 60 s revalidation window (ISR).
 */
export const publicApi = createClient<paths>({
  baseUrl: serverApiBaseUrl(),
  fetch: (request: Request) => fetch(request, { next: { revalidate: 60 } }),
})

/** Uncached server client (BFF route handlers, anything user-specific). */
export const backendApi = createClient<paths>({
  baseUrl: serverApiBaseUrl(),
  fetch: (request: Request) => fetch(request, { cache: 'no-store' }),
})
