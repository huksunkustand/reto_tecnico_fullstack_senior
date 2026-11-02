# Atlantic City | Gestion de Pedidos

Aplicacion full-stack para consultar y administrar pedidos. Incluye una API REST con autenticacion JWT, una interfaz web React y una base de datos SQL Server.

## Funcionalidades

- Inicio de sesion con correo y contrasena.
- Listado paginado de pedidos con busqueda y filtro por estado.
- Creacion, edicion y eliminacion logica de pedidos.
- Validacion de datos y control de concurrencia al editar.
- Registro de eventos de la API en consola y archivos JSON.

## Tecnologias

- **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core y SQL Server.
- **Frontend:** React 19, React Router y Vite.
- **Pruebas:** xUnit y Moq.
- **Base de datos:** SQL Server, con scripts SQL incluidos.

## Estructura

```text
backend/
	src/
		Pedidos.Api/             API, controladores y configuracion
		Pedidos.Application/     Servicios y contratos de aplicacion
		Pedidos.Domain/          Entidades y reglas del dominio
		Pedidos.Infrastructure/  Persistencia y repositorios
	tests/
		Pedidos.Tests/           Pruebas automatizadas
	Pedidos.sln
database/                     Scripts de base de datos
frontend/                     Aplicacion React con Vite
scripts/                      Scripts de inicio para PowerShell
```

## Requisitos

- .NET 8 SDK.
- SQL Server y SSMS o Azure Data Studio.
- Node.js 20.19+ o 22.12+; se recomienda Node.js 24 LTS.
- PowerShell para utilizar los scripts de inicio incluidos.

## Preparar la base de datos

Ejecuta los scripts desde SSMS o Azure Data Studio, en este orden:

| Orden | Script | Descripcion |
| --- | --- | --- |
| 1 | [`database/01-create-database.sql`](database/01-create-database.sql) | Crea la base `AtlanticPedidos`. |
| 2 | [`database/02-schema.sql`](database/02-schema.sql) | Crea tablas, restricciones e historial de migraciones. |
| 3 | [`database/03-seed-pedidos.sql`](database/03-seed-pedidos.sql) | Inserta pedidos de ejemplo. Es opcional. |
| 4 | [`database/05-create-admin.sql`](database/05-create-admin.sql) | Crea el administrador de prueba para iniciar sesion. |

`database/04-queries.sql` contiene consultas y ejemplos CRUD; no es necesario ejecutarlo para iniciar la aplicacion. Para detalles sobre usuarios y contrasenas, consulta [`database/README.md`](database/README.md).

## Configurar y ejecutar

Sigue estos pasos desde la raiz del repositorio. Antes de continuar, asegúrate de tener SQL Server iniciado y de haber ejecutado `database/01-create-database.sql` y `database/02-schema.sql`. Para iniciar sesión con un usuario creado mediante el script SQL, ejecuta también `database/05-create-admin.sql`.

1. Configura la cadena de conexión a tu instancia de SQL Server. Este ejemplo usa autenticación integrada de Windows; ajusta el servidor y las opciones de conexión según tu entorno:

```powershell
$env:ConnectionStrings__SqlServer = "Server=localhost;Database=AtlanticPedidos;Trusted_Connection=True;TrustServerCertificate=True"
```

2. En una terminal de PowerShell, inicia el backend:

```powershell
.\scripts\start-backend.ps1
```

La API queda disponible en `http://localhost:5080`. El script genera una clave JWT local si no existe `Jwt__Key`.

3. En una segunda terminal, instala las dependencias e inicia el frontend:

```powershell
cd frontend
npm install
cd ..
.\scripts\start-frontend.ps1
```

La aplicación web queda disponible en `http://localhost:5173`. Vite redirige las solicitudes a `/auth` y `/api` al backend local. Para usar otra URL de API, configura `VITE_API_URL` antes de iniciar Vite.

### Migraciones y administrador inicial

Por defecto, `Database:AutoMigrate` esta desactivado. En este modo, crea el esquema con los scripts SQL y registra un administrador mediante `database/05-create-admin.sql` antes de iniciar sesion.

