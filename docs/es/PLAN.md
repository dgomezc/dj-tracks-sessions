# Plan de implementación de DJ Tracks & Sessions

> Traducción al español. El documento original en inglés se conserva en `../../PLAN.md`.

Este documento es la fuente ejecutable de verdad para el alcance del producto, la arquitectura, el orden de entrega y los criterios de aceptación. Track Management no tiene un índice persistido de pistas; la entrega continúa estrictamente por pantallas completas: gestión de pistas, reproductor, Sessions y después las pantallas restantes.

La exploración de Track Management usa el sistema de archivos: enumera directamente las raíces configuradas Main, Pending y Remember mediante rutas relativas confinadas y lee de los archivos las propiedades técnicas y etiquetas actuales. Las etiquetas del archivo son la fuente de verdad. PostgreSQL no es necesario para listar, mostrar detalle, editar ni mover pistas. Sessions sigue siendo una colección y un flujo separado.

Un agente de implementación debe completar las fases en orden. Dentro de una fase, entrega una unidad de trabajo vertical cada vez junto con sus pruebas. Una pantalla no está completa hasta que existen su comportamiento API, estados de UI, flujo de seguridad y evidencias de aceptación; no comiences la siguiente pantalla hasta que pasen los criterios de salida de la actual.

## 1. Resultado del producto

Construir una aplicación web en español, de un solo usuario, que se ejecute en Docker sobre un NAS Linux x86-64 y gestione una colección personal de música House/electrónica.

El producto tiene tres experiencias claramente separadas:

1. **Explorer / Player** para explorar, buscar, reproducir y organizar pistas catalogadas.
2. **Analyzer / Tagger** para identificar pistas, revisar propuestas, escribir metadatos y procesar archivos pendientes.
3. **Sessions** para explorar y reproducir sesiones personales de DJ con sus tracklists TXT asociados.

## 2. Objetivos excluidos

- Aplicaciones de escritorio, móviles, Tauri o Electron.
- Cuentas multiusuario, autorización o exposición a Internet en el MVP.
- Scraping automático de Beatport.
- Eliminación automática de cualquier archivo.
- Movimiento automático de archivos que ya estén en Main o Remember.
- Análisis automático de Sessions.
- Paneles de estadísticas.
- Gestión de letras, archivos CUE o carátulas externas.
- Marcas de tiempo de pistas de sesiones o edición de tracklists en el MVP.

## 3. Configuración de ejecución

Docker Compose debe configurar estas rutas del host como montajes de lectura/escritura:

| Configuración de entorno | Ruta del contenedor | Política |
|---|---|---|
| `MAIN_LIBRARY_PATH` | `/music/main` | Pistas catalogadas; reanálisis mediante solicitud y aprobación explícitas |
| `PENDING_LIBRARY_PATH` | `/music/pending` | Bandeja de entrada; detectar automáticamente, analizar manualmente, mover después de aprobación y confirmación |
| `REMEMBER_LIBRARY_PATH` | `/music/remember` | Analizar/editar sin mover; forzar `PersonalGenre=Remember` |
| `SESSIONS_LIBRARY_PATH` | `/music/sessions` | Solo metadatos manuales; reproductor y tracklists TXT separados |
| Datos de la aplicación | `/app/data` | Caché de waveform y datos de ejecución no pertenecientes a la base de datos |
| Conexión PostgreSQL | Secreto/configuración externa local del NAS | Estado de escaneo opcional, historial, estado de jobs, playlists, notificaciones y configuración; no es necesaria para explorar Track Management |

Configura también:

- Identidad y contacto de la aplicación MusicBrainz.
- Token de Discogs.
- Clave de aplicación de AcoustID.
- Perfiles de traducción de rutas NAS a Windows/UNC para las exportaciones de playlists.
- Intervalo de escaneo/reconciliación.
- Concurrencia del análisis, con valor predeterminado uno.
- Cadena de conexión PostgreSQL desde una fuente de secretos/configuración local del NAS para contenedores o .NET User Secrets para el desarrollo local de la API.

Los secretos deben proceder de variables de entorno o Docker secrets y nunca deben confirmarse en el repositorio.

Requisitos de PostgreSQL:

- La instancia PostgreSQL del NAS es la base de desarrollo. Compose no debe desplegar, iniciar, montar ni gestionar contenedores o volúmenes PostgreSQL.
- El desarrollo local de la API en el entorno `Development` lee `ConnectionStrings:Postgres` desde .NET User Secrets. Configura los contenedores con la cadena externa de Npgsql como `ConnectionStrings__Postgres` mediante un archivo de entorno ignorado o una fuente de secretos; User Secrets no está disponible dentro de contenedores.
- Aprovisionar una base de producción separada únicamente para el futuro despliegue de producción.
- Usar migraciones de Entity Framework Core como único mecanismo de evolución del esquema.
- Aplicar migraciones mediante un paso explícito de despliegue/inicio que falle visiblemente; nunca crear ni recrear silenciosamente la base de datos.
- Usar restricciones y transacciones de PostgreSQL para invariantes que crucen registros persistidos.
- Probar el comportamiento de la base de datos contra la versión major compatible de PostgreSQL mediante Testcontainers for .NET.

## 4. Arquitectura objetivo

```text
Navegador de escritorio
  |
  +-- DjTracksSessions.Web (Blazor Interactive Server)
          |
          +-- cliente HTTP generado/tipado
                  |
                  +-- DjTracksSessions.Api
                        +-- Vertical slices
                        +-- Worker de jobs en segundo plano
                        +-- DjTrackSessions.Domain (entidades, objetos de valor, invariantes)
                        +-- DjTrackSessions.Infrastructure (EF Core/Npgsql, migraciones, adaptadores)
```

