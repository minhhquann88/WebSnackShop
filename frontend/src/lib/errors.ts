import messages from '@/messages/errors.vi.json'

const errorMessages: Record<string, string> = messages
const FALLBACK = 'Đã có lỗi xảy ra. Vui lòng thử lại.'

/** Normalised RFC 9457 problem returned by the API (or synthesised for network failures). */
export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly errors: Record<string, string[]>
  readonly traceId: string | undefined

  constructor(status: number, code: string, errors: Record<string, string[]> = {}, traceId?: string) {
    super(code)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.errors = errors
    this.traceId = traceId
  }

  /**
   * Builds an ApiError from a problem+json body. `title` is the stable error code;
   * framework-generated problems (plain 401/403/404) use the HTTP reason phrase as title,
   * so codes without a dot fall back to `Http.<status>`.
   */
  static fromProblem(status: number, body: unknown): ApiError {
    if (isRecord(body)) {
      const title = typeof body.title === 'string' && body.title.includes('.') ? body.title : `Http.${status}`
      const traceId = typeof body.traceId === 'string' ? body.traceId : undefined
      return new ApiError(status, title, parseFieldErrors(body.errors), traceId)
    }
    return new ApiError(status, `Http.${status}`)
  }

  static network(): ApiError {
    return new ApiError(0, 'Network')
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function parseFieldErrors(value: unknown): Record<string, string[]> {
  if (!isRecord(value)) return {}
  const result: Record<string, string[]> = {}
  for (const [key, messagesForKey] of Object.entries(value)) {
    if (Array.isArray(messagesForKey)) {
      result[key] = messagesForKey.filter((m): m is string => typeof m === 'string')
    }
  }
  return result
}

/** Vietnamese message for an error code; falls back to the HTTP status message then a generic one. */
export function messageForCode(code: string, status?: number): string {
  return (
    errorMessages[code] ?? (status !== undefined ? errorMessages[`Http.${status}`] : undefined) ?? FALLBACK
  )
}

/** User-facing Vietnamese message for anything thrown by API calls. Never exposes `detail`. */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) return messageForCode(error.code, error.status)
  return FALLBACK
}

export function isApiError(error: unknown, code?: string): error is ApiError {
  return error instanceof ApiError && (code === undefined || error.code === code)
}

/**
 * Converts an API field key (`shippingAddress.phone`, `items[0].quantity`, `ShippingAddress.Phone`)
 * into a React Hook Form path (`shippingAddress.phone`, `items.0.quantity`).
 */
export function toFormPath(apiKey: string): string {
  return apiKey
    .replace(/\[(\d+)\]/g, '.$1')
    .split('.')
    .map((segment) => (segment.length > 0 ? segment.charAt(0).toLowerCase() + segment.slice(1) : segment))
    .join('.')
}

export interface FieldErrorTarget {
  setError: (name: never, error: { type: string; message: string }) => void
}

/**
 * Applies 400 validation errors to a React Hook Form instance.
 * Only paths listed in `knownFields` are attached to inputs; the rest are returned
 * (already translated) so the caller can show them as a form-level message.
 * `prefixMap` rewrites API prefixes to form prefixes, e.g. `{ address: 'address' }`.
 */
export function applyFieldErrors(
  error: unknown,
  setError: FieldErrorTarget['setError'],
  knownFields: readonly string[],
  prefixMap: Record<string, string> = {},
): string[] {
  if (!(error instanceof ApiError)) return []
  const set = setError as unknown as (name: string, error: { type: string; message: string }) => void
  const unmatched: string[] = []
  for (const [apiKey, fieldMessages] of Object.entries(error.errors)) {
    let path = toFormPath(apiKey)
    for (const [from, to] of Object.entries(prefixMap)) {
      if (path === from || path.startsWith(`${from}.`)) path = to + path.slice(from.length)
    }
    const message = translateValidationMessage(fieldMessages[0] ?? messageForCode(error.code, error.status))
    if (knownFields.includes(path)) set(path, { type: 'server', message })
    else unmatched.push(message)
  }
  return unmatched
}

/**
 * FluentValidation messages from the API are English. Translate the common shapes;
 * anything unrecognised falls back to a generic Vietnamese message.
 */
export function translateValidationMessage(message: string): string {
  const rules: [RegExp, string][] = [
    [/phone/i, 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.'],
    [/must not be empty|is required|required/i, 'Trường này là bắt buộc.'],
    [/email/i, 'Email không hợp lệ.'],
    [/password/i, 'Mật khẩu chưa đạt yêu cầu (tối thiểu 8 ký tự, gồm chữ và số).'],
    [/sku/i, 'Mã SKU không hợp lệ.'],
    [/slug/i, 'Đường dẫn chỉ gồm chữ thường không dấu, số và dấu gạch ngang.'],
    [/between|in the range|greater than|less than|at least/i, 'Giá trị nằm ngoài khoảng cho phép.'],
    [/characters or fewer|maximum length|too long|at most/i, 'Nội dung quá dài.'],
  ]
  for (const [pattern, vi] of rules) if (pattern.test(message)) return vi
  return 'Giá trị không hợp lệ.'
}
