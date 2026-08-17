# Despliegue de prueba en el NAS

Usa el [manual del NAS](nas-manual.md). Esta página se conserva como redirección breve para el flujo de pruebas.

El flujo es manual y se ejecuta en el propio NAS solo cuando existe una versión utilizable de desarrollo/pruebas. Clona o actualiza allí la versión seleccionada, prepara la configuración local ignorada y raíces desechables, ejecuta la migración documentada e inicia allí el Compose base. El proyecto no usa SSH, `scp`, transferencia WSL-NAS, archivos de imágenes, registry ni scripts de despliegue automatizado.

La instancia PostgreSQL del NAS es PostgreSQL externo de desarrollo. El proyecto no despliega un contenedor ni un volumen PostgreSQL. Nunca uses la biblioteca musical real para esta prueba.