### 4.1 Estructura de proyectos

Crea como mínimo los proyectos útiles:

```text
src/
  DjTracksSessions.Api/
    Features/
      Library/
      Pending/
      Matching/
      Metadata/
      Playback/
      Playlists/
      Duplicates/
      History/
      Sessions/
  DjTrackSessions.Domain/
    Entidades y objetos de valor
    Invariantes de dominio
  DjTrackSessions.Infrastructure/
    DbContext EF Core Code First y configuraciones Fluent
    Migraciones de EF Core
    Adaptadores de base de datos, sistema de archivos, proveedores, imágenes y herramientas de audio
  DjTracksSessions.Web/
  DjTracksSessions.Contracts/
tests/
  DjTracksSessions.UnitTests/
  DjTracksSessions.IntegrationTests/
```

`DjTracksSessions.Api` es propietaria de los directorios `Features` de los vertical slices. Cada feature contiene su endpoint, consultas, comandos, validaciones, mappers, handlers y pruebas de comportamiento; usa `DjTrackSessions.Domain` para entidades e invariantes y los adaptadores de `DjTrackSessions.Infrastructure`. No muevas la persistencia ni las implementaciones externas a la API, no conviertas los slices en carpetas horizontales genéricas ni crees más proyectos de capas sin un límite real de dependencias.

### 4.2 Contrato del slice

Cada slice de comando/consulta debe contener:

- Contrato del endpoint y de request/route.
- Consulta o comando.
- Validador de FluentValidation.
- Handler.
- Resultado de éxito/fallo de FluentResults.
- Mapeo específico del slice.
- Pruebas de comportamiento.

Usa códigos de error estables como `track.not_found`, `path.outside_root`, `match.ambiguous`, `file.collision` y `tag.unsupported`.

### 4.3 Trabajo en segundo plano

Las operaciones largas no deben ejecutarse en un circuito de Blazor ni en una solicitud HTTP:

- Escaneos y reconciliaciones de raíces.
- Hashing y fingerprinting.
- Consultas a proveedores.
- Análisis de BPM/tonalidad.
- Generación de waveform.
- Operaciones masivas de etiquetas.
- Limpieza del historial.

Persiste el estado de los jobs en PostgreSQL mediante Entity Framework Core. Expón el progreso y la finalización mediante polling de la API o notificaciones del servidor. Un reinicio no debe perder silenciosamente el trabajo en cola o activo; los elementos interrumpidos pasan a ser reintentables o fallidos con un motivo.

## 5. Modelo de datos principal

Las entidades EF exactas pueden variar, pero los conceptos persistidos deben incluir:

| Concepto | Responsabilidad requerida |
|---|---|
| `LibraryRoot` | Tipo de raíz, ruta canónica y capacidades permitidas |
| `AudioTrack` | ID estable, ruta actual, raíz, propiedades técnicas, hashes y estado |
| `TrackMetadata` | Metadatos estructurados actuales del proveedor, personales y efectivos |
| `MetadataProvenance` | Origen, confianza, ID del proveedor y fecha de consulta por campo |
| `Artwork` | Metadatos de imagen normalizados actuales/referencia de caché |
| `AnalysisRun` | Identidad de entrada, estado, versiones de herramientas, resultados y fallos |
| `MatchCandidate` | Candidato del proveedor, puntuación, evidencias y estado de selección |
| `ChangeProposal` | Valores anteriores/posteriores que requieren aprobación |
| `ChangeHistory` | Estado anterior reversible, operación, marca de tiempo y expiración |
| `Playlist` | Entradas ordenadas manuales o definición de regla inteligente |
| `PlaybackSession` | Cola, estado de shuffle, elemento actual y última posición |
| `DuplicateGroup` | Evidencia de duplicado exacto/posible y estado de resolución |
| `DjSession` | Metadatos de sesión separados, identidad de audio y ruta del tracklist |
| `Job` | Estado y progreso del trabajo persistente |
| `Notification` | Notificación persistente de finalización/error |

Usa un token de concurrencia optimista para los registros mutables y rechaza aprobaciones obsoletas.

## 6. Reglas de metadatos

### 6.1 Valores estructurados

Almacena estos valores por separado en lugar de analizar posteriormente strings de presentación:

- Artistas principales ordenados.
- Título base.
- Nombre del mix.
- Remixers.
- Año del proveedor y año personal.
- Género del proveedor y `PersonalGenre`.
- BPM/tonalidad proporcionados y analizados localmente.
- ISRC, sello, número de catálogo e identificadores de proveedores.

### 6.2 Valores efectivos

```text
EffectiveYear = ProviderYear ?? PersonalYear
EffectiveGenre = ProviderGenre ?? PersonalGenre
EffectiveBpm = TrustedProviderBpm ?? LocalBpm
EffectiveKey = TrustedProviderKey ?? LocalKey
```

La salida de tonalidad usa notación Camelot.

### 6.3 Presentación y nombre de archivo

```text
Artist tag: Artist 1, Artist 2
Title tag: Base Title (Remixer Remix)
Filename: Artist 1, Artist 2 - Base Title (Remixer Remix).ext
```

No dupliques un sufijo de remix existente. Conserva la extensión original y sanea únicamente los caracteres inválidos para el sistema de archivos de destino.

### 6.4 PersonalGenre

Valores iniciales permitidos:

- `Day Instrumental`
- `Day Vocal`
- `Night Instrumental`
- `Night Vocal`
- `TechnoHouse`
- `Tribal`
- `Remember`

