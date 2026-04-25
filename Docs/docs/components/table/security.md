# Seguridad

El componente `Table` puede trabajar en modo cliente y modo servidor.

En modo servidor, el objeto `TableDataRequest` viene del cliente y debe tratarse como entrada no confiable.

## Reglas obligatorias

Aplicar siempre estas reglas en backend:

```txt
No concatenar SQL con valores del request.
Usar LINQ/EF Core o SQL parametrizado.
Usar lista blanca para columnas filtrables.
Usar lista blanca para columnas ordenables.
Ignorar ColumnKey desconocidos.
Limitar PageSize.
Aplicar autorización en la query.
Usar AsNoTracking en listados de solo lectura.
Controlar exportaciones masivas.
No confiar en VisibleColumnKeys para seguridad.
```

## No concatenar SQL

No hacer:

```csharp
var sql = $"SELECT * FROM Users WHERE Name LIKE '%{request.SearchText}%'";
```

No hacer:

```csharp
var sql = "ORDER BY " + request.Sorts.First().ColumnKey;
```

## Usar lista blanca para filtros

Correcto:

```csharp
query = filter.ColumnKey switch
{
    "name" when !string.IsNullOrWhiteSpace(filter.Value) =>
        query.Where(u => u.Name.Contains(filter.Value)),

    "email" when !string.IsNullOrWhiteSpace(filter.Value) =>
        query.Where(u => u.Email.Contains(filter.Value)),

    "role" when !string.IsNullOrWhiteSpace(filter.Value) =>
        query.Where(u => u.Role == filter.Value),

    _ => query
};
```

Si el cliente envía una `ColumnKey` desconocida, debe ignorarse.

## Usar lista blanca para ordenación

Correcto:

```csharp
ordered = sort.ColumnKey switch
{
    "name" => ApplyOrder(query, ordered, u => u.Name, asc),
    "email" => ApplyOrder(query, ordered, u => u.Email, asc),
    "role" => ApplyOrder(query, ordered, u => u.Role, asc),
    "createdAt" => ApplyOrder(query, ordered, u => u.CreatedAt, asc),
    _ => ordered
};
```

Incorrecto:

```csharp
query = query.OrderBy(request.Sorts.First().ColumnKey);
```

## Limitar PageSize

```csharp
var page = Math.Max(1, request.Page);
var pageSize = Math.Clamp(request.PageSize, 5, 100);
```

No permitir que el cliente solicite cantidades arbitrarias de filas.

## Aplicar autorización

La autorización debe aplicarse en la query.

Ejemplo:

```csharp
query = query.Where(u => u.TenantId == currentTenantId);
```

No confiar en que la tabla o el frontend oculten datos.

## Exportación segura

En exportaciones:

- aplicar los mismos permisos que en pantalla;
- aplicar filtros activos;
- limitar filas si corresponde;
- evitar exportaciones masivas sin control;
- registrar auditoría si el dominio lo requiere.

## VisibleColumnKeys no es seguridad

`VisibleColumnKeys` indica qué columnas ve el usuario en la tabla, pero no debe usarse como mecanismo de autorización.

El backend debe decidir qué datos puede ver el usuario.