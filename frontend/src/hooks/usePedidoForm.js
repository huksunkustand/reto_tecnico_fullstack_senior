import { useEffect, useState } from 'react'
import { pedidosService } from '../services/pedidosService'

export function usePedidoForm(token, id, onSaved, service = pedidosService) {
  const [form, setForm] = useState(() => ({ numeroPedido: '', cliente: '', fecha: new Date().toLocaleDateString('en-CA'), total: '', estado: 'Registrado' }))
  const [loading, setLoading] = useState(Boolean(id))
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [loadError, setLoadError] = useState(false)

  useEffect(() => {
    if (!id) return
    const controller = new AbortController()
    service.get(token, id, controller.signal).then(setForm).catch(failure => {
      if (failure.name !== 'AbortError') { setError(failure.message); setLoadError(true) }
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [id, token, service])

  function change(event) {
    const { name, value } = event.target
    setForm(current => ({ ...current, [name]: value }))
  }

  async function submit(event) {
    event.preventDefault(); setBusy(true); setError('')
    try {
      const pedido = { ...form, total: Number(form.total) }
      if (id) await service.update(token, id, pedido)
      else await service.create(token, pedido)
      onSaved()
    } catch (failure) { setError(failure.message) }
    finally { setBusy(false) }
  }

  return { form, loading, busy, error, loadError, change, submit }
}