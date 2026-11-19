import { expect, test, type Page } from '@playwright/test'

const API_URL = process.env.E2E_API_URL ?? 'http://localhost:5000'
const ADMIN_EMAIL = process.env.E2E_ADMIN_EMAIL ?? 'admin@snackshop.local'
const ADMIN_PASSWORD = process.env.E2E_ADMIN_PASSWORD ?? 'Admin12345'
const ORDER_NUMBER = /SS-\d{8}-\d{5}/
// Generous: `next dev` compiles each route on first visit. Against `next start` navigations take < 2 s.
const NAVIGATION_TIMEOUT = 60_000

interface ProductSummary {
  slug: string
  inStock: boolean
}

/** Picks an in-stock product through the public API so the test does not depend on seed slugs. */
async function inStockProductSlug(page: Page): Promise<string> {
  const response = await page.request.get(`${API_URL}/api/v1/products?pageSize=50`)
  expect(response.ok()).toBeTruthy()
  const { items } = (await response.json()) as { items: ProductSummary[] }
  const product = items.find((p) => p.inStock)
  expect(product, 'need at least one in-stock product (run the dev seed)').toBeDefined()
  return product!.slug
}

async function addToCartAndCheckout(page: Page): Promise<void> {
  await page.goto(`/san-pham/${await inStockProductSlug(page)}`)
  // The button is server-rendered: a click before hydration does nothing, so retry until the cart badge updates.
  await expect(async () => {
    await page.getByRole('button', { name: 'Thêm vào giỏ' }).click()
    await expect(page.getByRole('link', { name: /Giỏ hàng, [1-9]/ })).toBeVisible({ timeout: 2_000 })
  }).toPass({ timeout: NAVIGATION_TIMEOUT })
  await page.goto('/gio-hang')
  await page
    .getByRole('link', { name: /thanh toán/i })
    .first()
    .click()
  await expect(page).toHaveURL(/\/thanh-toan/)
}

async function fillGuestCheckout(page: Page, phone: string): Promise<void> {
  await page.getByLabel('Email nhận thông báo đơn hàng').fill('e2e.guest@example.com')
  await page.locator('#ship-fullName').fill('Khách E2E')
  await page.locator('#ship-phone').fill(phone)
  await page.locator('#ship-line1').fill('12 Lê Lợi')
  await page.locator('#ship-ward').fill('Bến Nghé')
  await page.locator('#ship-district').fill('Quận 1')
  await page.locator('#ship-province').fill('TP. Hồ Chí Minh')
}

async function readOrderNumber(page: Page): Promise<string> {
  await expect(page).toHaveURL(/\/dat-hang-thanh-cong/, { timeout: NAVIGATION_TIMEOUT })
  await expect(page.getByText(ORDER_NUMBER).first()).toBeVisible()
  return (await page.locator('body').innerText()).match(ORDER_NUMBER)![0]
}

test('guest COD order goes from checkout to delivered and stays trackable', async ({ page }) => {
  const phone = '0912345678'
  await addToCartAndCheckout(page)
  await fillGuestCheckout(page, phone)
  await page.locator('input[value="Cod"]').check()
  await page.getByRole('button', { name: 'Đặt hàng', exact: true }).click()
  const orderNumber = await readOrderNumber(page)

  // Cart is emptied after a successful order.
  await page.goto('/gio-hang')
  await expect(page.getByRole('link', { name: /thanh toán/i })).toHaveCount(0)

  // Guest tracking by order number + phone.
  await page.goto('/tra-cuu-don')
  await page.getByLabel('Mã đơn hàng').fill(orderNumber)
  await page.getByLabel('Số điện thoại').fill(phone)
  await page.getByRole('button', { name: 'Tra cứu' }).click()
  await expect(page.getByText(/chờ xác nhận/i).first()).toBeVisible()

  // Admin walks the order through the state machine.
  await page.goto('/dang-nhap?returnUrl=/admin/don-hang')
  await page.getByLabel(/email/i).fill(ADMIN_EMAIL)
  await page
    .getByLabel(/mật khẩu/i)
    .first()
    .fill(ADMIN_PASSWORD)
  await page
    .getByRole('button', { name: /đăng nhập/i })
    .first()
    .click()
  await expect(page).toHaveURL(/\/admin\/don-hang/, { timeout: NAVIGATION_TIMEOUT })
  await page.getByRole('link', { name: orderNumber }).first().click()
  await expect(page.getByRole('heading', { name: new RegExp(orderNumber) })).toBeVisible({
    timeout: NAVIGATION_TIMEOUT,
  })

  for (const action of ['Xác nhận đơn', 'Bắt đầu chuẩn bị', 'Giao cho vận chuyển', 'Đã giao thành công']) {
    await page.getByRole('button', { name: action }).click()
    const dialog = page.getByRole('dialog')
    await dialog
      .getByRole('button', { name: /xác nhận/i })
      .last()
      .click()
    await expect(dialog).toBeHidden()
    await expect(page.getByRole('button', { name: action })).toHaveCount(0)
  }

  const tracked = await page.request.get(
    `${API_URL}/api/v1/orders/track?orderNumber=${orderNumber}&phone=${phone}`,
  )
  expect(((await tracked.json()) as { status: string }).status).toBe('Delivered')
})

test('guest bank-transfer order shows the VietQR code and transfer content @mobile', async ({ page }) => {
  await addToCartAndCheckout(page)
  await fillGuestCheckout(page, '0987654321')
  await page.locator('input[value="BankTransfer"]').check()
  await page.getByRole('button', { name: 'Đặt hàng và chuyển khoản' }).click()
  const orderNumber = await readOrderNumber(page)

  await expect(page.getByAltText('Mã QR chuyển khoản')).toBeVisible()
  await expect(page.getByText('Nội dung chuyển khoản')).toBeVisible()
  expect(orderNumber).toMatch(ORDER_NUMBER)
  // No horizontal scroll on phones.
  const overflow = await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth)
  expect(overflow).toBeLessThanOrEqual(0)
})