Es obligatorio para las pistas catalogadas ordinarias. Persístelo en PostgreSQL y en el archivo de audio. La asignación inicial de MP3 es `TXXX:PERSONAL_GENRE`; define y prueba una asignación explícita para cada formato compatible.

La clasificación evalúa por separado la energía/contexto, la presencia vocal y el carácter tribal/étnico. `Tribal` y `TechnoHouse` son categorías dominantes. Para las pistas pendientes, muestra la mejor propuesta, confianza, evidencias y alternativa más fuerte; la aprobación del usuario es obligatoria.

## 7. Políticas de raíces

| Capacidad | Main | Pending | Remember | Sessions |
|---|---:|---:|---:|---:|
| Indexar automáticamente | Sí | Sí | Sí | Sí |
| Explorar/reproducir | Sí | Sí | Sí | Sección separada |
| Analizar automáticamente al detectar | No | No | No | Nunca |
| Iniciar análisis de proveedor manualmente | Pistas seleccionadas | Pistas nuevas/seleccionadas | Pistas seleccionadas | Nunca |
| Exigir aprobación antes de escribir datos analizados | Siempre | Coincidencia ambigua, carátula, PersonalGenre | Flujo de revisión configurado | N/A |
| Edición manual de metadatos | Sí | Sí | Sí | Sí |
| Movimiento automático | Nunca | Después de aprobación y confirmación explícita | Nunca | Nunca |
| Regla de PersonalGenre | Desde carpeta/manual | Inferido y aprobado | Siempre Remember | No requerido |

Estados del análisis Pending:

- New.
- Queued.
- Analyzing.
- Awaiting match review.
- Awaiting artwork review.
- Awaiting PersonalGenre approval.
- Approved, awaiting apply/move.
- Unidentified.
- Postponed/rejected.
- Failed.
- Completed/moved.

La identidad del análisis se basa en el hash del contenido y la versión del análisis. Un archivo modificado se convierte en nuevo. Un archivo sin cambios y analizado previamente sigue visible como analizado y se excluye de la ejecución predeterminada salvo que se incluya explícitamente.

## 8. Estrategia de proveedores y análisis

### 8.1 Pipeline de coincidencias

1. Lee las etiquetas actuales y las propiedades técnicas sin mutar.
2. Calcula un hash de contenido y la duración.
3. Da preferencia a un ISRC existente válido.
4. Genera el fingerprint de Chromaprint y consulta AcoustID.
5. Resuelve grabaciones/lanzamientos mediante MusicBrainz.
6. Busca y enriquece mediante Discogs.
7. Puntúa candidatos usando ISRC, fingerprint, artistas, título/mix normalizados y duración.
8. Devuelve el candidato seleccionado de alta confianza o una lista de revisión ordenada.

Respeta los requisitos de identidad, atribución, licencias, caché y límites de frecuencia de cada proveedor. Implementa reintentos con backoff exponencial acotado para fallos transitorios, pero no reintentes indefinidamente los fallos de validación/autenticación.

### 8.2 Análisis de audio local

Usa FFmpeg/ffprobe en contenedor y una biblioteca/herramienta de análisis probada para obtener:

- Duración y propiedades del códec.
- BPM de fallback.
- Tonalidad musical de fallback, convertida a Camelot.
- Datos de picos de waveform.
- Evidencia útil para sugerencias de `PersonalGenre` cuando sea técnicamente viable.

Registra las versiones de herramientas y algoritmos para poder invalidar y recalcular resultados después de una actualización.

### 8.3 Beatport

Define una interfaz de proveedor que pueda admitir Beatport más adelante, pero no implementes scraping automático. La integración con API autorizada y la importación manual asistida de URL siguen siendo trabajo futuro.

## 9. Contrato de carátulas

Las carátulas siempre se revisan antes de reemplazarlas.

Al aplicar una carátula:

1. Prefiere la carátula aprobada del proveedor.
2. Si no existe, selecciona una imagen incrustada existente.
3. Corrige la orientación.
4. Convierte a JPEG.
5. Ajusta dentro de 500x500 conservando la relación de aspecto.
6. Nunca amplíes.
7. Elimina todas las imágenes incrustadas.
8. Incrusta exactamente la imagen normalizada aprobada.
9. Verifica que el archivo resultante pueda volver a abrirse y contenga una imagen.

Conserva la carátula anterior en el historial reversible hasta su expiración.

## 10. Contrato del sistema de archivos y mutaciones

- Resuelve cada ruta solicitada bajo su raíz canónica configurada.
- Rechaza escapes mediante symlink o traversal de rutas.
- Comprueba los permisos de escritura disponibles y las colisiones de destino antes de mutar.
- Escribe las etiquetas en un archivo hermano temporal, vuelve a abrirlo y verifícalo, y después reemplaza atómicamente el original cuando el sistema de archivos lo admita.
- Un movimiento desde Pending combina la escritura de etiquetas verificada, el nombre de archivo final y el movimiento de destino como una operación recuperable.
- La aprobación no autoriza el movimiento. Muestra el origen exacto, destino, nombre de archivo final y colisiones, y después solicita confirmación por separado.
- El destino usa `<PersonalYear>/House - <PersonalGenre>/`.
- `PersonalYear` es el año personal/de descarga, normalmente el año actual para pistas nuevas pendientes.
- Si existe un nombre de archivo de destino, bloquea y abre una resolución de duplicado/manual. Nunca añadas `(1)`.
- Los movimientos externos en raíces catalogadas actualizan el índice y crean una propuesta de metadatos cuando cambian los valores derivados de la carpeta; no escriben automáticamente.

