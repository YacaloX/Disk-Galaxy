# Disk Galaxy

**Visualiza tu disco duro como una galaxia de estrellas en 3D.**

Disk Galaxy escanea tu sistema de archivos y representa cada carpeta y archivo como nodos brillantes en un espacio 3D interactivo, creando una "galaxia" única que refleja la estructura y el tamaño de tus datos.

![Disk Galaxy](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Avalonia](https://img.shields.io/badge/Avalonia-11.1-8B5CFE)
![Silk.NET](https://img.shields.io/badge/Silk.NET-2.23-EA4335)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Características

- **Escaneo del sistema de archivos** — Escanea directorios de forma recursiva con reporte de progreso en tiempo real. Soporta detección de symlinks, límite de profundidad, filtros de tamaño y rutas largas.
- **Visualización 3D** — Renderiza la jerarquía de archivos como un grafo 3D usando OpenGL (Silk.NET):
  - **Nodos**: Archivos y carpetas como quads brillantes (point sprites), con tamaño proporcional al peso del archivo y color según categoría.
  - **Aristas**: Líneas que conectan nodos padre-hijo con brillo variable por distancia.
  - **Cúmulos**: Carpetas representadas como cúmulos de partículas esféricas.
  - **Skybox**: 3,000 estrellas de fondo para la ambientación espacial.
- **Cámara 3D** — Navegación libre (WASD, ratón para mirar, scroll para zoom). Doble clic para "volar" a un nodo.
- **Panel de inspección** — Haz clic en cualquier nodo para ver sus propiedades (nombre, ruta, tamaño, categoría, padre).
- **Búsqueda** — Búsqueda en vivo con debounce de 150ms, resalta nodos coincidentes.
- **Filtros** — Muestra/oculta categorías de archivos, rango de tamaños, archivos recientes/grandes, presets.
- **Clasificación por categorías** — 12 categorías (Imagen, Video, Documento, Audio, Archivo, Ejecutable, Código, Fuente, Base de Datos, Configuración, Temporal, Desconocido) con colores distintivos.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Sistema operativo: Windows, Linux o macOS
- GPU con soporte OpenGL 3.3+

## Instalación y uso

```bash
# Clonar el repositorio
git clone https://github.com/YacaloX/Disk-Galaxy.git
cd Disk-Galaxy

# Ejecutar en modo Debug
dotnet run --project src/DiskGalaxy.UI

# O compilar y ejecutar en modo Release
dotnet run --configuration Release --project src/DiskGalaxy.UI
```

### Controles

| Acción | Teclado / Ratón |
|---|---|
| Moverse adelante/atrás | `W` / `S` |
| Moverse izquierda/derecha | `A` / `D` |
| Moverse arriba/abajo | `Espacio` / `Ctrl Izquierdo` — `Ctrl Derecho` para bajar |
| Mirar alrededor | Arrastrar con el ratón |
| Zoom | Scroll del ratón |
| Seleccionar nodo | Clic izquierdo |
| Volar a nodo | Doble clic |
| Salir | `Esc` |

## Arquitectura del proyecto

```
Disk-Galaxy/
├── src/
│   ├── DiskGalaxy.Core/          # Modelos de dominio y escáner de archivos
│   ├── DiskGalaxy.Rendering/     # Motor de renderizado OpenGL (Silk.NET)
│   └── DiskGalaxy.UI/            # Interfaz de usuario (Avalonia)
├── tests/
│   └── RenderTest/               # Prueba de humo OpenGL standalone
└── DiskGalaxy.sln
```

## Tecnologías

| Componente | Tecnología |
|---|---|
| Plataforma | .NET 8 (C# 12) |
| UI | Avalonia 11.1 (multiplataforma) |
| 3D | Silk.NET 2.23 (OpenGL) |
| MVVM | CommunityToolkit.Mvvm 8.2 |
| DI | Microsoft.Extensions.DependencyInjection |
| Logging | Serilog |

## Licencia

Este proyecto está bajo la licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.
