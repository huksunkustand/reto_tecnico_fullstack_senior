# Frontend - Gestion de Pedidos

Aplicacion React con Vite. Requiere Node.js 20.19+ o 22.12+; se recomienda Node.js 24 LTS.

## Desarrollo

```sh
npm install
npm run dev
```

Vite publica el frontend en http://127.0.0.1:5173 y redirige `/auth` y `/api` al backend en http://localhost:5080. La variable `VITE_API_URL` permite definir otra URL de API.

## Estructura

```text
src/
	App.jsx                         # Rutas, login y layout de la aplicacion
	App.css                         # Estilos existentes de la interfaz
	auth/
		AuthContext.js                # Contexto y acceso a la sesion
		AuthProvider.jsx              # Estado y expiracion de la sesion
	services/
		api.js                        # Cliente HTTP compartido y errores de sesion
		pedidosService.js             # Operaciones HTTP de Pedidos
	components/
		DeletePedidoDialog.jsx         # Confirmacion visual de eliminacion
	hooks/
		usePedidos.js                  # Consulta, filtros y flujo de eliminacion
		usePedidoForm.js               # Carga, estado y guardado del formulario
	pages/
		pedidos/
			pedidoConfig.js             # Estados, paginacion y formatos
			PedidosPage.jsx             # Listado, filtros y paginacion
			PedidoFormPage.jsx          # Pantalla de creacion y edicion
```

## Mantenimiento y SOLID

- Responsabilidad unica: las paginas presentan la interfaz, los hooks coordinan el flujo y el servicio conoce los endpoints. El dialogo solo recibe datos y callbacks; no accede a HTTP ni a la sesion.
- Dependencias sustituibles: los hooks reciben el token y admiten un servicio opcional. Se puede inyectar una implementacion de prueba sin modificar las paginas ni el cliente HTTP.
- Contratos pequenos: el listado utiliza `list` y `remove`; el formulario utiliza `get`, `create` y `update`. Las implementaciones sustitutas deben conservar argumentos, promesas y errores del servicio original.
- Para modificar campos del formulario, revisar `PedidoFormPage.jsx` y `usePedidoForm.js`; para cambiar endpoints, revisar `pedidosService.js`; para estados o tamano de pagina, revisar `pedidoConfig.js`.
- Las pantallas de cada modulo deben tener su propia carpeta en `pages/`, sin importar codigo de `App.jsx` ni de otras funcionalidades. Los hooks, componentes y servicios permanecen en sus respectivas carpetas de `src/`.

Las rutas siguen siendo `/pedidos`, `/pedidos/nuevo` y `/pedidos/:id/editar`. Los cambios de organizacion no alteran el contrato del backend ni los estilos existentes.

## Verificacion

```sh
npm run lint
npm run build
```
