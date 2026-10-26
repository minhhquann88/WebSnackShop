import { api, unwrap } from '@/lib/api/client'
import type { Schemas } from '@/lib/api/types'

export interface PagedSearch {
  search?: string
  page: number
  pageSize: number
}

// ---- Customers ------------------------------------------------------------
export const listCustomers = (f: PagedSearch) =>
  unwrap(
    api.GET('/api/v1/admin/customers', {
      params: { query: { search: f.search || undefined, page: f.page, pageSize: f.pageSize } },
    }),
  )

export const getCustomer = (id: string) =>
  unwrap(api.GET('/api/v1/admin/customers/{id}', { params: { path: { id } } }))

// ---- Users (staff management) ---------------------------------------------
export const listUsers = (f: PagedSearch & { role?: string }) =>
  unwrap(
    api.GET('/api/v1/admin/users', {
      params: {
        query: {
          role: f.role || undefined,
          search: f.search || undefined,
          page: f.page,
          pageSize: f.pageSize,
        },
      },
    }),
  )

export const createStaff = (body: Schemas['CreateStaffCommand']) =>
  unwrap(api.POST('/api/v1/admin/users', { body }))

export const setUserRoles = (id: string, roles: string[]) =>
  unwrap(api.PUT('/api/v1/admin/users/{id}/roles', { params: { path: { id } }, body: { roles } }))

export const setUserLock = (id: string, locked: boolean) =>
  unwrap(api.POST('/api/v1/admin/users/{id}/lock', { params: { path: { id } }, body: { locked } }))
