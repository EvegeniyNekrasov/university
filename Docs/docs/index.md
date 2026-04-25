# Documentación del Proyecto

Esta documentación describe los componentes, patrones y convenciones técnicas utilizados en el proyecto.

El objetivo es proporcionar una referencia clara para que los desarrolladores puedan implementar funcionalidades de forma consistente, segura y mantenible.

## Contenido principal

- Primeros pasos
- Componentes reutilizables
- Arquitectura del proyecto
- Convenciones técnicas
- Guías de uso
- Buenas prácticas
- Resolución de problemas

## Componentes disponibles

Actualmente la documentación incluye:

- `Table`: componente avanzado de tabla para Blazor Web App.

## Convenciones generales

Los componentes documentados deben usarse siguiendo estas reglas:

- Mantener una API declarativa.
- Evitar duplicar lógica en páginas.
- Centralizar comportamiento común en componentes reutilizables.
- Usar nombres estables para claves técnicas.
- Validar siempre datos recibidos desde cliente.
- Aplicar criterios de rendimiento cuando el dataset sea grande.

## Estructura de la documentación

La documentación se organiza por secciones:

```txt
getting-started/
components/
architecture/
changelog.md
```

La documentación de cada componente se divide por tema para facilitar su mantenimiento.