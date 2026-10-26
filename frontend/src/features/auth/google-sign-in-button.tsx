'use client'

import Script from 'next/script'
import { useCallback, useEffect, useRef, useState } from 'react'
import { googleClientId } from '@/lib/env'

interface GoogleCredentialResponse {
  credential: string
}

interface GoogleIdentity {
  accounts: {
    id: {
      initialize: (options: {
        client_id: string
        callback: (response: GoogleCredentialResponse) => void
      }) => void
      renderButton: (
        element: HTMLElement,
        options: { theme?: string; size?: string; text?: string; width?: number; locale?: string },
      ) => void
    }
  }
}

declare global {
  interface Window {
    google?: GoogleIdentity
  }
}

/** Google Identity Services button. Renders nothing when NEXT_PUBLIC_GOOGLE_CLIENT_ID is empty. */
export function GoogleSignInButton({ onCredential }: { onCredential: (idToken: string) => void }) {
  const container = useRef<HTMLDivElement>(null)
  const callback = useRef(onCredential)
  const [scriptReady, setScriptReady] = useState(false)

  useEffect(() => {
    callback.current = onCredential
  }, [onCredential])

  const render = useCallback(() => {
    const google = window.google
    if (!google || !container.current) return
    google.accounts.id.initialize({
      client_id: googleClientId,
      callback: (response) => callback.current(response.credential),
    })
    google.accounts.id.renderButton(container.current, {
      theme: 'outline',
      size: 'large',
      text: 'continue_with',
      width: 320,
      locale: 'vi',
    })
  }, [])

  useEffect(() => {
    if (scriptReady || window.google) render()
  }, [scriptReady, render])

  if (!googleClientId) return null

  return (
    <>
      <Script
        src="https://accounts.google.com/gsi/client"
        strategy="afterInteractive"
        onReady={() => setScriptReady(true)}
      />
      <div ref={container} className="flex min-h-11 justify-center" />
    </>
  )
}
