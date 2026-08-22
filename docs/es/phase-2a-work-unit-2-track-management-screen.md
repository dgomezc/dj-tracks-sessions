# Fase 2A, unidad 2: Pantalla de gestión de pistas de solo lectura

> Traducción al español. El documento original en inglés se conserva en `../../phase-2a-work-unit-2-track-management-screen.md`.

## Resultado

Entregar la primera parte completa de la pantalla de gestión de pistas de escritorio en español con exploración directa de los archivos configurados de Main, Pending y Remember. Cierra la experiencia de exploración/lectura de etiquetas antes de iniciar cualquier pantalla de reproductor o Sessions. No existe un escaneo persistido ni un ID persistido de pista en este flujo.

## Alcance

- Añadir contratos y slices API respaldados por el sistema de archivos solo para Main, Pending y Remember. Resolver cada ruta solicitada bajo su raíz canónica y exponer únicamente rutas relativas.
- Añadir una cuadrícula densa de escritorio con selección de raíz, búsqueda de texto básica, filtros de año y PersonalGenre, detalle de pista y estados claros de carga, vacío, fallo y no disponible. Leer las propiedades técnicas y etiquetas actuales de cada archivo; las etiquetas del archivo son la fuente de verdad.
- Mostrar fallos individuales de extracción sin ocultar los archivos leídos correctamente.
- Mantener el acceso de Web únicamente a través de contratos API.

## Criterios de aceptación

- Una persona puede explorar cada raíz ordinaria de forma distinta sin un escaneo previo, buscar metadatos actuales e inspeccionar las propiedades técnicas y etiquetas actuales desde la pantalla de gestión de pistas en español.
- Los registros de Sessions nunca aparecen en la ruta, respuesta de consulta, cuadrícula, valores de filtros, detalle ni futuros controles de cola de esta pantalla.
- Ningún control escribe medios, mueve archivos, inicia análisis de proveedores, encola audio ni se actualiza automáticamente.
- Los escapes por symlink y traversal se rechazan; los archivos ausentes, ilegibles, corruptos o no compatibles producen errores visibles por archivo sin ocultar los archivos legibles.
- Las pruebas API/UI específicas cubren la separación de raíces y etiquetas de navegación/acción durables en español.

## Límites explícitos

- Esta unidad no añade reproducción, cola, edición de metadatos, movimientos Pending, acciones masivas, acciones de proveedores, watchers, reconciliación programada ni actualización automática.
- No crea una página de Sessions. Sessions comienza únicamente después de cerrar la pantalla de gestión de pistas y completar la pantalla del reproductor.

## Verificación y rollback

Registra el comando de prueba específico, resultado exacto, escenario de ejecución con raíces desechables y límite de rollback al implementar la unidad. No accedas a una raíz musical configurada ni de producción. El rollback se limita a los cambios de browse/UI y pruebas de fixtures de esta unidad.
