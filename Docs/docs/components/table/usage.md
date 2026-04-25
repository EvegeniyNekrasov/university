# Uso del componente Table

## Requisitos

En `_Imports.razor`:

```razor
@using TuProyecto.Components.UiComponents.Table
```

En páginas interactivas:

```razor
@rendermode InteractiveServer
```

## Uso básico

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

## `TItem`

`TItem` representa el tipo de dato de cada fila.

Ejemplo:

```razor
<Table TItem="User">
```

Si la tabla muestra usuarios, `TItem` será `User`.

## `RowKey`

`RowKey` identifica cada fila de forma única.

Ejemplo:

```razor
RowKey="@(u => u.Id)"
```

Debe usarse siempre que sea posible.

Se utiliza para:

- selección de filas;
- filas expandibles;
- renderizado estable;
- persistencia de estado;
- evitar comportamientos inconsistentes.

## `Key` en columnas

Cada columna debe tener un identificador estable.

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)" />
```

El `Key` se usa para:

- filtros;
- ordenación;
- SearchPanes;
- visibilidad de columnas;
- persistencia;
- modo servidor;
- exportación.

## Uso con contenido avanzado

Cuando se usan fragments como `ToolbarActions`, `RowDetails`, `RowActions` o `EmptyTemplate`, las columnas deben ir dentro de `<ChildContent>`.

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)">

    <ToolbarActions>
        <button>Nuevo usuario</button>
    </ToolbarActions>

    <RowDetails Context="u">
        <div>@u.Email</div>
    </RowDetails>

    <RowActions Context="u">
        <button @onclick="() => EditUser(u)">Editar</button>
    </RowActions>

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)" />
    </ChildContent>
</Table>
```

## Error común

Este uso es incorrecto cuando hay otros fragments:

```razor
<Table TItem="User" Items="Users">
    <RowDetails Context="u">
        <div>@u.Email</div>
    </RowDetails>

    <TableColumn TItem="User"
                 Key="name"
                 Title="Nombre"
                 Value="@(u => u.Name)" />
</Table>
```

Puede producir:

```txt
RZ9996: Unrecognized child content inside component 'Table'
```

## Modo cliente

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)">
```

En modo cliente, el componente procesa en memoria:

- búsqueda;
- filtros;
- ordenación;
- SearchPanes;
- paginación.

## Modo servidor

```razor
<Table TItem="User"
       ServerSide="true"
       DataProvider="LoadUsersAsync"
       RowKey="@(u => u.Id)">
```

En modo servidor, el componente envía un `TableDataRequest` y el backend devuelve una página de resultados.