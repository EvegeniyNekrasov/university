# Exportación

El componente permite exportar datos a CSV.

## Activar exportación

```razor
<Table ShowExportButton="true"
       CsvFileName="usuarios.csv"
       ExportMode="TableExportMode.Filtered">
```

## Modos de exportación

```csharp
TableExportMode.Filtered
TableExportMode.CurrentPage
TableExportMode.Selected
TableExportMode.All
```

| Modo | Descripción |
|---|---|
| `Filtered` | Exporta los datos filtrados. |
| `CurrentPage` | Exporta solo la página actual. |
| `Selected` | Exporta las filas seleccionadas. |
| `All` | Exporta todos los datos disponibles. |

## Exportar página actual

```razor
<Table ShowExportButton="true"
       ExportMode="TableExportMode.CurrentPage">
```

## Exportar filtrados

```razor
<Table ShowExportButton="true"
       ExportMode="TableExportMode.Filtered">
```

## Exportar seleccionados

```razor
<Table ShowSelection="true"
       ShowExportButton="true"
       ExportMode="TableExportMode.Selected">
```

## Exportar todo

```razor
<Table ShowExportButton="true"
       ExportMode="TableExportMode.All">
```

Debe usarse con precaución en datasets grandes.

## Excluir columnas

```razor
<TableColumn TItem="User"
             Key="actions"
             Title="Acciones"
             Value="@(_ => "")"
             Exportable="false" />
```

## Valor de exportación personalizado

```razor
<TableColumn TItem="User"
             Key="isActive"
             Title="Activo"
             Value="@(u => u.IsActive)"
             ExportValue='@(u => u.IsActive ? "Activo" : "Inactivo")' />
```

## ExportProvider en modo servidor

En modo servidor, usar `ExportProvider`.

```razor
<Table ServerSide="true"
       ExportProvider="ExportUsersAsync"
       ShowExportButton="true"
       ExportMode="TableExportMode.Filtered">
```

Ejemplo:

```csharp
private async Task<IReadOnlyList<User>> ExportUsersAsync(TableDataRequest request)
{
    var query = DbContext.Users.AsNoTracking();

    query = ApplyUserSearch(query, request);
    query = ApplyUserColumnFilters(query, request);
    query = ApplyUserSearchPanes(query, request);
    query = ApplyUserSorting(query, request);

    return await query.ToListAsync();
}
```

## Seguridad

En datasets grandes o datos sensibles:

- controlar permisos;
- limitar número de filas;
- registrar exportaciones si aplica;
- evitar `TableExportMode.All` sin control;
- aplicar los mismos filtros y permisos que en pantalla.

## JavaScript interop

La descarga se realiza mediante:

```txt
wwwroot/js/tableInterop.js
```

Este archivo crea un Blob y fuerza la descarga del CSV.