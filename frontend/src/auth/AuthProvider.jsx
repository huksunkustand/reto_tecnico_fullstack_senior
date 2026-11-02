import { useEffect, useState } from 'react'
import { AuthContext } from './AuthContext'

export function AuthProvider({ children }) {
  const [session, setSession] = useState(null)
  const [message, setMessage] = useState('')
  useEffect(() => {
    function expire() { setSession(null); setMessage('Tu sesion termino. Ingresa nuevamente.') }
    window.addEventListener('session-expired', expire)
    const timer = session ? setTimeout(expire, Math.max(0, session.expiresAt - Date.now())) : null
    return () => { window.removeEventListener('session-expired', expire); clearTimeout(timer) }
  }, [session])
  return <AuthContext.Provider value={{ session, setSession, message, setMessage }}>{children}</AuthContext.Provider>
}