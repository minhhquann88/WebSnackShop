import { ApiError, applyFieldErrors, getErrorMessage, messageForCode, toFormPath } from './errors'

describe('ApiError.fromProblem', () => {
  it('uses the problem title as the stable code and keeps field errors', () => {
    const error = ApiError.fromProblem(400, {
      title: 'Validation.Failed',
      status: 400,
      errors: { 'shippingAddress.phone': ['Phone must be 10 digits starting with 0.'] },
      traceId: 'abc',
    })
    expect(error.code).toBe('Validation.Failed')
    expect(error.errors['shippingAddress.phone']).toEqual(['Phone must be 10 digits starting with 0.'])
    expect(error.traceId).toBe('abc')
  })

  it('falls back to Http.<status> for framework problems without a dotted code', () => {
    expect(ApiError.fromProblem(401, { title: 'Unauthorized', status: 401 }).code).toBe('Http.401')
    expect(ApiError.fromProblem(502, 'Bad gateway').code).toBe('Http.502')
  })
})

describe('messages', () => {
  it('maps known codes to Vietnamese', () => {
    expect(messageForCode('Orders.PaymentRequired')).toMatch(/chưa được thanh toán/)
    expect(getErrorMessage(new ApiError(409, 'Inventory.InsufficientStock'))).toBe('Sản phẩm không đủ hàng.')
  })

  it('falls back to the status message, then a generic one', () => {
    expect(messageForCode('Some.UnknownCode', 429)).toMatch(/quá nhanh/)
    expect(messageForCode('Some.UnknownCode', 418)).toBe('Đã có lỗi xảy ra. Vui lòng thử lại.')
    expect(getErrorMessage(new TypeError('boom'))).toBe('Đã có lỗi xảy ra. Vui lòng thử lại.')
    expect(getErrorMessage(ApiError.network())).toMatch(/kết nối/)
  })
})

describe('toFormPath', () => {
  it('converts API keys to React Hook Form paths', () => {
    expect(toFormPath('shippingAddress.phone')).toBe('shippingAddress.phone')
    expect(toFormPath('items[0].quantity')).toBe('items.0.quantity')
    expect(toFormPath('ShippingAddress.Phone')).toBe('shippingAddress.phone')
  })
})

describe('applyFieldErrors', () => {
  it('sets known fields and returns the rest translated', () => {
    const setError = vi.fn()
    const error = new ApiError(400, 'Validation.Failed', {
      'shippingAddress.phone': ['Phone must be 10 digits starting with 0.'],
      contactEmail: ["'Contact Email' is not a valid email address."],
      'items[0].quantity': ["'Quantity' must be between 1 and 99. You entered 0."],
    })
    const rest = applyFieldErrors(error, setError, ['shippingAddress.phone', 'contactEmail'])
    expect(setError).toHaveBeenCalledWith('shippingAddress.phone', {
      type: 'server',
      message: 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.',
    })
    expect(setError).toHaveBeenCalledWith('contactEmail', { type: 'server', message: 'Email không hợp lệ.' })
    expect(rest).toEqual(['Giá trị nằm ngoài khoảng cho phép.'])
  })

  it('ignores non-API errors', () => {
    const setError = vi.fn()
    expect(applyFieldErrors(new Error('x'), setError, ['a'])).toEqual([])
    expect(setError).not.toHaveBeenCalled()
  })
})
