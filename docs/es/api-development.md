# Desarrollo de la API con Scalar

La API expone su documento OpenAPI generado y una referencia interactiva de Scalar para el desarrollo local y las pruebas dentro de una LAN de confianza. Estos endpoints son herramientas de desarrollo/pruebas, no un límite de autenticación.

## Ruta rápida

1. Configura el ajuste de base de datos local solo al probar endpoints que dependan de PostgreSQL:

   ```bash
   dotnet user-secrets set "ConnectionStrings:Postgres" "<connection-string>" --project src/DjTracksSessions.Api
   ```

   Mantén el valor real fuera de Git y no incluyas credenciales en comandos copiados en documentación o incidencias.
2. Inicia la API:

   ```bash
   dotnet run --project src/DjTracksSessions.Api
   ```

3. Abre [Scalar](http://localhost:5000/scalar) en un navegador. Usa la URL y el puerto que muestre `dotnet run` si son diferentes.

## Endpoints

| URL | Propósito |
|---|---|
| `/openapi/v1.json` | Documento JSON OpenAPI generado |
| `/scalar` | Referencia interactiva de Scalar |

## Probar una operación

1. Abre `/scalar` y selecciona una operación en la navegación izquierda.
2. Revisa la ruta, los parámetros, el cuerpo de la petición y las respuestas documentadas.
3. Selecciona **Try it** o **Test Request** y ejecuta la petición GET o POST.
4. Inspecciona el código de estado, las cabeceras y el cuerpo de respuesta que muestra Scalar.

Los endpoints de ejemplo actuales no necesitan una base de datos activa. Los endpoints de salud de base de datos y los futuros endpoints de catálogo sí. Configura `ConnectionStrings:Postgres` mediante .NET User Secrets para una API ejecutada localmente; en contenedores usa únicamente configuración externa no versionada.

## Límite de seguridad

Mantén `/openapi/v1.json` y `/scalar` en localhost o en la LAN de desarrollo de confianza. No los expongas a Internet ni a una red no confiable en una futura topología de producción salvo que se protejan explícitamente con autenticación, autorización y transporte seguro. No uses montajes de la biblioteca musical real en pruebas automatizadas ni introduzcas credenciales de producción en peticiones de Scalar.

Las pruebas Docker/NAS pertenecen a un flujo manual separado. Cuando exista una versión utilizable, ejecútala manualmente en el NAS con configuración local del NAS y raíces desechables o representativas, según [el flujo del NAS](deployment/nas-manual.md).
