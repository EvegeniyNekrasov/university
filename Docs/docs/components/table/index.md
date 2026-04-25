# Table Component

`Table` es un componente genérico para Blazor Web App diseñado para renderizar tablas de datos con funcionalidades avanzadas.

El componente está inspirado en patrones habituales de DataTables, pero está implementado de forma nativa en Blazor y no depende de jQuery ni de DataTables.js.

## Funcionalidades principales

- Búsqueda global.
- Filtros por columna.
- Filtros por tipo.
- SearchPanes.
- Buscador dentro de SearchPanes.
- Paginación.
- Ordenación simple.
- Multi-sort.
- Selección de filas.
- Filas clicables.
- Filas expandibles.
- Acciones por fila.
- Toolbar personalizada.
- Exportación CSV.
- Columnas ocultables.
- Columnas ocultas pero filtrables.
- Sticky header.
- Responsive cards.
- Loading skeleton.
- Persistencia de estado.
- Modo cliente.
- Modo servidor.

## Ubicación

```txt
Components/UiComponents/Table/
```

Archivos:

```txt
Table.razor
Table.razor.cs
TableColumn.razor
TableModels.cs
Table.razor.css
```

Archivo JavaScript auxiliar:

```txt
wwwroot/js/tableInterop.js
```

## Ejemplo mínimo

```razor
<Table TItem="User"
       Items="Users"
       Title="Usuarios"
       RowKey="@(u => u.Id)">

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)" />

        <TableColumn TItem="User"
                     Key="email"
                     Title="Email"
                     Value="@(u => u.Email)" />
    </ChildContent>
</Table>
```

## Cuándo usar modo cliente

Usar modo cliente cuando:

- el dataset es pequeño;
- los datos ya están en memoria;
- no hay requisitos especiales de seguridad;
- no se espera un crecimiento elevado de registros.

## Cuándo usar modo servidor

Usar modo servidor cuando:

- el dataset es grande;
- los datos vienen de base de datos;
- se requiere paginación real;
- hay reglas de permisos;
- se necesitan filtros eficientes;
- se trabaja con 10.000 o más registros.

Para 50.000 o más registros, usar modo servidor.