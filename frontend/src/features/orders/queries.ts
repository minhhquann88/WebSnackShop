'use client'

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { cancelMyOrder, getMyOrder, listMyOrders, placeOrder, trackOrder, type PlaceOrderInput } from './api'

export const orderKeys = {
  mine: ['me', 'orders'] as const,
  myList: (page: number) => ['me', 'orders', 'list', page] as const,
  myDetail: (orderNumber: string) => ['me', 'orders', 'detail', orderNumber] as const,
  track: (orderNumber: string, phone: string) => ['orders', 'track', orderNumber, phone] as const,
}

export function useMyOrdersQuery(page: number) {
  return useQuery({
    queryKey: orderKeys.myList(page),
    queryFn: () => listMyOrders(page),
    placeholderData: keepPreviousData,
  })
}

export function useMyOrderQuery(orderNumber: string) {
  return useQuery({ queryKey: orderKeys.myDetail(orderNumber), queryFn: () => getMyOrder(orderNumber) })
}

export function useCancelMyOrderMutation(orderNumber: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string | null) => cancelMyOrder(orderNumber, reason),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: orderKeys.mine }),
  })
}

export function useTrackOrderQuery(orderNumber: string, phone: string) {
  return useQuery({
    queryKey: orderKeys.track(orderNumber, phone),
    queryFn: () => trackOrder(orderNumber, phone),
    enabled: orderNumber.length > 0 && phone.length > 0,
  })
}

export function usePlaceOrderMutation() {
  return useMutation({
    mutationFn: ({ body, idempotencyKey }: { body: PlaceOrderInput; idempotencyKey: string }) =>
      placeOrder(body, idempotencyKey),
  })
}
