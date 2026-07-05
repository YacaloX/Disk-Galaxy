# Changelog

Todas las cambios notables en este proyecto se documentarán aquí.

## [Unreleased]

### Added
- README, LICENSE, CHANGELOG y CONTRIBUTING para publicación open-source.

## [0.5.0] — 2026-07-04

### Fixed
- Ajuste de posición y pitch por defecto de la cámara; corrección de sintaxis en plantillas Serilog.

### Added
- Proyecto de test RenderTest a la solución.

## [0.4.0] — 2026-07-04

### Fixed
- Protección del bucle de errores GL; uso de Stopwatch para temporización precisa del delta.

### Added
- Soporte de `RightCtrl` para movimiento hacia abajo.

### Changed
- Composición del estado visual de nodos mediante sistema de prioridad `EffectiveColor`.
- Corrección de visibilidad de listas de error/búsqueda con conversor `IsPositive`.

### Refactored
- Extracción de `FileSizeFormatter`; corrección de boxing en `HasFlag`; eliminación de import no usado.

## [0.3.0] — 2026-07-03

### Added
- Skybox con campo de estrellas.
- Manejo de casos extremos y soporte de rutas largas.

### Fixed
- Progreso de escaneo siempre indeterminado; clarificación de nombres de brillo de aristas.

## [0.2.0] — 2026-07-03

### Added
- Paneles de Inspector, Búsqueda y Filtros.
- Barra de estado con FPS y conteo de nodos.

### Changed
- Cúmulos de galaxias, ray picking, transiciones de cámara, LOD.

## [0.1.0] — 2026-07-02

### Added
- Renderizado de grafo 3D con visualización jerárquica del sistema de archivos.

## [0.0.2] — 2026-07-01

### Added
- Escáner de sistema de archivos con progreso en segundo plano y seguridad de symlinks.
- Modelos de dominio: FileStar, FolderGalaxy, FileCategory, ScanProgress, ScanResult.

## [0.0.1] — 2026-06-30

### Added
- Estructura inicial de solución con Avalonia UI 11.1, Silk.NET 2.23 y .NET 8.

[Unreleased]: https://github.com/YacaloX/Disk-Galaxy/compare/v0.5.0...HEAD
[0.5.0]: https://github.com/YacaloX/Disk-Galaxy/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/YacaloX/Disk-Galaxy/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/YacaloX/Disk-Galaxy/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/YacaloX/Disk-Galaxy/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/YacaloX/Disk-Galaxy/compare/v0.0.2...v0.1.0
[0.0.2]: https://github.com/YacaloX/Disk-Galaxy/compare/v0.0.1...v0.0.2
[0.0.1]: https://github.com/YacaloX/Disk-Galaxy/releases/tag/v0.0.1
