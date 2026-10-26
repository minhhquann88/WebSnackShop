'use client'

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { Schemas } from '@/lib/api/types'
import {
  createStaff,
  getCustomer,
  listCustomers,
  listUsers,
  setUserLock,
  setUserRoles,
  type PagedSearch,
} from './api'

export const adminPeopleKeys = {
  customers: (f: PagedSearch) => ['admin', 'customers', 'list', f] as const,
  customer: (id: string) => ['admin', 'customers', 'detail', id] as const,
  usersAll: ['admin', 'users'] as const,
  users: (f: PagedSearch & { role?: string }) => ['admin', 'users', 'list', f] as const,
}

export function useCustomersQuery(filters: PagedSearch) {
  return useQuery({
    queryKey: adminPeopleKeys.customers(filters),
    queryFn: () => listCustomers(filters),
    placeholderData: keepPreviousData,
  })
}

export function useCustomerQuery(id: string) {
  return useQuery({ queryKey: adminPeopleKeys.customer(id), queryFn: () => getCustomer(id) })
}

export function useUsersQuery(filters: PagedSearch & { role?: string }, enabled = true) {
  return useQuery({
    queryKey: adminPeopleKeys.users(filters),
    queryFn: () => listUsers(filters),
    placeholderData: keepPreviousData,
    enabled,
  })
}

export function useUserMutations() {
  const queryClient = useQueryClient()
  const invalidate = () => queryClient.invalidateQueries({ queryKey: adminPeopleKeys.usersAll })
  return {
    create: useMutation({
      mutationFn: (body: Schemas['CreateStaffCommand']) => createStaff(body),
      onSuccess: invalidate,
    }),
    setRoles: useMutation({
      mutationFn: ({ id, roles }: { id: string; roles: string[] }) => setUserRoles(id, roles),
      onSuccess: invalidate,
    }),
    setLock: useMutation({
      mutationFn: ({ id, locked }: { id: string; locked: boolean }) => setUserLock(id, locked),
      onSuccess: invalidate,
    }),
  }
}
