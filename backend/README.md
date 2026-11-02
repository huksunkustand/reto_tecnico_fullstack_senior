# Backend - Logs

La API conserva los logs JSON de consola y escribe los mismos eventos mediante
Serilog en `src/Pedidos.Api/logs/log-AAAAMMDD.txt`. La carpeta se crea al escribir
el primer evento y la ruta se resuelve desde el directorio de contenido de la API,
no desde el directorio de trabajo de la terminal.

Cada linea del archivo es un objeto JSON con fecha, nivel, plantilla del mensaje,
mensaje renderizado, propiedades y excepcion cuando corresponde. Los niveles se
controlan mediante la seccion `Logging:LogLevel` de la configuracion existente.

- Rotacion diaria y al alcanzar 10 MB por archivo.
- Los archivos adicionales del dia usan un sufijo numerico.
- Se conservan los 14 archivos mas recientes, no necesariamente 14 dias.
- La carpeta `logs/` esta excluida del control de versiones.
- La cuenta que ejecuta la API necesita permiso de escritura en esa carpeta.

Para verificarlo, reinicia la API y revisa el archivo del dia. Con un JWT valido,
`GET /api/pedidos?page=0&pageSize=10` devuelve 400 y genera un evento `Warning`
con las propiedades `Status` y `Message`. Una excepcion inesperada genera un
evento `Error` con la excepcion y el `TraceId` de la respuesta.

No registrar contrasenas, tokens ni cuerpos completos de solicitudes. Los logs
pueden contener informacion interna: restringir su acceso y no publicarlos como
archivos estaticos.