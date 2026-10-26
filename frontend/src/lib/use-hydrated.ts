import { useSyncExternalStore } from 'react'

const subscribe = () => () => {}

/** False during SSR and hydration, true afterwards. Use before rendering localStorage-backed state. */
export function useHydrated(): boolean {
  return useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  )
}
