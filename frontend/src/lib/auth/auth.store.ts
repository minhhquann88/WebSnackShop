import { create } from 'zustand'
import type { User } from '@/lib/api/types'

export type SessionStatus = 'loading' | 'authenticated' | 'anonymous'

/** What the BFF (`/api/auth/*`) returns to the browser. The refresh token never leaves the httpOnly cookie. */
export interface BrowserSession {
  accessToken: string
  accessTokenExpiresAt: string
  user: User
}

interface AuthState {
  status: SessionStatus
  accessToken: string | null
  /** Epoch ms. */
  expiresAt: number
  user: User | null
  setSession: (session: BrowserSession) => void
  setUser: (user: User) => void
  clear: () => void
}

/** In-memory only (not persisted): the access token is lost on reload and restored through the BFF refresh route. */
export const useAuthStore = create<AuthState>()((set) => ({
  status: 'loading',
  accessToken: null,
  expiresAt: 0,
  user: null,
  setSession: (session) =>
    set({
      status: 'authenticated',
      accessToken: session.accessToken,
      expiresAt: new Date(session.accessTokenExpiresAt).getTime(),
      user: session.user,
    }),
  setUser: (user) => set({ user }),
  clear: () => set({ status: 'anonymous', accessToken: null, expiresAt: 0, user: null }),
}))

export function hasRole(user: User | null | undefined, ...roles: string[]): boolean {
  return !!user && user.roles.some((r) => roles.includes(r))
}

export const isStaff = (user: User | null | undefined): boolean => hasRole(user, 'Staff', 'Admin')
export const isAdmin = (user: User | null | undefined): boolean => hasRole(user, 'Admin')
