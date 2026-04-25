# Uso rápido

Esta guía muestra un ejemplo mínimo de uso del componente `Table`.

## Página de ejemplo

```razor
@page "/usuarios"
@rendermode InteractiveServer

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

@code {
    private List<User> Users = new()
    {
        new(1, "Ana García", "ana@app.com"),
        new(2, "Luis Pérez", "luis@app.com")
    };

    private record User(
        int Id,
        string Name,
        string Email);
}
```

## Requisitos

En `_Imports.razor`:

```razor
@using TuProyecto.Components.UiComponents.Table
```

En la página:

```razor
@rendermode InteractiveServer
```

## Reglas básicas

Usar siempre `RowKey`:

```razor
RowKey="@(u => u.Id)"
```

Usar siempre `Key` en las columnas:

```razor
Key="email"
```

Cuando se usen bloques como `RowDetails`, `RowActions`, `ToolbarActions` o `EmptyTemplate`, las columnas deben ir dentro de:

```razor
<ChildContent>
    ...
</ChildContent>
```