El historial conserva durante 365 días las etiquetas, carátula, nombre de archivo y ruta anteriores completas. Un job programado elimina el historial y los blobs expirados. La eliminación permanente no se puede deshacer y requiere confirmación reforzada tanto para operaciones individuales como masivas.

## 11. Requisitos de Explorer / Player

### 11.1 Explorer

- Árbol de carpetas para Main, Pending y Remember.
- Cuadrícula de datos densa para escritorio.
- Búsqueda de texto completo.
- Filtros por artista, título, año efectivo, género del proveedor y `PersonalGenre`.
- Filtros combinados, ordenación y multiselección.
- Acciones para reproducir, poner en cola, editar manualmente, editar en masa, volver a analizar, playlist, revisar duplicados y eliminar.
- Los resultados de búsqueda pueden reemplazar o añadir a la cola.

### 11.2 Reproductor global

- Un motor de reproducción compartido por los controles globales y contextuales.
- Cola desde selección manual, carpeta, búsqueda o playlist.
- Añadir, eliminar, reordenar, limpiar, anterior, siguiente, repeat, volumen y seek.
- Shuffle crea un ciclo sin repeticiones y conserva el historial para Previous.
- Muestra carátula, artistas, título, género efectivo, `PersonalGenre`, BPM y tonalidad Camelot.
- Muestra un waveform interactivo mediante picos almacenados en caché y generados por el servidor.
- Persiste la cola, la pista actual, el estado de shuffle y la posición; restaura en pausa.
- Resuelve pistas movidas mediante ID estable y omite entradas inexistentes/eliminadas.
- Transmite audio con soporte HTTP Range.

### 11.3 Teclado y notificaciones

- Atajos sensibles al contexto para reproducir/pausar, anterior/siguiente, cola, guardar, aprobar/rechazar y navegación de Pending.
- Ignora los atajos mientras el usuario escribe.
- Nunca omite confirmaciones destructivas.
- Persiste las notificaciones de finalización/error de jobs y enlaza con sus resultados.

## 12. Playlists

Admite:

- Playlists manuales con referencias estables y ordenadas a pistas.
- Playlists inteligentes que almacenan reglas de filtro y se actualizan dinámicamente.
- Reglas iniciales útiles como Day, Day Vocals, Night, Tribal y TechnoHouse para todos los años.
- Reemplazo/adición a la cola y reproducción con shuffle.

Exporta M3U8 en UTF-8 usando perfiles configurables. Traduce las rutas de contenedor `/music/...` a rutas UNC/Windows visibles para el host para AIMP y Traktor. Ofrece modos relativo y absoluto cuando sean válidos y muestra una vista previa de las rutas no resueltas antes de exportar.

## 13. Duplicados

Detecta:

- Duplicados exactos por hash de contenido.
- Posibles duplicados por fingerprint, ISRC, metadatos normalizados y duración.

La comparación muestra ambos archivos y permite reproducirlos, con ruta, formato, bitrate, frecuencia de muestreo, tamaño, duración, metadatos y carátula. Recomienda qué archivo conservar usando la calidad técnica y la evidencia de versión exacta. Nunca elimines automáticamente.

## 14. Sessions

Sessions tiene sus propias rutas, consultas, explorer, búsqueda y reproductor. No reutilices la cola global de pistas.

Requisitos:

- Explorar por año/carpeta.
- Leer la duración real del audio.
- Destacar título, año, carátula y duración, permitiendo otras ediciones manuales de metadatos compatibles.
- No llamar a proveedores de metadatos musicales ni al análisis automático de pistas.
- Mostrar un waveform interactivo.
- No persistir la posición de reproducción.
- Resolver el tracklist en este orden:
  1. `<audio-base-name>.txt` junto al audio.
  2. `tracklist.txt` junto al audio cuando el directorio representa una sesión.
  3. Marcar como ambiguo si varios audios comparten un tracklist genérico.
- Mostrar el texto del tracklist en modo de solo lectura y conservar el archivo fuente sin cambios.

## 15. Fases de entrega

### 15.1 Flujo de desarrollo y despliegue de pruebas

Git y el repositorio canónico de GitHub, <https://github.com/dgomezc/dj-tracks-and-sessions>, son la fuente de verdad. La ruta normal de entrega es:

1. Clona y conserva el árbol de trabajo en el sistema de archivos Linux de WSL del PC de desarrollo Windows 11, no bajo `/mnt/c`, salvo que una restricción documentada de una herramienta exija lo contrario. Esto conserva la semántica del sistema de archivos Linux/Docker y evita penalizaciones de rendimiento entre sistemas.
2. Desarrolla en una rama de funcionalidades en WSL. Ejecuta la API local en `Development` con su cadena de conexión en .NET User Secrets. Ejecuta compilaciones locales repetibles, pruebas y verificación de Docker Compose contra raíces fixture desechables y un valor externo no versionado de `ConnectionStrings__Postgres` porque los contenedores no pueden acceder a User Secrets. Esta verificación local es la puerta de entrega. Nunca montes la biblioteca musical de producción en el ciclo local.
3. Después de pasar la puerta local de compilación y pruebas, construye opcionalmente imágenes de aplicación inmutables `linux/amd64` desde el commit Git exacto con una etiqueta explícita derivada del commit como comprobación de arquitectura/imágenes. No transfieras esas imágenes al NAS ni uses una etiqueta flotante `latest`. Envía manualmente la rama y los commits a GitHub.
4. Cuando exista una versión utilizable de desarrollo/pruebas, clona o actualiza manualmente el repositorio en el NAS, selecciona allí la rama, etiqueta o commit, prepara un archivo de entorno/secretos local ignorado y raíces desechables, y valida la configuración de Compose. No se usa conexión SSH al NAS, transferencia WSL-NAS, archivo de imagen, registry ni script de despliegue.
5. Desde el checkout seleccionado en el NAS, construye y ejecuta manualmente el destino de migraciones contra la instancia externa PostgreSQL de desarrollo, ejecuta `docker compose up -d` y realiza comprobaciones de salud y smoke.

