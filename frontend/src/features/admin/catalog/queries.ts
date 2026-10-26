'use client'

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { Schemas } from '@/lib/api/types'
import {
  addVariant,
  createCategory,
  createProduct,
  deleteCategory,
  deleteProduct,
  deleteProductImage,
  getAdminProduct,
  listAdminCategories,
  listAdminProducts,
  removeVariant,
  reorderCategories,
  reorderImages,
  setPrimaryImage,
  setProductVisibility,
  updateCategory,
  updateProduct,
  updateVariant,
  uploadProductImage,
  type AdminProductFilters,
} from './api'

export const adminCatalogKeys = {
  categories: ['admin', 'categories'] as const,
  products: ['admin', 'products'] as const,
  productList: (f: AdminProductFilters) => ['admin', 'products', 'list', f] as const,
  product: (id: string) => ['admin', 'products', 'detail', id] as const,
}

export function useAdminCategoriesQuery(enabled = true) {
  return useQuery({ queryKey: adminCatalogKeys.categories, queryFn: listAdminCategories, enabled })
}

function useInvalidate(...keys: (readonly unknown[])[]) {
  const queryClient = useQueryClient()
  return () => Promise.all(keys.map((queryKey) => queryClient.invalidateQueries({ queryKey })))
}

export function useCategoryMutations() {
  const invalidate = useInvalidate(adminCatalogKeys.categories)
  return {
    create: useMutation({
      mutationFn: (body: Schemas['CreateCategoryCommand']) => createCategory(body),
      onSuccess: invalidate,
    }),
    update: useMutation({
      mutationFn: ({ id, body }: { id: string; body: Schemas['UpdateCategoryRequest'] }) =>
        updateCategory(id, body),
      onSuccess: invalidate,
    }),
    remove: useMutation({ mutationFn: (id: string) => deleteCategory(id), onSuccess: invalidate }),
    reorder: useMutation({ mutationFn: (ids: string[]) => reorderCategories(ids), onSuccess: invalidate }),
  }
}

export function useAdminProductsQuery(filters: AdminProductFilters) {
  return useQuery({
    queryKey: adminCatalogKeys.productList(filters),
    queryFn: () => listAdminProducts(filters),
    placeholderData: keepPreviousData,
  })
}

export function useAdminProductQuery(id: string) {
  return useQuery({ queryKey: adminCatalogKeys.product(id), queryFn: () => getAdminProduct(id) })
}

export function useCreateProductMutation() {
  const invalidate = useInvalidate(adminCatalogKeys.products, adminCatalogKeys.categories)
  return useMutation({
    mutationFn: (body: Schemas['CreateProductCommand']) => createProduct(body),
    onSuccess: invalidate,
  })
}

export function useSetVisibilityMutation() {
  const invalidate = useInvalidate(adminCatalogKeys.products)
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: Schemas['ProductVisibilityRequest'] }) =>
      setProductVisibility(id, body),
    onSuccess: invalidate,
  })
}

/** All mutations on a single product; each invalidates the product detail and lists. */
export function useProductMutations(id: string) {
  const invalidate = useInvalidate(
    adminCatalogKeys.products,
    ['admin', 'inventory'],
    adminCatalogKeys.categories,
  )
  return {
    update: useMutation({
      mutationFn: (body: Schemas['UpdateProductRequest']) => updateProduct(id, body),
      onSuccess: invalidate,
    }),
    visibility: useMutation({
      mutationFn: (body: Schemas['ProductVisibilityRequest']) => setProductVisibility(id, body),
      onSuccess: invalidate,
    }),
    remove: useMutation({ mutationFn: () => deleteProduct(id), onSuccess: invalidate }),
    addVariant: useMutation({
      mutationFn: (body: Schemas['AddVariantRequest']) => addVariant(id, body),
      onSuccess: invalidate,
    }),
    updateVariant: useMutation({
      mutationFn: ({ variantId, body }: { variantId: string; body: Schemas['UpdateVariantRequest'] }) =>
        updateVariant(id, variantId, body),
      onSuccess: invalidate,
    }),
    removeVariant: useMutation({
      mutationFn: (variantId: string) => removeVariant(id, variantId),
      onSuccess: invalidate,
    }),
    uploadImage: useMutation({
      mutationFn: ({ file, altText }: { file: File; altText: string | null }) =>
        uploadProductImage(id, file, altText),
      onSuccess: invalidate,
    }),
    deleteImage: useMutation({
      mutationFn: (imageId: string) => deleteProductImage(id, imageId),
      onSuccess: invalidate,
    }),
    setPrimaryImage: useMutation({
      mutationFn: (imageId: string) => setPrimaryImage(id, imageId),
      onSuccess: invalidate,
    }),
    reorderImages: useMutation({
      mutationFn: (ids: string[]) => reorderImages(id, ids),
      onSuccess: invalidate,
    }),
  }
}
