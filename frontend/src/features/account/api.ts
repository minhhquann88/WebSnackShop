import { api, unwrap } from '@/lib/api/client'
import type { Schemas } from '@/lib/api/types'

export const getMe = () => unwrap(api.GET('/api/v1/me'))

export const updateProfile = (body: Schemas['UpdateProfileCommand']) =>
  unwrap(api.PUT('/api/v1/me', { body }))

export const changePassword = (body: Schemas['ChangePasswordCommand']) =>
  unwrap(api.PUT('/api/v1/me/password', { body }))

export const listAddresses = () => unwrap(api.GET('/api/v1/me/addresses'))

export const addAddress = (body: Schemas['SaveAddressRequest']) =>
  unwrap(api.POST('/api/v1/me/addresses', { body }))

export const updateAddress = (id: string, body: Schemas['SaveAddressRequest']) =>
  unwrap(api.PUT('/api/v1/me/addresses/{id}', { params: { path: { id } }, body }))

export const deleteAddress = (id: string) =>
  unwrap(api.DELETE('/api/v1/me/addresses/{id}', { params: { path: { id } } }))

export const setDefaultAddress = (id: string) =>
  unwrap(api.POST('/api/v1/me/addresses/{id}/default', { params: { path: { id } } }))

export const forgotPassword = (email: string) =>
  unwrap(api.POST('/api/v1/auth/forgot-password', { body: { email } }))

export const resetPassword = (body: Schemas['ResetPasswordCommand']) =>
  unwrap(api.POST('/api/v1/auth/reset-password', { body }))

export const confirmEmail = (body: Schemas['ConfirmEmailCommand']) =>
  unwrap(api.POST('/api/v1/auth/confirm-email', { body }))
