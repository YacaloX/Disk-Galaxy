# Contribuyendo a Disk Galaxy

¡Gracias por tu interés en contribuir a Disk Galaxy! Este documento describe el proceso para colaborar.

## Código de conducta

Al participar en este proyecto, te comprometes a mantener un ambiente respetuoso e inclusivo.

## Reportar issues

- Usa el [rastreador de issues](https://github.com/YacaloX/Disk-Galaxy/issues) para reportar bugs o solicitar features.
- Antes de crear un issue, verifica que no exista uno similar.
- Incluye información del sistema, pasos para reproducir el bug, y logs si es posible.

## Enviar Pull Requests

1. **Fork** el repositorio y crea una rama desde `main`.
2. **Desarrolla** tu cambio siguiendo el estilo del código existente.
3. **Ejecuta** la build:
   ```bash
   dotnet build
   ```
4. **Asegúrate** de que no haya errores de análisis (`TreatWarningsAsErrors` está activado).
5. **Commit** con mensajes descriptivos usando [conventional commits](https://www.conventionalcommits.org/):
   - `feat:` para nuevas características
   - `fix:` para correcciones
   - `refactor:` para refactorización
   - `chore:` para tareas de mantenimiento
6. **Push** a tu fork y abre un Pull Request hacia `main`.

## Guía de estilo

- El proyecto usa C# 12 con nullable habilitado e `ImplicitUsings`.
- Los warnings se tratan como errores.
- Busca mantener el código limpio y bien estructurado.
- Para el renderizado, prioriza rendimiento sobre complejidad.

## Estructura del proyecto

```
src/
├── DiskGalaxy.Core/       # Modelos de dominio y lógica de negocio
├── DiskGalaxy.Rendering/  # Motor de renderizado OpenGL
└── DiskGalaxy.UI/         # Interfaz de usuario (Avalonia + MVVM)
```

## Licencia

Al contribuir, aceptas que tus contribuciones se licencien bajo la [MIT License](LICENSE).
