import { useState } from 'react'
import { BrowserRouter, Link, Navigate, Outlet, Route, Routes } from 'react-router-dom'
import { ArrowRight, ClipboardList, LogOut, Package, ShieldCheck } from 'lucide-react'
import { api } from './services/api'
import { AuthProvider } from './auth/AuthProvider'
import { useAuth } from './auth/AuthContext.js'
import PedidosPage from './pages/pedidos/PedidosPage.jsx'
import PedidoFormPage from './pages/pedidos/PedidoFormPage.jsx'
import './App.css'

function Brand() {
  return <Link className="brand" to="/pedidos" aria-label="Atlantic City, inicio"><span className="brand-symbol"><Package size={23} /></span><span>ATLANTIC CITY<small>OPERACIONES</small></span></Link>
}

function Login() {
  const { session, setSession, message, setMessage } = useAuth()
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  if (session) return <Navigate to="/pedidos" replace />
  async function submit(event) {
    event.preventDefault()
    const data = new FormData(event.currentTarget)
    setBusy(true); setError('')
    try {
      const result = await api(null, '/auth/login', { method: 'POST', body: { email: data.get('email'), password: data.get('password') } })
      setMessage(''); setSession({ ...result, expiresAt: Date.now() + result.expiresIn * 1000 })
    } catch (failure) { setError(failure.message) }
    finally { setBusy(false) }
  }
  return <main className="login-page">
    <div className="login-top"><Brand /><span className="secure-label"><ShieldCheck size={16} /> Acceso seguro</span></div>
    <div className="login-content">
      <section className="login-form"><span className="eyebrow">PORTAL INTERNO</span><h1>Bienvenido de nuevo.</h1><p className="muted">Ingresa a tu cuenta de operaciones.</p>
        {message && <p className="notice" role="status">{message}</p>}
        <form onSubmit={submit}>
          <label>Correo electronico<input name="email" type="email" autoComplete="username" placeholder="nombre@empresa.com" required maxLength={200} /></label>
          <label>Contrasena<input name="password" type="password" autoComplete="current-password" placeholder="Tu contrasena" required maxLength={200} /></label>
          {error && <p className="error" role="alert">{error}</p>}
          <button className="primary login-submit" disabled={busy}>{busy ? 'Ingresando...' : 'Iniciar sesion'}<ArrowRight size={18} /></button>
        </form>
        <p className="login-footer"><ShieldCheck size={15} /> Solo personal autorizado</p>
      </section>
      <div className="login-image"><img src="https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=1400&q=85" alt="Arquitectura de un edificio de oficinas" /><div><span className="eyebrow">ATLANTIC CITY</span><h2>Cada pedido cuenta.</h2><p>Equipo de operaciones</p></div></div>
    </div><footer className="page-footer">Atlantic City <span>Gestion de pedidos / 2026</span></footer>
  </main>
}

function Layout() {
  const { session, setSession } = useAuth()
  if (!session) return <Navigate to="/login" replace />
  return <div className="app-shell"><aside className="sidebar"><Brand /><span className="nav-caption">ESPACIO DE TRABAJO</span><nav><Link className="nav-active" to="/pedidos"><ClipboardList size={19} /> Pedidos <span className="nav-dot" /></Link></nav><div className="sidebar-bottom"><span className="avatar">{session.email[0].toUpperCase()}</span><div><strong>{session.rol === 'Admin' ? 'Administrador' : 'Usuario'}</strong><small title={session.email}>{session.email}</small></div><button className="icon-button" title="Cerrar sesion" aria-label="Cerrar sesion" onClick={() => setSession(null)}><LogOut size={18} /></button></div></aside><div className="workspace"><header className="topbar"><span>Operaciones <span className="crumb">/</span> <strong>Pedidos</strong></span><span className="secure-label"><span className="online-dot" /> Sesion activa</span></header><main className="main-content"><Outlet /></main><footer className="page-footer">Atlantic City <span>Centro de operaciones</span></footer></div></div>
}

export default function App() {
  return <BrowserRouter><AuthProvider><Routes><Route path="/login" element={<Login />} /><Route element={<Layout />}><Route path="/pedidos" element={<PedidosPage />} /><Route path="/pedidos/nuevo" element={<PedidoFormPage key="new" />} /><Route path="/pedidos/:id/editar" element={<PedidoFormPage key="edit" />} /></Route><Route path="*" element={<Navigate to="/pedidos" replace />} /></Routes></AuthProvider></BrowserRouter>
}