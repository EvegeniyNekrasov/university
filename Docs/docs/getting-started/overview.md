# Resumen

Este proyecto utiliza Blazor Web App y una colección de componentes reutilizables para construir interfaces consistentes.

La documentación técnica tiene como objetivo explicar:

- cómo instalar y usar componentes;
- cómo configurar páginas;
- cómo trabajar con datos;
- cómo aplicar seguridad;
- cómo evitar problemas de rendimiento;
- cómo mantener una estructura de código clara.

## Principios del proyecto

Los componentes deben cumplir los siguientes principios:

- Ser reutilizables.
- Tener una API clara.
- Separar markup y lógica cuando el componente sea complejo.
- Evitar lógica repetida en páginas.
- Permitir modo cliente y modo servidor cuando sea necesario.
- Ser seguros frente a entrada no confiable.
- Ser documentados con ejemplos reales.

## Estructura recomendada

Los componentes UI reutilizables se ubican en:

```txt
Components/UiComponents/
```

Ejemplo:

```txt
Components/UiComponents/Table/
```

Cada componente complejo puede estar compuesto por:

```txt
Component.razor
Component.razor.cs
Component.razor.css
ComponentModels.cs
```

## Renderizado interactivo

Las páginas que usen componentes interactivos deben declarar el render mode correspondiente.

Ejemplo:

```razor
@rendermode InteractiveServer
```

## Documentación de componentes

Cada componente debe documentar:

- descripción;
- ubicación;
- parámetros;
- ejemplos básicos;
- ejemplos avanzados;
- rendimiento;
- seguridad;
- problemas comunes.