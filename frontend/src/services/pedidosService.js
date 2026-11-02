import { api } from './api'
import { pedidoPageSize } from '../pages/pedidos/pedidoConfig'

export function listPedidos(token, { search, estado, page }, signal) {
  const query = new URLSearchParams({ search, estado, page: String(page), pageSize: String(pedidoPageSize) })
  return api(token, `/api/pedidos?${query}`, { signal })
}

export function getPedido(token, id, signal) {
  return api(token, `/api/pedidos/${id}`, { signal })
}

export function createPedido(token, pedido) {
  return api(token, '/api/pedidos', { method: 'POST', body: pedido })
}

export function updatePedido(token, id, pedido) {
  return api(token, `/api/pedidos/${id}`, { method: 'PUT', body: pedido })
}

export function deletePedido(token, id) {
  return api(token, `/api/pedidos/${id}`, { method: 'DELETE' })
}

export const pedidosService = {
  list: listPedidos,
  get: getPedido,
  create: createPedido,
  update: updatePedido,
  remove: deletePedido,
}