El operador debe detenerse si falla la configuración, la migración, el inicio, la salud o la verificación smoke. Las credenciales y secretos no deben aparecer en el control de código fuente, los argumentos documentados ni los logs de despliegue.

El rollback selecciona la etiqueta de imagen inmutable anterior. Restaura una copia de seguridad creada por el administrador de la base únicamente cuando la versión anterior de la aplicación sea incompatible con el esquema migrado; nunca intentes una degradación implícita del esquema. La documentación de despliegue debe indicar el límite de compatibilidad de migraciones y hacer visibles los fallos de migración y rollback.

La automatización de GitHub Actions y la publicación de imágenes en GHCR podrán evaluarse más adelante. Ninguna forma parte del plan ni de la puerta de entrega actuales.

### Fase 0: Spikes de viabilidad

Objetivo: eliminar la incertidumbre técnica antes de construir los flujos del producto.

Unidades de trabajo:

1. Round trip de lectura/escritura para fixtures representativos MP3, FLAC, M4A, AIFF y WAV.
2. Asignación personalizada de `PersonalGenre` por formato.
3. Normalización de carátula y verificación de una imagen.
4. FFmpeg/ffprobe, Chromaprint y análisis de BPM/tonalidad en un contenedor `linux/amd64`.
5. Streaming HTTP Range y generación de picos de waveform.
6. Clientes de proveedores que demuestren solicitudes, límites y fixtures normalizados de MusicBrainz, Discogs y AcoustID.

Criterios de salida:

- Una matriz de capacidades escrita identifica los campos seguros de lectura/escritura por formato.
- Los archivos de muestra sobreviven a una mutación verificada sin perder etiquetas desconocidas.
- Las imágenes de herramientas se ejecutan en la arquitectura NAS objetivo.
- Los adaptadores de proveedores tienen pruebas deterministas con fixtures.
- Cualquier comportamiento de formato no compatible se degrada explícitamente en lugar de adivinarse.

### Fase 1: Base de la aplicación

Objetivo: iniciar una aplicación vacía con forma de producción.

Unidades de trabajo:

1. Solución, proyectos, dirección de dependencias y proyectos de pruebas.
2. Problem Details de la API y mapeo de FluentResults.
3. Registro de FluentValidation y pipeline explícito de validación asíncrona.
4. Esquema PostgreSQL Code First, migraciones EF Core revisadas mediante Npgsql y health checks.
5. Dockerfiles y Docker Compose con cuatro montajes musicales, conexión PostgreSQL externa y volumen persistente de datos de aplicación.
6. Shell Blazor Blueprint, UI en español, temas claro/oscuro y áreas principales de navegación separadas.
7. Primitivas durables de jobs y notificaciones.
8. Comandos o scripts locales repetibles de WSL para compilación, pruebas, verificación de Compose y construcción de imágenes inmutables `linux/amd64` desde un commit Git exacto.
9. Documentación del despliegue manual de prueba en el NAS que cubra selección de versión en un clone del NAS, configuración local, raíces desechables, validación de Compose, migración externa explícita, inicio, health checks, smoke checks, rollback y limpieza.

Criterios de salida:

- Compose inicia API y Web en `linux/amd64`.
- Los health checks verifican la base de datos, los montajes configurados y las herramientas necesarias.
- El frontend se comunica únicamente mediante contratos de API.
- Las pruebas de integración de persistencia se ejecutan contra contenedores PostgreSQL desechables y las verificaciones Compose usan una conexión externa no versionada con raíces del sistema de archivos desechables.
- La verificación local falla antes de construir imágenes o desplegar cuando fallan compilaciones, pruebas o comprobaciones Compose requeridas; las imágenes locales usan una etiqueta inmutable explícita derivada del commit.
- Una versión utilizable seleccionada manualmente puede probarse en el NAS usando configuración exclusiva del NAS y raíces desechables, sin credenciales codificadas.
- Un fallo de configuración, migración, inicio, health check o smoke check es visible sin habilitar montajes de bibliotecas reales.

### Fase 2: Índice de la biblioteca (base completada hasta la unidad 4)

Objetivo: establecer un catálogo confiable y de solo lectura.

Unidades de trabajo completadas:

1. Configurar y validar políticas de raíces.
2. Escanear audio compatible y archivos TXT de sesiones.
3. Extraer propiedades técnicas y etiquetas actuales.
4. Calcular incrementalmente una identidad hash estable.

Unidades originales aplazadas:

5. Reconciliar archivos renombrados, movidos, modificados y ausentes. Sustituida para la entrega inmediata por la reconciliación más simple de la unidad 1 de la Fase 2A.
6. Vigilar las raíces y programar la reconciliación completa. Aplazada; la exploración de la Fase 2A accede directamente al sistema de archivos y el escaneo persistido es estado opcional, no un requisito de exploración.
7. Construir árbol de carpetas, cuadrícula de catálogo, búsqueda y filtros requeridos. Sustituida para la entrega inmediata por el catálogo de solo lectura más pequeño de la unidad 2 de la Fase 2A.

