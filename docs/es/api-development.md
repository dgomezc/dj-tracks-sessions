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

## Depurar ambos proyectos desde Visual Studio con WSL

Usa este flujo cuando quieras iniciar `DjTracksSessions.Api` y `DjTracksSessions.Web` juntos bajo el depurador de Visual Studio:

1. Abre `DjTracksSessions.slnx` en Visual Studio Community 2026. Abre el archivo de solución, no la carpeta del repositorio ni un proyecto individual.
2. En el Explorador de soluciones, haz clic derecho en la raíz de la solución y selecciona **Configure Startup Projects...**.
3. Selecciona **Multiple startup projects**.
4. Establece `DjTracksSessions.Api` y `DjTracksSessions.Web` en **Start**. Para cada proyecto, selecciona su destino de depuración `WSL` cuando esté disponible.
5. Guarda la configuración y selecciona el perfil de inicio multi-proyecto resultante en el selector de ejecución/depuración de Visual Studio.
6. Pulsa **F5**. Visual Studio debería iniciar ambos procesos y abrir las URL de API y Web definidas por sus perfiles WSL.

Los perfiles actuales del repositorio definen `WSL` para ambos proyectos y usan la distribución `Ubuntu`. La API usa el puerto HTTPS `2019` (HTTP `2020`); el proyecto Web usa el puerto HTTPS `2021` (HTTP `2022`). Usa las URL que muestre Visual Studio si la configuración local es diferente.

Visual Studio guarda una configuración de inicio de solución creada por el usuario en `DjTracksSessions.slnLaunch.user`. Es específica del usuario y normalmente no es un artefacto de configuración independiente de la máquina, por lo que debes configurar el perfil de nuevo en otro equipo. Un archivo compartido `DjTracksSessions.slnLaunch` solo debe versionarse cuando se quiera compartir intencionadamente esa configuración exacta; no sustituye la comprobación de que cada equipo tenga la distribución WSL, el SDK y los prerrequisitos de los proyectos.

Este flujo es depuración directa con WSL, no depuración mediante Docker Compose. El perfil WSL ejecuta los proyectos API y Web de la solución bajo el depurador. Docker Compose compila y ejecuta los servicios en contenedores con su propia configuración, puertos, montajes y un valor externo de `ConnectionStrings__Postgres`; usa el flujo de Compose para verificar contenedores o el comportamiento en el NAS.

Antes de pulsar F5, verifica que Visual Studio Community 2026 tiene las herramientas de desarrollo .NET/web, que WSL 2 está instalado y dispone de la distribución `Ubuntu`, que el SDK de .NET 10 está disponible en WSL y que el repositorio es accesible desde el sistema de archivos Linux de WSL. Si los proyectos fallan al iniciar, comprueba primero los destinos de depuración seleccionados, la distribución WSL, la configuración local de User Secrets/base de datos necesaria para los endpoints que dependan de ella y las URL HTTPS que muestre Visual Studio. No uses los montajes de la biblioteca musical de producción en este flujo de depuración local.

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
