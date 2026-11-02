import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Check, Package } from 'lucide-react'
import { useAuth } from '../../auth/AuthContext'
import { usePedidoForm } from '../../hooks/usePedidoForm'
import { pedidoStates } from './pedidoConfig'

export default function PedidoFormPage() {
  const { id } = useParams()
  const { session } = useAuth()
  const navigate = useNavigate()
  const { form, loading, busy, error, loadError, change, submit } = usePedidoForm(session.token, id, () => navigate('/pedidos'))

  return <>
    <Link className="back-link" to="/pedidos"><ArrowLeft size={16} /> Volver a pedidos</Link>
    <div className="section-heading"><div><span className="eyebrow">REGISTRO DE PEDIDOS</span><h1>{id ? 'Editar pedido' : 'Nuevo pedido'}<span className="heading-dot">.</span></h1><p className="muted">{id ? form.numeroPedido : 'Datos del pedido y del cliente.'}</p></div></div>
    <section className="form-section">
      <div className="form-section-title"><span className="form-icon"><Package size={21} /></span><h2>Informacion del pedido</h2></div>
      {loading ? <p role="status">Cargando pedido...</p> : <form onSubmit={submit}>
        <fieldset disabled={busy || loadError}><div className="form-grid">
          <label>Numero de pedido<input name="numeroPedido" placeholder="PED-001" value={form.numeroPedido} onChange={change} maxLength={50} required /></label>
          <label>Cliente<input name="cliente" placeholder="Nombre o razon social" value={form.cliente} onChange={change} maxLength={150} required /></label>
          <label>Fecha<input type="date" name="fecha" value={form.fecha} onChange={change} required /></label>
          <label>Total (S/)<input type="number" name="total" placeholder="0.00" value={form.total} onChange={change} min="0.01" max="99999999.99" step="0.01" required /></label>
          <label>Estado<select name="estado" value={form.estado} onChange={change}>{pedidoStates.map(state => <option key={state}>{state}</option>)}</select></label>
        </div></fieldset>
        {error && <p className="error" role="alert">{error}</p>}
        <div className="form-actions"><Link className="button" to="/pedidos">Cancelar</Link><button className="primary" disabled={busy || loadError}><Check size={17} />{busy ? 'Guardando...' : id ? 'Guardar cambios' : 'Crear pedido'}</button></div>
      </form>}
    </section>
  </>
}