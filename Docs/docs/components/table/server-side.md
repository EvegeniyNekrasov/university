# Modo servidor

El modo servidor permite trabajar con datasets grandes sin enviar todos los datos al componente.

La tabla envía un `TableDataRequest` al backend y recibe solo la página actual.

## Activación

```razor
<Table TItem="User"
       ServerSide="true"
       DataProvider="LoadUsersAsync"
       RowKey="@(u => u.Id)">
```

## Configuración recomendada

```razor
<Table TItem="User"
       ServerSide="true"
       DataProvider="LoadUsersAsync"
       SearchPaneProvider="LoadUserSearchPanesAsync"
       ExportProvider="ExportUsersAsync"
       SearchDebounceMs="400"
       EnableSearchPanes="true"
       SearchPanesInitialCollapsed="true"
       RecalculateSearchPanesOnSearch="false"
       EnableMultiSort="true"
       RowKey="@(u => u.Id)">
```

## TableDataRequest

```csharp
public sealed record TableDataRequest(
    int Page,
    int PageSize,
    string? SearchText,
    IReadOnlyList<TableSortDescriptor> Sorts,
    IReadOnlyList<TableColumnFilter> ColumnFilters,
    IReadOnlyList<TableSearchPaneSelection> SearchPaneSelections,
    IReadOnlyList<string> VisibleColumnKeys,
    bool IsExport = false);
```

## TableDataResult

```csharp
public sealed record TableDataResult<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalItems);
```

## DataProvider

Ejemplo con EF Core:

```csharp
private async Task<TableDataResult<User>> LoadUsersAsync(TableDataRequest request)
{
    var page = Math.Max(1, request.Page);
    var pageSize = Math.Clamp(request.PageSize, 5, 100);

    var query = DbContext.Users.AsNoTracking();

    query = ApplyUserSearch(query, request);
    query = ApplyUserColumnFilters(query, request);
    query = ApplyUserSearchPanes(query, request);
    query = ApplyUserSorting(query, request);

    var total = await query.CountAsync();

    var rows = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new TableDataResult<User>(rows, total);
}
```

## Búsqueda global

```csharp
private static IQueryable<User> ApplyUserSearch(
    IQueryable<User> query,
    TableDataRequest request)
{
    if (string.IsNullOrWhiteSpace(request.SearchText))
    {
        return query;
    }

    var search = request.SearchText.Trim();

    return query.Where(u =>
        u.Name.Contains(search) ||
        u.Email.Contains(search) ||
        u.Role.Contains(search));
}
```

## Filtros por columna

```csharp
private static IQueryable<User> ApplyUserColumnFilters(
    IQueryable<User> query,
    TableDataRequest request)
{
    foreach (var filter in request.ColumnFilters)
    {
        query = filter.ColumnKey switch
        {
            "name" when !string.IsNullOrWhiteSpace(filter.Value) =>
                query.Where(u => u.Name.Contains(filter.Value)),

            "email" when !string.IsNullOrWhiteSpace(filter.Value) =>
                query.Where(u => u.Email.Contains(filter.Value)),

            "role" when !string.IsNullOrWhiteSpace(filter.Value) =>
                query.Where(u => u.Role == filter.Value),

            "isActive" when bool.TryParse(filter.Value, out var active) =>
                query.Where(u => u.IsActive == active),

            "createdAt" =>
                ApplyDateRangeFilter(query, filter.From, filter.To),

            _ => query
        };
    }

    return query;
}
```

## Filtro de rango de fechas

```csharp
private static IQueryable<User> ApplyDateRangeFilter(
    IQueryable<User> query,
    string? from,
    string? to)
{
    if (DateTime.TryParse(from, out var fromDate))
    {
        query = query.Where(u => u.CreatedAt.Date >= fromDate.Date);
    }

    if (DateTime.TryParse(to, out var toDate))
    {
        query = query.Where(u => u.CreatedAt.Date <= toDate.Date);
    }

    return query;
}
```

## SearchPanes

```csharp
private static IQueryable<User> ApplyUserSearchPanes(
    IQueryable<User> query,
    TableDataRequest request,
    string? excludeColumnKey = null)
{
    foreach (var pane in request.SearchPaneSelections)
    {
        if (pane.ColumnKey == excludeColumnKey || !pane.Values.Any())
        {
            continue;
        }

        query = pane.ColumnKey switch
        {
            "role" =>
                query.Where(u => pane.Values.Contains(u.Role)),

            "createdAt" =>
                query.Where(u => pane.Values.Contains(u.CreatedAt.Year.ToString())),

            "isActive" =>
                query.Where(u => pane.Values.Contains(u.IsActive ? "Activo" : "Inactivo")),

            _ => query
        };
    }

    return query;
}
```

## Ordenación

```csharp
private static IQueryable<User> ApplyUserSorting(
    IQueryable<User> query,
    TableDataRequest request)
{
    var sorts = request.Sorts
        .OrderBy(s => s.Priority)
        .ToList();

    if (!sorts.Any())
    {
        return query.OrderBy(u => u.Id);
    }

    IOrderedQueryable<User>? ordered = null;

    foreach (var sort in sorts)
    {
        var asc = sort.Direction == TableSortDirection.Asc;

        ordered = sort.ColumnKey switch
        {
            "name" => ApplyOrder(query, ordered, u => u.Name, asc),
            "email" => ApplyOrder(query, ordered, u => u.Email, asc),
            "role" => ApplyOrder(query, ordered, u => u.Role, asc),
            "createdAt" => ApplyOrder(query, ordered, u => u.CreatedAt, asc),
            "isActive" => ApplyOrder(query, ordered, u => u.IsActive, asc),
            _ => ordered
        };
    }

    return ordered ?? query.OrderBy(u => u.Id);
}
```

```csharp
private static IOrderedQueryable<User> ApplyOrder<TKey>(
    IQueryable<User> query,
    IOrderedQueryable<User>? ordered,
    Expression<Func<User, TKey>> selector,
    bool asc)
{
    if (ordered is null)
    {
        return asc
            ? query.OrderBy(selector)
            : query.OrderByDescending(selector);
    }

    return asc
        ? ordered.ThenBy(selector)
        : ordered.ThenByDescending(selector);
}
```

Using necesario:

```csharp
using System.Linq.Expressions;
```

## Recomendaciones

- Usar `AsNoTracking()` para listados de solo lectura.
- Limitar `PageSize`.
- Usar lista blanca para filtros.
- Usar lista blanca para ordenación.
- No concatenar SQL.
- Aplicar permisos dentro de la query.
- Usar índices en columnas buscables.