# Instrucciones para agentes

> Traducción al español. El documento original en inglés se conserva en `../../AGENTS.md`.

Estas reglas son obligatorias para todas las personas y agentes de IA que contribuyan al proyecto.

## Comenzar aquí

1. Lee `README.md`, `PLAN.md` y `DECISIONS.md` antes de proponer o implementar cambios.
2. Identifica la fase y la unidad de trabajo activas en `PLAN.md`.
3. Inspecciona el código existente antes de elegir una implementación.
4. No reinterpretes una regla de producto. Registra una ambigüedad genuina y formula una pregunta concreta.

## Habilidades y MCP del proyecto

- Carga `.opencode/skills/execute-plan-work-unit/SKILL.md` para toda implementación o corrección de una unidad de trabajo de `PLAN.md`.
- Carga `.opencode/skills/dotnet-vertical-slice/SKILL.md` al cambiar slices de API, contratos o comportamiento de FluentResults o FluentValidation.
- Carga `.opencode/skills/audio-filesystem-safety/SKILL.md` antes de tocar el escaneo, las etiquetas, las carátulas o el comportamiento de renombrado, movimiento, eliminación o reconciliación.
- Carga `.opencode/skills/frontend-design/SKILL.md` antes de diseñar o refactorizar visualmente la UI, los layouts, los temas o los estados de UI de Blazor.
- Usa Blazor Blueprint MCP para consultar las API exactas de componentes, la configuración, los patrones y los cambios de versión. No adivines parámetros de componentes.
- Usa Playwright MCP únicamente cuando una UI en ejecución necesite inspección interactiva o verificación manual de flujo. No sustituye las pruebas automatizadas de Playwright .NET.
- Mantén las consultas MCP acotadas y específicas para evitar contexto innecesario.

## Límites del producto

- El producto es una aplicación web de un solo usuario que se ejecuta únicamente en Docker sobre un NAS Linux x86-64.
- El MVP está limitado a la red local y no tiene autenticación ni autorización.
- Mantén separadas la API ASP.NET Core y la interfaz Blazor. La interfaz no debe acceder directamente a la base de datos, al sistema de archivos, a los proveedores de metadatos ni a las herramientas de audio.
- Sessions es una colección separada. No la incluyas en el catálogo de pistas, la cola global, las playlists, la detección de duplicados ni el análisis automático.
- Nunca muevas automáticamente archivos de Main o Remember.
- Nunca analices ni modifiques automáticamente archivos catalogados solo porque ocurrió un evento del sistema de archivos.
- Nunca elimines automáticamente un archivo. La eliminación permanente siempre requiere confirmación explícita.
- Nunca sobrescribas una coincidencia de proveedor de baja confianza o ambigua.
- Nunca muevas una pista pendiente sin mostrar y confirmar su origen exacto, destino y nombre de archivo final.
- No añadas scraping automático de Beatport como proveedor. La integración de Beatport requiere acceso autorizado a la API; la importación manual asistida es un elemento futuro.

## Arquitectura

- `DjTracksSessions.Api` es propietaria de `Features` organizadas como vertical slices; cada slice contiene su endpoint, consultas, comandos, validaciones, mappers, handlers y pruebas de comportamiento, no carpetas técnicas globales como `Controllers`, `Services` o `Repositories`.
- Los slices usan `DjTrackSessions.Domain` para entidades e invariantes y los adaptadores de `DjTrackSessions.Infrastructure`; no muevas la persistencia ni las implementaciones externas a la API.
- `DjTrackSessions.Domain` es propietaria de las entidades, objetos de valor e invariantes independientes del framework; no debe referenciar EF Core.
- `DjTrackSessions.Infrastructure` es propietaria del `DbContext` Code First de EF Core, las asignaciones `IEntityTypeConfiguration<T>` de Fluent, las migraciones y los adaptadores de sistema de archivos, base de datos, proveedores, imágenes y herramientas de audio.
- Usa Entity Framework Core Code First con el proveedor Npgsql para toda la persistencia de la aplicación. PostgreSQL es la única base de datos de producción compatible.
- Crea cambios de esquema mediante migraciones revisadas de EF Core. No uses `EnsureCreated` al iniciar la aplicación ni para desplegar en producción.
- Devuelve los fallos esperados mediante `FluentResults`. No uses excepciones para validaciones, coincidencias inexistentes, colisiones u otros resultados esperados.
- Usa `FluentValidation` de forma explícita y asíncrona. No dependas de la validación automática síncrona de ASP.NET.
- Mapea los fallos de la API a un único contrato coherente de Problem Details con códigos de error estables y legibles por máquina.
- Mantén los contratos de la API independientes de las entidades de persistencia y los modelos de vista de Blazor.
- Usa UTC para las marcas de tiempo almacenadas y fechas locales explícitas para los años personales o de lanzamiento.

