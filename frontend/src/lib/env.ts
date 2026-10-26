/** Public API origin (no trailing slash). Inlined at build time by Next.js. */
export const publicApiBaseUrl = (process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5000').replace(
  /\/$/,
  '',
)

/** Google Identity Services client id; empty string hides the Google button. */
export const googleClientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID ?? ''

/** API origin for server-side code (SSR, BFF route handlers). Falls back to the public URL. */
export function serverApiBaseUrl(): string {
  const internal = process.env.API_INTERNAL_URL
  return (internal && internal.length > 0 ? internal : publicApiBaseUrl).replace(/\/$/, '')
}
