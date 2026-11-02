import { useEffect, useState } from 'react'
import { pedidoPageSize } from '../pages/pedidos/pedidoConfig'
import { pedidosService } from '../services/pedidosService'

export function usePedidos(token, service = pedidosService) {
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [estado, setEstado] = useState('')
  const [page, setPage] = useState(1)
  const [data, setData] = useState({ items: [], total: 0 })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [refresh, setRefresh] = useState(0)
  const [deleting, setDeleting] = useState(null)
  const [deleteBusy, setDeleteBusy] = useState(false)
  const [deleteError, setDeleteError] = useState('')
  const [notice, setNotice] = useState('')

  useEffect(() => {
    const timer = setTimeout(() => { setDebouncedSearch(search); setPage(1) }, 300)
    return () => clearTimeout(timer)
  }, [search])

  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setError('')
    service.list(token, { search: debouncedSearch, estado, page }, controller.signal).then(result => {
      if (result.items.length === 0 && result.total > 0 && page > 1) setPage(page - 1)
      else setData(result)
    }).catch(failure => {
      if (failure.name !== 'AbortError') setError(failure.message)
    }).finally(() => {
      if (!controller.signal.aborted) setLoading(false)
    })
    return () => controller.abort()
  }, [token, debouncedSearch, estado, page, refresh, service])

  function reload() { setRefresh(value => value + 1) }
  function filterByEstado(value) { setEstado(value); setPage(1) }
  function clearFilters() { setSearch(''); setEstado(''); setPage(1) }
  function requestDelete(pedido) { setDeleteError(''); setDeleting(pedido) }
  function closeDelete() { if (!deleteBusy) setDeleting(null) }

  async function confirmDelete() {
    setDeleteBusy(true); setDeleteError('')
    try {
      await service.remove(token, deleting.id)
      setDeleting(null); setNotice('Pedido eliminado correctamente.'); reload()
    } catch (failure) { setDeleteError(failure.message) }
    finally { setDeleteBusy(false) }
  }

  return {
    search, setSearch, estado, filterByEstado, page, setPage, data, loading, error,
    pages: Math.max(1, Math.ceil(data.total / pedidoPageSize)),
    reload, clearFilters, notice, dismissNotice: () => setNotice(''),
    deleting, deleteBusy, deleteError, requestDelete, closeDelete, confirmDelete,
  }
}