## Seguridad del sistema de archivos

- Trata las raíces montadas configuradas como límites de seguridad. Rechaza las rutas que escapen de su raíz después de la canonicalización.
- Identifica los archivos de Track Management mediante una raíz permitida y una ruta relativa canonicalizada. No requieras un ID persistido para explorar, mostrar detalle, editar metadatos ni mover Pending.
- Escribe las etiquetas atómicamente a través de un archivo temporal en el mismo sistema de archivos y reemplázalo solo después de verificarlo.
- Conserva las etiquetas no compatibles y desconocidas salvo que una regla específica del formato indique explícitamente eliminarlas.
- Conserva exactamente una imagen de carátula incrustada después de una operación de carátula aprobada.
- Detecta las colisiones de destino antes de renombrar o mover. Nunca inventes sufijos numéricos para los nombres de archivo.
- Una operación por lotes debe informar el éxito o el fallo de cada archivo y no ocultar una finalización parcial.
- Los watchers del sistema de archivos son indicios. Reconcilia cada raíz periódicamente porque se pueden perder eventos del NAS.

## Invariantes de metadatos

- Formatea los artistas como `Artist 1, Artist 2`.
- Formatea los remixes como `Title (Remixer Remix)` tanto en la etiqueta de título como en el nombre de archivo.
- Formatea los nombres de archivo como `Artist 1, Artist 2 - Title (Remixer Remix).ext`.
- Almacena por separado los valores del proveedor, personales y efectivos.
- `EffectiveYear = ProviderYear ?? PersonalYear`.
- `EffectiveGenre = ProviderGenre ?? PersonalGenre`.
- `PersonalGenre` es obligatorio para las pistas ordinarias.
- Las pistas Remember siempre usan `PersonalGenre=Remember`.
- MP3 almacena el género personal como `TXXX:PERSONAL_GENRE`; define asignaciones equivalentes explícitas para cada formato compatible.
- Escribe la tonalidad musical en notación Camelot.
- El BPM y la tonalidad del proveedor tienen precedencia sobre el análisis local cuando su procedencia es confiable.

## Reglas de UI

- El texto visible para el usuario está en español. El código, los identificadores, los comentarios, los contratos de API y la documentación técnica permanecen en inglés.
- Los archivos Razor bajo `src/DjTracksSessions.Web` contienen únicamente markup y directivas; mantén todo el código C# en el archivo de code-behind `.razor.cs` adyacente, dentro de su clase parcial.
- Optimiza para un navegador de escritorio. El diseño específico para móviles está fuera de alcance.
- Conserva un límite claro entre `Explorer / Player` y `Analyzer / Tagger`.
- Usa componentes Blazor Blueprint y sus temas claro/oscuro antes de crear primitivas personalizadas.
- Mantén las acciones destructivas visualmente diferenciadas y exige confirmación.
- Los atajos de teclado no deben activarse mientras se escribe en un campo ni omitir confirmaciones.
- Solo un elemento de audio puede reproducir a la vez; los reproductores global y contextual se coordinan mediante un único servicio de reproducción del frontend.

## Pruebas y entrega

- Las pruebas se entregan junto con el comportamiento que verifican.
- Prefiere pruebas unitarias centradas en comportamiento para reglas puras y pruebas de integración para contratos de API, PostgreSQL, sistema de archivos, escritura de etiquetas y adaptadores externos.
- Ejecuta las pruebas de integración de persistencia contra contenedores PostgreSQL desechables. No sustituyas el comportamiento de PostgreSQL por EF Core InMemory o SQLite.
- Cada prueba del sistema de archivos usa una raíz temporal aislada y archivos de muestra desechables.
- Nunca ejecutes pruebas destructivas contra una biblioteca musical real montada.
- Las pruebas de proveedores usan fixtures registrados o dobles de prueba por defecto; las pruebas en vivo deben habilitarse explícitamente.
- Cada unidad de trabajo debe documentar su comando de prueba específico, la verificación en ejecución y el límite de rollback.
- Mantén los commits como unidades de comportamiento revisables y usa Conventional Commits sin atribución de IA.
- No marques una fase como completada hasta que pasen todos los criterios de salida de `PLAN.md`.

## Mantenimiento del plan

- Actualiza `PLAN.md` cuando cambien el alcance o el orden.
- Actualiza `DECISIONS.md` cuando cambie una decisión de producto o arquitectura.
- No añadas silenciosamente capas de compatibilidad, hosts alternativos, comportamiento multiusuario, despliegue en la nube ni funcionalidades futuras.
