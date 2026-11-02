# Base de datos SQL Server

Ejecuta los archivos manualmente en SQL Server Management Studio (SSMS) o Azure Data Studio, sobre la misma instancia y en este orden. Ejecuta cada archivo completo y continua solo si termina sin errores. Los separadores `GO` requieren una herramienta compatible.

| Orden | Archivo | Proposito | Requerido |
| --- | --- | --- | --- |
| 1 | [01-create-database.sql](01-create-database.sql) | Crea `AtlanticPedidos` si no existe. | Si |
| 2 | [02-schema.sql](02-schema.sql) | Crea tablas, restricciones, indices e historial de EF Core. | Si |
| 3 | [03-seed-pedidos.sql](03-seed-pedidos.sql) | Inserta ocho pedidos de ejemplo sin duplicar sus numeros. | Opcional |
| 4 | [04-queries.sql](04-queries.sql) | Consultas de listado, detalle, busqueda, resumen y ejemplo de CRUD. | Opcional |
| 5 | [05-create-admin.sql](05-create-admin.sql) | Crea el administrador de prueba `kleiver@gmail.com` con un hash de ASP.NET Core Identity. | Para probar login |

El script 5 solo requiere que se hayan ejecutado los scripts 1 y 2. Usa la contrasena de prueba acordada para ese correo, sin almacenarla en texto plano en el archivo. Si el correo ya existe, no modifica su contrasena ni su rol; revisa el resultado del `SELECT`. Este usuario es exclusivamente para desarrollo y pruebas, no para produccion.

## Requisitos

- SQL Server y una cuenta con permisos para crear la base de datos y su esquema.
- Los archivos seleccionan explicitamente la base que utilizan. Si cambias el nombre `AtlanticPedidos`, actualizalo en los scripts y en la cadena de conexion del backend.
- El esquema corresponde a la migracion inicial de EF Core e incluye su registro en `__EFMigrationsHistory`. No crees las tablas manualmente por separado.

## Crear otros usuarios

El usuario se identifica por su correo; el modelo actual no incluye un nombre. Para generar SQL con otra contrasena es necesario generar otro hash compatible con ASP.NET Core Identity. No uses `HASHBYTES` ni guardes la clave directamente en `PasswordHash`.

Desde la carpeta raiz del repositorio, con el SDK .NET 8 o posterior instalado, ejecuta:

```powershell
dotnet run --project scripts/UserSql -- otro@ejemplo.com User database/06-create-user.generated.sql
```

La herramienta solicita la contrasena y su confirmacion de forma oculta (minimo 12 caracteres), genera y verifica el hash, y escribe el archivo SQL. El rol puede ser `User` o `Admin`; por defecto es `User`. Puedes cambiar el correo, el rol y el nombre del archivo en el comando. No sobrescribe archivos existentes.

Abre el archivo generado y ejecutalo manualmente despues de los scripts 1 y 2. No es necesario activar migraciones. Si el correo ya existe, el SQL falla sin cambiar su contrasena ni su rol. Luego prueba `/auth/login` en Swagger con ese correo y la contrasena introducida.

Los archivos `*.generated.sql` de esta carpeta estan excluidos de Git. Aunque contienen hashes y no claves en texto plano, deben tratarse como datos sensibles. La herramienta no se conecta a SQL Server ni crea usuarios por si misma.

Comprobacion local del generador, sin SQL Server ni contrasenas reales:

```powershell
dotnet run --project scripts/UserSql -- --self-test
```

## Consideraciones

- Los tres primeros scripts se pueden volver a ejecutar tras una ejecucion exitosa sin duplicar la base, el esquema ni los pedidos de ejemplo. No reparan tablas modificadas manualmente.
- El ultimo bloque de `04-queries.sql` demuestra alta, edicion y eliminacion logica dentro de una transaccion con `ROLLBACK`: no conserva esos cambios, aunque SQL Server puede consumir un valor de identidad.
- El script 5 permite crear el administrador sin activar migraciones ni almacenar contrasenas en texto plano. Como alternativa, la API contempla crear el primer administrador mediante `Seed__Email` y `Seed__Password` (minimo 12 caracteres) cuando `Database__AutoMigrate=true` y la tabla de usuarios esta vacia. Ese arranque tambien comprueba y aplica migraciones pendientes.
- Para que la API no aplique migraciones al arrancar, configura `Database__AutoMigrate=false`. Con esa opcion tampoco se crea el administrador inicial; debe existir previamente un usuario con un hash compatible con `PasswordHasher<Usuario>` de ASP.NET Core Identity.
- Configura `ConnectionStrings__SqlServer` para apuntar a tu instancia y a `AtlanticPedidos`. No guardes credenciales reales en el repositorio.

La ejecucion de estos archivos queda a cargo del usuario; no es necesario ejecutar comandos SQL desde los scripts de arranque para utilizar esta carpeta.