Criterios de salida originales de la Fase 2, ahora aplazados o cubiertos mediante la Fase 2A y el trabajo posterior del roadmap:

- Las cuatro raíces se indexan sin modificar archivos.
- Los escaneos repetidos son idempotentes.
- Los movimientos se correlacionan por identidad cuando es posible.
- Se rechaza la navegación fuera de las raíces configuradas.
- Main/Pending/Remember son visiblemente distintos; Sessions está separada.

La Fase 2 no es la cola de implementación inmediata. Continúa con la Fase 2A siguiente; vuelve al trabajo aplazado de la Fase 2 únicamente mediante una modificación explícita del plan.

### Fase 2A: Entrega con prioridad al catálogo (inmediata)

Objetivo: entregar un catálogo útil y el flujo mínimo seguro de organización antes de la automatización, el análisis de proveedores, la reproducción y las funciones especializadas.

Esta etapa usa operaciones síncronas iniciadas explícitamente cuando corresponde. La exploración de Track Management mantiene el acceso directo al sistema de archivos; el estado de escaneo persistido es opcional para explorar, Sessions permanece separada y se conservan todos los límites de seguridad del sistema de archivos existentes.

Unidades de trabajo:

1. **Persistir un escaneo explícito de solo lectura (completado en `b1f4df6`).** Escanear las cuatro raíces configuradas mediante el scanner confinado, la extracción y el hashing incremental existentes; hacer upsert síncrono del modelo mínimo, informar fallos por archivo, reconciliar coincidencias inequívocas por hash dentro de la misma raíz y marcar ausentes sin borrar nada.
2. **Entregar la pantalla de gestión de pistas de solo lectura (siguiente).** Añadir endpoints respaldados por el sistema de archivos y una pantalla de escritorio en español para explorar por separado Main/Pending/Remember, mostrar etiquetas/propiedades técnicas actuales, buscar texto básico, aplicar filtros simples y presentar estados de carga/vacío/fallo. Un escaneo manual puede actualizar el estado persistido, pero no es requisito previo para explorar. Sessions queda excluida. Sin reproducción, acciones de proveedores, acciones masivas, escrituras de metadatos ni actualización automática.
3. **Cerrar la gestión de pistas con edición segura de un MP3.** Añadir en la misma pantalla una vista previa exacta antes/después y envío explícito para campos de texto MP3 compatibles, incluido `TXXX:PERSONAL_GENRE`; usar rutas confinadas, escritura temporal hermana, verificación al reabrir, reemplazo atómico y fallos claros para formatos no compatibles.
4. **Cerrar la gestión de pistas con un movimiento Pending a Main aprobado.** Añadir en la misma pantalla la confirmación separada de origen exacto, destino, nombre final y resultado de colisión después de una edición aprobada; bloquear colisiones y actualizar el catálogo únicamente tras un movimiento verificado.

Criterios de salida:

- Un escaneo explícito persiste un fixture desechable de cuatro raíces sin cambiar los bytes originales, es idempotente e informa los fallos de forma independiente.
- La pantalla de gestión de pistas explora Main, Pending y Remember directamente desde sus raíces configuradas mediante contratos API; las etiquetas actuales del archivo se muestran como fuente de verdad. La persistencia PostgreSQL no es necesaria para listar ni mostrar etiquetas, y Sessions nunca entra en su ruta, consultas, filtros, detalle ni futuros controles de cola.
- Una edición de texto de un MP3 conserva etiquetas desconocidas y deja el origen intacto ante cualquier fallo de validación, verificación, confinamiento o reemplazo.
- Un movimiento de MP3 Pending requiere confirmación exacta separada, bloquea colisiones sin sufijos y nunca mueve archivos de Main o Remember.

Límite de rollback: cada unidad revierte su migración, cambios de API/Web y pruebas con fixtures desechables; los archivos fuente permanecen intactos salvo la escritura MP3 o el movimiento Pending explícitamente confirmado y verificado.

Las unidades originales 5-7 de la Fase 2 y todas las pantallas posteriores a gestión de pistas siguen siendo alcance posterior. Su automatización y requisitos no son prerrequisitos de la Fase 2A y no deben describirse como el próximo trabajo inmediato.

### Secuencia de entrega por pantallas

1. **Pantalla de gestión de pistas.** Completa las unidades 2-4 de la Fase 2A: UI de escaneo/exploración/detalle de solo lectura, edición segura de un MP3 y un movimiento Pending con confirmación separada. No inicies UI de reproductor ni de Sessions antes de que pasen sus criterios de salida.
2. **Pantalla del reproductor.** Completa la UI del reproductor de catálogo y su soporte Range/API, servicio de reproducción compartido, cola, estados visibles y pruebas. Incluye una única superficie Player montada en el shell, no un mini-reproductor contextual. Sessions queda fuera del reproductor y su cola. Waveforms, reproducción persistida, shuffle/repeat, integración de mini-reproductor y atajos se aplazan salvo que se añadan explícitamente a las unidades aprobadas de esta pantalla.
3. **Pantalla de Sessions.** Completa consultas aisladas de Sessions, explorer/detalle, su reproductor propio, resolución y presentación de tracklist de solo lectura, estados visibles de ambigüedad/error y pruebas. Sessions nunca entra en la búsqueda de pistas, cola global, playlists, detección de duplicados, proveedores ni análisis automático.
4. **Pantallas restantes.** Solo después de cerrar las tres primeras pantallas, secuencia Analyzer/Tagger, playlists, duplicados, edición avanzada de metadatos/historial/eliminación y pantallas operativas como unidades verticales independientes.

