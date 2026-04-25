# Instalación y configuración

Esta sección describe la configuración básica necesaria para usar los componentes documentados.

## Namespaces

Los componentes reutilizables se encuentran bajo:

```txt
Components/UiComponents/
```

Para usar un componente, debe importarse su namespace en `_Imports.razor`.

Ejemplo para el componente `Table`:

```razor
@using TuProyecto.Components.UiComponents.Table
```

Sustituir `TuProyecto` por el namespace raíz real de la aplicación.

## Render mode

En páginas interactivas, añadir:

```razor
@rendermode InteractiveServer
```

Ejemplo:

```razor
@page "/usuarios"
@rendermode InteractiveServer
```

## JavaScript interop

Algunos componentes pueden requerir archivos JavaScript auxiliares en `wwwroot`.

El componente `Table` utiliza:

```txt
wwwroot/js/tableInterop.js
```

Este archivo se usa para:

- exportación CSV;
- localStorage;
- persistencia de estado.

## CSS aislado

Los componentes pueden usar CSS aislado de Blazor.

Ejemplo:

```txt
Table.razor
Table.razor.css
```

El archivo `.razor.css` debe estar en la misma carpeta y compartir el mismo nombre base que el componente.

## Recomendación

Para cada componente documentado, revisar su página específica de instalación y uso antes de integrarlo en una página nueva.