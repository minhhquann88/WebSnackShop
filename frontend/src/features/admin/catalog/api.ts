import { api, unwrap } from '@/lib/api/client'
import type { Schemas } from '@/lib/api/types'

export interface AdminProductFilters {
  search?: string
  categoryId?: string
  isActive?: boolean
  page: number
  pageSize: number
}

// ---- Categories -----------------------------------------------------------
export const listAdminCategories = () => unwrap(api.GET('/api/v1/admin/categories'))

export const createCategory = (body: Schemas['CreateCategoryCommand']) =>
  unwrap(api.POST('/api/v1/admin/categories', { body }))

export const updateCategory = (id: string, body: Schemas['UpdateCategoryRequest']) =>
  unwrap(api.PUT('/api/v1/admin/categories/{id}', { params: { path: { id } }, body }))

export const deleteCategory = (id: string) =>
  unwrap(api.DELETE('/api/v1/admin/categories/{id}', { params: { path: { id } } }))

export const reorderCategories = (ids: string[]) =>
  unwrap(api.PUT('/api/v1/admin/categories/order', { body: { ids } }))

// ---- Products -------------------------------------------------------------
export const listAdminProducts = (f: AdminProductFilters) =>
  unwrap(
    api.GET('/api/v1/admin/products', {
      params: {
        query: {
          search: f.search || undefined,
          categoryId: f.categoryId || undefined,
          isActive: f.isActive,
          page: f.page,
          pageSize: f.pageSize,
        },
      },
    }),
  )

export const getAdminProduct = (id: string) =>
  unwrap(api.GET('/api/v1/admin/products/{id}', { params: { path: { id } } }))

export const createProduct = (body: Schemas['CreateProductCommand']) =>
  unwrap(api.POST('/api/v1/admin/products', { body }))

export const updateProduct = (id: string, body: Schemas['UpdateProductRequest']) =>
  unwrap(api.PUT('/api/v1/admin/products/{id}', { params: { path: { id } }, body }))

export const deleteProduct = (id: string) =>
  unwrap(api.DELETE('/api/v1/admin/products/{id}', { params: { path: { id } } }))

export const setProductVisibility = (id: string, body: Schemas['ProductVisibilityRequest']) =>
  unwrap(api.PATCH('/api/v1/admin/products/{id}/status', { params: { path: { id } }, body }))

// ---- Variants -------------------------------------------------------------
export const addVariant = (id: string, body: Schemas['AddVariantRequest']) =>
  unwrap(api.POST('/api/v1/admin/products/{id}/variants', { params: { path: { id } }, body }))

export const updateVariant = (id: string, variantId: string, body: Schemas['UpdateVariantRequest']) =>
  unwrap(
    api.PUT('/api/v1/admin/products/{id}/variants/{variantId}', {
      params: { path: { id, variantId } },
      body,
    }),
  )

export const removeVariant = (id: string, variantId: string) =>
  unwrap(
    api.DELETE('/api/v1/admin/products/{id}/variants/{variantId}', { params: { path: { id, variantId } } }),
  )

// ---- Images ---------------------------------------------------------------
/**
 * Multipart upload. openapi-fetch passes FormData bodies through untouched and sets no
 * Content-Type, so the browser adds the multipart boundary itself.
 */
export async function uploadProductImage(id: string, file: File, altText: string | null) {
  const form = new FormData()
  form.append('file', file)
  if (altText) form.append('altText', altText)
  return unwrap(
    api.POST('/api/v1/admin/products/{id}/images', {
      params: { path: { id } },
      body: form as never,
    }),
  )
}

export const deleteProductImage = (id: string, imageId: string) =>
  unwrap(api.DELETE('/api/v1/admin/products/{id}/images/{imageId}', { params: { path: { id, imageId } } }))

export const setPrimaryImage = (id: string, imageId: string) =>
  unwrap(
    api.POST('/api/v1/admin/products/{id}/images/{imageId}/primary', { params: { path: { id, imageId } } }),
  )

export const reorderImages = (id: string, ids: string[]) =>
  unwrap(api.PUT('/api/v1/admin/products/{id}/images/order', { params: { path: { id } }, body: { ids } }))