### Fase 3: Pantalla del reproductor

Objetivo: entregar la pantalla completa del reproductor de catálogo después de cerrar gestión de pistas.

Unidades de trabajo:

1. Endpoint de audio con Range y content type apropiado para el formato.
2. Un único servicio frontend de reproducción compartido y superficie Player montada en el shell; sin mini-reproductor contextual.
3. Controles de cola en memoria: añadir, quitar, limpiar, anterior, siguiente, volumen y seek.
4. Estados de carga, no disponible, finalizado y un único audio activo, con pruebas específicas.

Criterios de salida:

- El seek funciona sin descargar primero el archivo completo.
- La navegación no interrumpe la reproducción.
- El Player montado en el shell es la única superficie de control de reproducción de catálogo en esta fase.
- Sessions nunca entra en la cola global ni en la pantalla del reproductor.

Se aplazan para después de la pantalla del reproductor: cola/posición persistida, shuffle/repeat, waveform, integración de mini-reproductor contextual y atajos de teclado. Cuando se apruebe un mini-reproductor, debe coordinarse mediante el mismo servicio frontend de reproducción.

### Fase 4: Pantalla de Sessions

Objetivo: entregar la pantalla completa e independiente de Sessions después de cerrar el reproductor.

Unidades de trabajo:

1. Consultas de Sessions y explorer por año/carpeta.
2. Detalle de sesión y edición manual de metadatos compatibles.
3. Reproductor separado sin posición persistida.
4. Resolución de tracklists, estado de ambigüedad y presentación de solo lectura.

Criterios de salida:

- Sessions no aparece en búsquedas de pistas, cola global, playlists ni jobs de duplicados.
- No hay análisis automático de proveedores disponible para Sessions.
- El TXT correspondiente se muestra mientras se reproduce la sesión y permanece sin cambios.

### Fase 5: Edición segura avanzada de metadatos

Objetivo: editar archivos originales con vista previa, verificación y deshacer.

Unidades de trabajo:

1. API de capacidades de formatos y editor de metadatos.
2. Normalización canónica de título/artista/remix.
3. Vista previa del nombre de archivo, saneamiento y validación de colisiones.
4. Escritura atómica de etiquetas individual y verificación posterior.
5. Semántica de parches masivos y resultados por archivo.
6. Asignaciones de etiquetas incrustadas de `PersonalGenre`.
7. Revisión y normalización de carátulas.
8. Historial reversible y limpieza de 365 días.
9. Confirmación de eliminación permanente individual y masiva.

Criterios de salida:

- No se escribe sin una vista previa exacta de antes/después cuando la revisión es obligatoria.
- Los campos no compatibles se informan y no se descartan silenciosamente.
- Deshacer restaura etiquetas, carátula, nombre y ruta en fixtures verificados.
- Los elementos fallidos de un lote no invalidan elementos independientes exitosos.

### Fase 6: Identificación y análisis

Objetivo: generar propuestas de metadatos explicables.

Unidades de trabajo:

1. Generación de Chromaprint y consulta de AcoustID.
2. Búsqueda/resolución de MusicBrainz.
3. Enriquecimiento de Discogs.
4. Modelo de proveedor normalizado y procedencia.
5. Puntuación de candidatos y umbrales de confianza.
6. UI de revisión de coincidencias ordenadas con reproducción contextual.
7. Fallback local de BPM y tonalidad.
8. Clasificador de `PersonalGenre` con evidencias y alternativa.

Criterios de salida:

- Las coincidencias ambiguas nunca mutan archivos.
- Cada campo propuesto expone origen y confianza.
- El reanálisis de la biblioteca Main siempre crea una propuesta de aprobación.
- Las propuestas Remember fuerzan `PersonalGenre` a Remember.
- Las tonalidades se escriben en notación Camelot válida.

### Fase 7: Flujo Pending avanzado

Objetivo: procesar la bandeja de entrada de forma segura, desde la detección hasta el movimiento confirmado.

Unidades de trabajo:

1. Estados de Pending, notificaciones y selección predeterminada de archivos no procesados.
2. Inicio manual para todos los archivos nuevos o seleccionados.
3. Espacio de revisión que combina candidatos, carátulas, `PersonalGenre` y mini-reproductor.
4. Aprobación sin autorización de movimiento.
5. Cálculo del destino a partir de `PersonalYear` y `PersonalGenre` aprobado.
6. Vista previa del movimiento y confirmación individual/masiva explícita.
7. Derivación a colisión/duplicado y operación de aplicación recuperable.

Criterios de salida:

- La detección nunca inicia el análisis.
- Los archivos sin cambios analizados previamente no se reprocesan por defecto.
- Ningún archivo se mueve antes de confirmar explícitamente el destino.
- Una colisión bloquea el movimiento sin crear un nombre con sufijo.
- Las pistas completadas aparecen correctamente en Main después de la reconciliación.

### Fase 8: Playlists y duplicados

Objetivo: admitir flujos de escucha y limpieza segura de la biblioteca.

Unidades de trabajo:

1. Playlists manuales.
2. Modelo de reglas de playlists inteligentes y presets iniciales.
3. Integración con la cola.
4. Exportación M3U8 y perfiles de rutas para AIMP/Traktor.
5. Grupos de duplicados exactos.
6. Puntuación, comparación y recomendación de conservación de posibles duplicados.

Criterios de salida:

- Las playlists inteligentes se actualizan cuando cambian los metadatos.
- El orden de las playlists manuales sobrevive a los movimientos de archivos.
- La vista previa de exportación contiene rutas visibles para el host.
- La eliminación de duplicados sigue siendo una acción explícita y confirmada.