Para desarrollo tambien puedes habilitar las migraciones automaticas y crear el primer administrador cuando la tabla de usuarios este vacia. La contrasena debe tener al menos 12 caracteres:

```powershell
$env:Database__AutoMigrate = "true"
$env:Seed__Email = "admin@atlantic.local"
$env:Seed__Password = "DefineUnaClaveLocalSegura"
.\scripts\start-backend.ps1
```

No reutilices las credenciales de desarrollo en produccion ni guardes secretos reales en el repositorio. Encontraras mas opciones de configuracion en [`database/README.md`](database/README.md).

## API

Swagger esta habilitado en desarrollo: `http://localhost:5080/swagger`. El endpoint de estado es `http://localhost:5080/health`.

| Metodo | Ruta | Descripcion | Autenticacion |
| --- | --- | --- | --- |
| `POST` | `/auth/login` | Inicia sesion y devuelve un JWT. | No |
| `GET` | `/api/pedidos` | Lista pedidos; acepta `search`, `estado`, `page` y `pageSize`. | Si |
| `GET` | `/api/pedidos/{id}` | Obtiene un pedido. | Si |
| `POST` | `/api/pedidos` | Crea un pedido. | Si |
| `PUT` | `/api/pedidos/{id}` | Actualiza un pedido. | Si |
| `DELETE` | `/api/pedidos/{id}` | Elimina logicamente un pedido. | Si |

La paginación comienza en `page=1` y `pageSize` admite valores de 1 a 100. Los estados permitidos son `Registrado`, `En proceso`, `Completado` y `Cancelado`. Para editar, envía la propiedad `version` devuelta por la API para detectar cambios concurrentes.

### Probar la API con Swagger

Swagger UI funciona como alternativa interactiva a una colección de Postman: permite enviar solicitudes, completar parámetros y consultar respuestas sin instalar otra herramienta.

1. Inicia el backend y abre `http://localhost:5080/swagger`.
2. Abre `POST /auth/login`, selecciona **Try it out** y envía las credenciales de un usuario existente:

```json
{
	"email": "admin@atlantic.local",
	"password": "LaClaveConfiguradaParaEsteUsuario"
}
```

3. Ejecuta la solicitud y copia el valor `token` de la respuesta. Pulsa **Authorize** en Swagger UI y pega el token en el campo Bearer. Confirma la autorización y cierra el diálogo. Swagger adjuntará el JWT a las operaciones protegidas.
4. Ya puedes ejecutar los endpoints de pedidos. Para crear uno, abre `POST /api/pedidos` y envía:

```json
{
	"numeroPedido": "PED-10001",
	"cliente": "Cliente de prueba",
	"fecha": "2026-11-02",
	"total": 1250.50,
	"estado": "Registrado"
}
```

5. Para editar, primero ejecuta `GET /api/pedidos/{id}` y copia la propiedad `version` de la respuesta. Inclúyela en el cuerpo de `PUT /api/pedidos/{id}` junto con los datos actualizados:

```json
{
	"numeroPedido": "PED-10001",
	"cliente": "Cliente actualizado",
	"fecha": "2026-11-02",
	"total": 1400.00,
	"estado": "En proceso",
	"version": "COPIAR_VERSION_DEVUELTA_POR_GET"
}
```

El listado acepta los parámetros opcionales `search`, `estado`, `page` y `pageSize`; por ejemplo, `GET /api/pedidos?search=PED-10001&estado=Registrado&page=1&pageSize=10`. `DELETE /api/pedidos/{id}` realiza una eliminación lógica. El inicio de sesión limita a cinco intentos por minuto por dirección IP.

## Pruebas y compilacion

Desde la raiz del repositorio:

```powershell
dotnet test backend/Pedidos.sln
dotnet build backend/Pedidos.sln
```

Para validar el frontend:

```powershell
cd frontend
npm install
npm run lint
npm run build
```

## Documentacion adicional

- [Backend](backend/README.md)
- [Frontend](frontend/README.md)
- [Base de datos](database/README.md)
