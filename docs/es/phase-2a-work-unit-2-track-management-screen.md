# Fase 2A, unidad 2: Pantalla de gestión de pistas de solo lectura

> Traducción al español. El documento original en inglés se conserva en `../../phase-2a-work-unit-2-track-management-screen.md`.

## Resultado

Entregar la primera parte completa de la pantalla de gestión de pistas de escritorio en español sobre el escaneo explícito persistido en `b1f4df6`. Cierra la experiencia de escaneo/exploración de solo lectura antes de iniciar cualquier pantalla de reproductor o Sessions.

## Alcance

- Añadir contratos de consulta y slices API del catálogo ordinario solo para Main, Pending y Remember.
- Añadir una acción de escaneo manual y una cuadrícula densa de escritorio con selección de raíz, búsqueda de texto básica, filtros de año y PersonalGenre, detalle de pista y estados claros de carga, vacío, fallo y no disponible.
- Mostrar los fallos de escaneo y extracción sin ocultar las pistas indexadas correctamente.
- Mantener el acceso de Web únicamente a través de contratos API.

## Criterios de aceptación

- Una persona puede iniciar un escaneo manual, explorar cada raíz ordinaria de forma distinta, buscar metadatos conocidos e inspeccionar su snapshot técnico/de etiquetas actuales desde la pantalla de gestión de pistas en español.
- Los registros de Sessions nunca aparecen en la ruta, respuesta de consulta, cuadrícula, valores de filtros, detalle ni futuros controles de cola de esta pantalla.
- Ningún control escribe medios, mueve archivos, inicia análisis de proveedores, encola audio ni se actualiza automáticamente.
- Las pruebas API/UI específicas cubren la separación de raíces y etiquetas de navegación/acción durables en español.

## Límites explícitos

- Esta unidad no añade reproducción, cola, edición de metadatos, movimientos Pending, acciones masivas, acciones de proveedores, watchers, reconciliación programada ni actualización automática.
- No crea una página de Sessions. Sessions comienza únicamente después de cerrar la pantalla de gestión de pistas y completar la pantalla del reproductor.

## Verificación y rollback

Registra el comando de prueba específico, resultado exacto, escenario de ejecución con raíces desechables y límite de rollback al implementar la unidad. No accedas a una raíz musical configurada ni de producción. El rollback se limita a los cambios de consulta/UI y pruebas de esta unidad; la persistencia de escaneo de `b1f4df6` permanece intacta.
