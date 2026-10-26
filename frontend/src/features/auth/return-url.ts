/** Only allow same-site relative paths as post-login redirect targets (prevents open redirects). */
export function safeReturnUrl(value: string | null | undefined, fallback = '/'): string {
  if (!value || !value.startsWith('/') || value.startsWith('//') || value.startsWith('/\\')) return fallback
  return value
}
