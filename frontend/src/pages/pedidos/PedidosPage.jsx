import { Link } from 'react-router-dom'
import { Check, ChevronLeft, ChevronRight, ClipboardList, Package, Pencil, Plus, RefreshCw, Search, Trash2, X } from 'lucide-react'
import { useAuth } from '../../auth/AuthContext'
import DeletePedidoDialog from '../../components/DeletePedidoDialog'
import { usePedidos } from '../../hooks/usePedidos'
import { currency, dateFormat, pedidoPageSize, pedidoStates } from './pedidoConfig'

export default function PedidosPage() {
  const { session } = useAuth()
  const {
    search, setSearch, estado, filterByEstado, page, setPage, data, loading, error, pages,
    reload, clearFilters, notice, dismissNotice,
    deleting, deleteBusy, deleteError, requestDelete, closeDelete, confirmDelete,
  } = usePedidos(session.token)

  return <>
    <div className="section-heading"><div><span className="eyebrow">OPERACIONES</span><h1>Pedidos<span className="heading-dot">.</span></h1><p className="muted">Control y seguimiento de tus pedidos.</p></div><Link className="button primary" to="/pedidos/nuevo"><Plus size={18} /> Nuevo pedido</Link></div>
    <div className="summary-strip"><div><span>Total de resultados</span><strong>{loading ? '--' : data.total.toLocaleString('es-PE')}</strong></div><div><span>Vista actual</span><strong className="summary-text">{estado || 'Todos los estados'}</strong></div><div className="summary-end"><Package size={28} strokeWidth={1.4} /><span>Registro de<br /><strong>pedidos</strong></span></div></div>
    {notice && <p className="success" role="status"><Check size={17} /> {notice}<button className="icon-button" title="Cerrar aviso" aria-label="Cerrar aviso" onClick={dismissNotice}><X size={16} /></button></p>}
    <section className="orders-section" aria-label="Listado de pedidos">
      <div className="list-title"><h2>Todos los pedidos <span>{data.total}</span></h2><button className="icon-button" title="Actualizar pedidos" aria-label="Actualizar pedidos" onClick={reload} disabled={loading}><RefreshCw size={17} className={loading ? 'spinning' : ''} /></button></div>
      <div className="filters">
        <label className="search-box"><Search size={18} /><input aria-label="Buscar por numero o cliente" placeholder="Buscar por numero o cliente..." value={search} maxLength={150} onChange={event => setSearch(event.target.value)} />{search && <button className="icon-button" aria-label="Limpiar busqueda" title="Limpiar busqueda" onClick={() => setSearch('')}><X size={16} /></button>}</label>
        <select aria-label="Filtrar por estado" value={estado} onChange={event => filterByEstado(event.target.value)}><option value="">Todos los estados</option>{pedidoStates.map(state => <option key={state}>{state}</option>)}</select>
      </div>
      {error ? <div className="empty-state" role="alert"><p className="error">{error}</p><button onClick={reload}><RefreshCw size={16} /> Reintentar</button></div> : <div className="table-scroll" aria-busy={loading}>
        <table>
          <thead><tr><th>NUMERO DE PEDIDO</th><th>CLIENTE</th><th>FECHA</th><th>TOTAL</th><th>ESTADO</th><th><span className="sr-only">Acciones</span></th></tr></thead>
          <tbody>{loading ? <tr><td colSpan="6" className="table-message">Cargando pedidos...</td></tr> : data.items.map(pedido => <tr key={pedido.id}>
            <td><Link className="order-number" to={`/pedidos/${pedido.id}/editar`}>{pedido.numeroPedido}</Link></td>
            <td className="client-cell">{pedido.cliente}</td>
            <td className="date-cell">{dateFormat.format(new Date(`${pedido.fecha}T12:00:00`))}</td>
            <td className="money">{currency.format(pedido.total)}</td>
            <td><span className={`status status-${pedidoStates.indexOf(pedido.estado)}`}><span />{pedido.estado}</span></td>
            <td><div className="row-actions"><Link className="icon-button" to={`/pedidos/${pedido.id}/editar`} title={`Editar ${pedido.numeroPedido}`} aria-label={`Editar ${pedido.numeroPedido}`}><Pencil size={16} /></Link><button className="icon-button delete-action" title={`Eliminar ${pedido.numeroPedido}`} aria-label={`Eliminar ${pedido.numeroPedido}`} onClick={() => requestDelete(pedido)}><Trash2 size={16} /></button></div></td>
          </tr>)}</tbody>
        </table>
        {!loading && !data.items.length && <div className="empty-state"><ClipboardList size={36} strokeWidth={1.3} /><h3>{search || estado ? 'No hay coincidencias' : 'Aun no hay pedidos'}</h3>{search || estado ? <button onClick={clearFilters}>Limpiar filtros</button> : <Link className="button primary" to="/pedidos/nuevo"><Plus size={16} /> Crear pedido</Link>}</div>}
      </div>}
      <div className="pagination"><span>{loading ? 'Cargando...' : `${data.total ? (page - 1) * pedidoPageSize + 1 : 0}-${Math.min(page * pedidoPageSize, data.total)} de ${data.total} pedidos`}</span><div><button className="icon-button" title="Pagina anterior" aria-label="Pagina anterior" disabled={page === 1 || loading} onClick={() => setPage(page - 1)}><ChevronLeft size={18} /></button><span>Pagina {page} de {pages}</span><button className="icon-button" title="Pagina siguiente" aria-label="Pagina siguiente" disabled={page >= pages || loading} onClick={() => setPage(page + 1)}><ChevronRight size={18} /></button></div></div>
    </section>
    {deleting && <DeletePedidoDialog pedido={deleting} busy={deleteBusy} error={deleteError} onClose={closeDelete} onConfirm={confirmDelete} />}
  </>
}