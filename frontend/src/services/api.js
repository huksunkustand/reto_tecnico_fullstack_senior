const baseUrl = (import.meta.env?.VITE_API_URL || '').replace(/\/$/, '')

export async function api(token, path, { body, headers, ...options } = {}) {
  const requestHeaders = new Headers(headers)
  if (token) requestHeaders.set('Authorization', `Bearer ${token}`)
  if (body !== undefined) requestHeaders.set('Content-Type', 'application/json')

  const response = await fetch(`${baseUrl}${path}`, {
    ...options,
    headers: requestHeaders,
    ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
  })
  const text = await response.text()
  let data = null
  if (text) {
    try { data = JSON.parse(text) }
    catch { data = { message: text } }
  }

  if (!response.ok) {
    if (response.status === 401 && token) window.dispatchEvent(new Event('session-expired'))
    const validationErrors = data?.errors ? Object.values(data.errors).flat().join(' ') : ''
    throw new Error(validationErrors || data?.message || data?.detail || data?.title || `Error HTTP ${response.status}`)
  }

  return data
}