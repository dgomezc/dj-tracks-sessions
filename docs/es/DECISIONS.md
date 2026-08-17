# Decisiones del producto

> Traducción al español. El documento original en inglés se conserva en `../../DECISIONS.md`.

Este archivo registra decisiones establecidas. Cámbialas únicamente con aprobación explícita del producto.

## Despliegue

- Nombre del producto: **DJ Tracks & Sessions**.
- Aplicación exclusivamente web desplegada con Docker Compose en un NAS Linux x86-64.
- Un solo usuario, red local y sin autenticación en el MVP.
- El acceso externo futuro puede protegerse mediante Authelia en el límite del proxy inverso.
- Aproximadamente 800 pistas con crecimiento mensual lento; PostgreSQL y un worker son suficientes.
- Git y el repositorio canónico de GitHub, <https://github.com/dgomezc/dj-tracks-and-sessions>, son la fuente de verdad del desarrollo.
- El desarrollo y la depuración se ejecutan en un PC Windows 11 mediante WSL. Normalmente el árbol de trabajo reside en el sistema de archivos Linux de WSL, no en `/mnt/c`, por motivos de comportamiento y rendimiento de Linux/Docker.
- El desarrollo local de la API en el entorno `Development` lee `ConnectionStrings:Postgres` desde .NET User Secrets. La verificación de Docker Compose usa un valor externo no versionado de `ConnectionStrings__Postgres` porque User Secrets no está disponible dentro de contenedores; ningún flujo monta la biblioteca musical de producción.
- La compilación y las pruebas repetibles de WSL, junto con la comprobación opcional de construcción de imágenes `linux/amd64` desde el commit exacto, son la puerta de entrega. La verificación opcional de Compose local usa solo raíces desechables; no es un despliegue en el NAS y las imágenes locales nunca se transfieren.
- Docker Compose se prueba en el NAS solo cuando una versión utilizable de desarrollo/pruebas se ha clonado y seleccionado manualmente allí. No se usa conexión SSH, transferencia WSL-NAS, archivo de imagen, registry ni script de despliegue automatizado.
- El flujo manual del NAS usa secretos/configuración exclusivos del NAS, raíces de prueba desechables o representativas antes de montar cualquier biblioteca real, migración explícita, inicio y comprobaciones de salud y smoke.
- La instancia PostgreSQL del NAS es la base de desarrollo. El proyecto no despliega, inicia, monta ni gestiona contenedores o volúmenes PostgreSQL. Las cadenas de conexión de Docker/NAS permanecen en archivos de entorno o secretos externos e ignorados; una futura base de producción es independiente y debe aprovisionarse antes del despliegue de producción.
- El rollback selecciona la etiqueta de imagen inmutable anterior. El rollback de migraciones de base de datos es una operación del administrador de la base; la degradación del esquema nunca es implícita y la restauración debe usar una copia previa a la migración creada por el administrador solo cuando la compatibilidad lo requiera.
- Las ramas y commits se envían manualmente a GitHub. La automatización de GitHub Actions y la publicación de imágenes en GHCR podrán evaluarse más adelante, pero no forman parte del plan actual.

## Tecnología

- .NET 10, ASP.NET Core API y Blazor Interactive Server.
- Blazor Blueprint UI con temas claro y oscuro persistentes.
- API y frontend son proyectos y límites de ejecución separados.
- `DjTracksSessions.Api` es propietaria de `Features` organizadas como vertical slices. Cada feature contiene su endpoint, consultas, comandos, validaciones, mappers, handlers y pruebas de comportamiento; usa invariantes/entidades de Domain y adaptadores de Infrastructure sin ser propietaria de la persistencia ni de las implementaciones externas.
- Entity Framework Core con el proveedor Npgsql es la API de persistencia; PostgreSQL es la única base de datos de producción.
- EF Core usa Code First. `DjTrackSessions.Domain` contiene entidades, objetos de valor e invariantes independientes del framework; `DjTrackSessions.Infrastructure` contiene el DbContext, las configuraciones Fluent de entidades, las migraciones y los adaptadores de servicios externos.
- Interfaz en español, optimizada para uso de escritorio.

## Archivos y colecciones

