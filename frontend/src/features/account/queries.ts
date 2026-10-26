'use client'

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/lib/auth/auth.store'
import type { Schemas } from '@/lib/api/types'
import {
  addAddress,
  changePassword,
  deleteAddress,
  getMe,
  listAddresses,
  setDefaultAddress,
  updateAddress,
  updateProfile,
} from './api'

export const accountKeys = {
  me: ['me', 'profile'] as const,
  addresses: ['me', 'addresses'] as const,
}

export function useMeQuery() {
  return useQuery({ queryKey: accountKeys.me, queryFn: getMe })
}

export function useUpdateProfileMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: updateProfile,
    onSuccess: async () => {
      const me = await queryClient.fetchQuery({ queryKey: accountKeys.me, queryFn: getMe })
      useAuthStore.getState().setUser(me)
    },
  })
}

export function useChangePasswordMutation() {
  return useMutation({ mutationFn: changePassword })
}

export function useAddressesQuery(enabled = true) {
  return useQuery({ queryKey: accountKeys.addresses, queryFn: listAddresses, enabled })
}

export function useSaveAddressMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: string | null; body: Schemas['SaveAddressRequest'] }) =>
      id ? updateAddress(id, body) : addAddress(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: accountKeys.addresses }),
  })
}

export function useDeleteAddressMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: deleteAddress,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: accountKeys.addresses }),
  })
}

export function useSetDefaultAddressMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: setDefaultAddress,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: accountKeys.addresses }),
  })
}