### Fase 9: Refuerzo y lanzamiento en NAS

Objetivo: demostrar una operación segura contra una copia representativa antes de montar la biblioteca real.

Unidades de trabajo:

1. Límites de recursos, cancelación, reintentos y apagado ordenado.
2. Logs estructurados y paquete de diagnóstico sin secretos.
3. Instrucciones de backup/restore de base de datos y recuperación de migraciones.
4. Validación de permisos del NAS y mapeo de rutas.
5. Ejecución de aceptación con biblioteca representativa.
6. Documentación de despliegue, actualización, rollback y recuperación ante desastres.
7. Ensayo de despliegue de producción con etiquetas inmutables exactas, backup previo a la migración, notas de compatibilidad y rollback a la etiqueta anterior.

Criterios de salida:

- El flujo completo pasa contra una biblioteca representativa desechable.
- El reinicio durante análisis/etiquetado tiene un resultado seguro documentado.
- El montaje de la biblioteca real no se habilita hasta que el usuario acepta la prueba en seco.
- La documentación operativa explica que el historial de la aplicación no es un backup del NAS.
- La restauración de backup se demuestra desde una base de datos desechable y la documentación distingue rollback de imagen de restauración de base de datos.
- La configuración de producción mantiene los secretos y rutas del host del NAS fuera de Git y registra las etiquetas de imagen inmutables desplegadas.

## 16. Estrategia de verificación

### Pruebas unitarias

- Formateo de metadatos y fallbacks.
- Conversión Camelot.
- Precedencia y evidencias de `PersonalGenre`.
- Puntuación de coincidencias y decisiones de confianza.
- Reglas de playlists inteligentes.
- Cálculo de destinos y nombres de archivo.
- Decisiones de política de raíces.

### Pruebas de integración

- Contratos de API y Problem Details.
- Migraciones, restricciones, transacciones y concurrencia optimista de PostgreSQL mediante EF Core/Npgsql.
- Canonicalización de raíces y rechazo de traversal de rutas.
- Comportamiento de escaneo/reconciliación.
- Round trips de escritura/lectura de etiquetas para cada formato compatible.
- Reemplazo de carátulas y restauración del historial.
- Respuestas Range y caché de waveform.
- Aplicación/movimiento Pending y rollback de colisiones.
- Traducción de rutas M3U8.
- Asociación de tracklists de sesiones.

### Aceptación en ejecución

- Usa una biblioteca fixture desechable que refleje las cuatro raíces.
- Incluye etiquetas malformadas, carátulas ausentes, múltiples frames de carátula, remixes ambiguos, archivos duplicados, colisiones, archivos movidos y sesiones largas.
- Nunca uses el montaje musical de producción para pruebas automatizadas.

## 17. Checklist de finalización del MVP

- [ ] Docker Compose se ejecuta de forma confiable en el NAS.
- [ ] Todas las raíces están indexadas y gobernadas por su política.
- [ ] Los flujos de explorer/búsqueda/filtros/reproductor funcionan desde un navegador de PC.
- [ ] Las etiquetas originales pueden editarse atómicamente y deshacerse durante un año.
- [ ] Los flujos de MusicBrainz, Discogs, AcoustID, BPM, tonalidad y waveform funcionan mediante jobs durables.
- [ ] El reanálisis de Main requiere aprobación.
- [ ] El procesamiento de Pending requiere inicio manual, revisión de `PersonalGenre`/carátula y confirmación del movimiento.
- [ ] Remember permanece en su lugar con `PersonalGenre` Remember.
- [ ] Las playlists inteligentes/manuales y las exportaciones M3U8 funcionan para rutas de AIMP/Traktor.
- [ ] Los duplicados se identifican y comparan sin eliminación automática.
- [ ] Sessions permanece independiente y muestra sus tracklists TXT.
- [ ] La UI en español, los temas claro/oscuro, los atajos de teclado y las notificaciones persistentes están completos.
- [ ] La prueba en seco con biblioteca representativa se aprueba antes del uso en producción.

## 18. Backlog futuro

- Acceso remoto protegido por Authelia.
- Evaluar acceso compatible con OpenSubsonic para aplicaciones externas de Windows y Android. OpenSubsonic es la evolución abierta de la API Subsonic heredada y proporciona un objetivo de interoperabilidad para clientes de escritorio y móviles. En el momento de la implementación, compara los servidores compatibles con OpenSubsonic mantenidos actualmente y un adaptador expuesto por esta aplicación; no preselecciones ahora un servidor. Decide si despliegas el servidor seleccionado junto a esta aplicación o si expones únicamente una API compatible, y hazlo solo después de demostrar compatibilidad representativa de clientes Windows y Android para pistas catalogadas y Sessions, streaming con HTTP Range o transcodificación según corresponda, carátulas y metadatos, y playlists cuando las reglas del producto lo permitan. Conserva Sessions como colección principal separada aunque un límite de compatibilidad la presente a los clientes. Documenta autenticación, autorización, seguridad de transporte, limitación de frecuencia y riesgos de exposición antes de cualquier uso fuera de la LAN.
- Proveedor autorizado de Beatport.
- Importación manual asistida de URL de Beatport.
- Marcas de tiempo y marcadores de waveform de pistas de sesiones.
- Click-to-seek en las entradas del tracklist.
- Edición de tracklists.
- Exportación Traktor NML.
- Ajuste de las sugerencias de `PersonalGenre` a partir de decisiones aprobadas.
