import { useEffect, useRef } from 'react'
import { Trash2, X } from 'lucide-react'

export default function DeletePedidoDialog({ pedido, busy, error, onClose, onConfirm }) {
  const dialog = useRef(null)
  useEffect(() => { dialog.current.showModal() }, [])

  return <dialog ref={dialog} onCancel={event => { if (busy) event.preventDefault(); else onClose() }} aria-labelledby="delete-title">
    <div className="dialog-heading"><span className="danger-symbol"><Trash2 size={22} /></span><button className="icon-button" title="Cerrar" aria-label="Cerrar" onClick={onClose} disabled={busy}><X size={20} /></button></div>
    <h2 id="delete-title">Eliminar pedido</h2>
    <p>El pedido <strong>{pedido.numeroPedido}</strong> de {pedido.cliente} dejara de aparecer en el listado.</p>
    {error && <p className="error" role="alert">{error}</p>}
    <div className="dialog-actions"><button onClick={onClose} disabled={busy}>Cancelar</button><button className="danger" onClick={onConfirm} disabled={busy}><Trash2 size={16} />{busy ? 'Eliminando...' : 'Eliminar pedido'}</button></div>
  </dialog>
}