- Docker Compose configura las raíces Main, Pending, Remember y Sessions.
- La biblioteca Main ya está catalogada y solo se vuelve a analizar mediante una solicitud explícita. Toda modificación propuesta requiere aprobación.
- Los archivos Pending se detectan automáticamente, pero solo se analizan cuando se solicita.
- Los archivos Pending aprobados solo pueden etiquetarse, renombrarse y moverse después de confirmar el movimiento.
- Las pistas Remember pueden analizarse y editarse, pero nunca se mueven; `PersonalGenre` siempre es `Remember`.
- Sessions es independiente, se edita manualmente, nunca se analiza automáticamente y queda fuera de las playlists de pistas y del reproductor global.
- Solo se gestionan archivos de audio y tracklists TXT de sesiones.

## Metadatos

- La salida de artistas principales usa separación por comas.
- Los títulos de remix usan `Title (Remixer Remix)`.
- El nombre de archivo usa `Artist 1, Artist 2 - Title (Remixer Remix).ext`.
- El año del proveedor prevalece en la etiqueta Year; el año personal/de descarga es el fallback.
- El género del proveedor prevalece en la etiqueta Genre; `PersonalGenre` es el fallback.
- `PersonalGenre` siempre se almacena por separado en la base de datos y en el archivo de audio.
- Géneros personales compatibles: Day Instrumental, Day Vocal, Night Instrumental, Night Vocal, TechnoHouse, Tribal y Remember.
- Tribal y TechnoHouse tienen precedencia sobre la clasificación Day/Night y Vocal/Instrumental.
- La tonalidad musical se escribe en notación Camelot.
- MP3 es el formato principal; FLAC, M4A/AAC, AIFF y WAV siguen reglas explícitas de capacidades.

## Carátulas

- Las carátulas siempre requieren revisión del usuario.
- Cada pista tiene como máximo una imagen incrustada.
- La carátula del proveedor reemplaza la carátula incrustada cuando se aprueba; de lo contrario, se normaliza la existente.
- La salida es JPEG, como máximo 500x500, conservando la relación de aspecto y sin ampliar.

## Proveedores y análisis

- Proveedores del MVP: MusicBrainz, Discogs y AcoustID/Chromaprint.
- El BPM y la tonalidad se analizan localmente cuando faltan en datos confiables del proveedor.
- Beatport es opcional y requiere acceso autorizado a la API.
- Las coincidencias de baja confianza o ambiguas requieren selección manual.
- El `PersonalGenre` de Pending se infiere, pero siempre se aprueba manualmente.

## Reproducción y playlists

- Reproductor global persistente con cola, shuffle, metadatos, carátula y waveform.
- El mini-reproductor contextual comparte el mismo motor de reproducción.
- La cola, la pista actual y la posición sobreviven a los reinicios y se restauran pausadas.
- Se admiten playlists inteligentes y manuales.
- Las exportaciones M3U8 admiten el mapeo configurable de rutas NAS a Windows para AIMP y Traktor.
- Sessions usa su propio reproductor y no necesita persistir la posición de reproducción.

## Seguridad

- Los archivos originales se modifican directamente.
- No hay copias de seguridad automáticas; la copia de seguridad del NAS es responsabilidad operativa.
- El historial reversible se conserva durante 365 días, incluidas las carátulas anteriores.
- Las eliminaciones individuales y masivas son permanentes, manuales y requieren confirmación explícita.
- Las recomendaciones de duplicados son orientativas; la eliminación nunca es automática.

## Alcance futuro

- Authelia y acceso externo.
- OpenSubsonic, la evolución abierta de la API Subsonic heredada, es el objetivo futuro de interoperabilidad para clientes representativos de Windows y Android; se trata de una evaluación, no de una selección de servidor para el MVP. En el momento de la implementación, compara servidores compatibles mantenidos actualmente con un adaptador propio de la aplicación y decide entre un despliegue paralelo y la exposición del adaptador usando evidencias de compatibilidad y seguridad.
- La evaluación de OpenSubsonic debe demostrar pistas y Sessions, streaming Range o transcodificación cuando corresponda, carátulas/metadatos y playlists cuando las reglas principales del producto lo permitan. Sessions sigue siendo una colección principal separada independientemente de cómo se presente por compatibilidad.
- API autorizada de Beatport o importación manual asistida de URL.
- Marcadores de sesiones y navegación por timestamp.
- Edición de tracklists.
- Exportación Traktor NML.
- Mejora de las sugerencias de `PersonalGenre` a partir de correcciones aprobadas.
