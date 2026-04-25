# SearchPanes

SearchPanes son paneles de filtrado rápido basados en valores únicos de una columna.

Permiten filtrar datos seleccionando opciones como:

```txt
Rol
Estado
Departamento
Año
Categoría
```

## Activar SearchPanes

```razor
<Table EnableSearchPanes="true">
```

Configuración recomendada:

```razor
<Table EnableSearchPanes="true"
       SearchPanesInitialCollapsed="true"
       SearchPanesCascade="true"
       SearchPanesViewTotal="true"
       SearchPanesColumns="3">
```

## Activar SearchPane en una columna

```razor
<TableColumn TItem="User"
             Key="role"
             Title="Rol"
             Value="@(u => u.Role)"
             SearchPane="true" />
```

## Desactivar SearchPane en una columna

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)"
             SearchPane="false" />
```

## SearchPane con valor agrupado

Ejemplo: mostrar una fecha en la tabla, pero agrupar el pane por año.

```razor
<TableColumn TItem="User"
             Key="createdAt"
             Title="Alta"
             Value="@(u => u.CreatedAt)"
             SearchPane="true"
             SearchPaneTitle="Año"
             SearchPaneValue="@(u => u.CreatedAt.Year.ToString())" />
```

## Selección dentro de un pane

Dentro de un mismo pane, varias opciones funcionan como OR.

Ejemplo:

```txt
Rol = Admin OR Manager
```

## Selección entre panes

Entre distintos panes, las opciones funcionan como AND.

Ejemplo:

```txt
Rol = Admin
AND
Activo = Sí
```

## Cascading panes

Con:

```razor
SearchPanesCascade="true"
```

las opciones de un pane se recalculan según las selecciones activas en otros panes.

## View total

Con:

```razor
SearchPanesViewTotal="true"
```

cada opción muestra contador visible y total.

Ejemplo:

```txt
Admin 10 / 32
```

## Buscador interno

Con:

```razor
EnableSearchPaneSearch="true"
```

cada pane muestra un buscador para filtrar sus opciones.

## Ocultar panes por defecto

```razor
SearchPanesInitialCollapsed="true"
```

Si `PersistState="true"`, el estado guardado puede prevalecer sobre esta configuración inicial.

Para reiniciar el estado:

```razor
StateKey="users-table-v2"
```

## SearchPane en columna oculta

```razor
<TableColumn TItem="User"
             Key="department"
             Title="Departamento"
             Value="@(u => u.Department)"
             ShowByDefault="false"
             SearchPane="true"
             SearchPaneWhenHidden="true" />
```

## SearchPaneProvider en modo servidor

En modo servidor, los panes se cargan mediante:

```razor
<Table ServerSide="true"
       SearchPaneProvider="LoadUserSearchPanesAsync">
```

Ejemplo simplificado:

```csharp
private async Task<IReadOnlyList<TableSearchPane>> LoadUserSearchPanesAsync(
    TableDataRequest request)
{
    var query = DbContext.Users.AsNoTracking();

    var roleOptions = await query
        .GroupBy(u => u.Role)
        .Select(g => new TableSearchPaneOption(
            g.Key,
            g.Key,
            g.Count(),
            g.Count(),
            false))
        .ToListAsync();

    return new List<TableSearchPane>
    {
        new TableSearchPane(
            ColumnKey: "role",
            Title: "Rol",
            Options: roleOptions,
            SelectedCount: 0)
    };
}
```

## Recomendaciones

- Usar SearchPanes en columnas con pocos valores únicos.
- Evitar SearchPanes en columnas como email, nombre, identificadores o textos largos.
- En datasets grandes, calcular SearchPanes en backend.
- Usar `SearchPanesInitialCollapsed="true"` si hay varios panes.
- Usar `SearchPaneValue` para agrupar fechas o valores complejos.