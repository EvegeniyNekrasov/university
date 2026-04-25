# Rendimiento

El rendimiento del componente depende principalmente del tamaño del dataset y del modo de carga usado.

## Modo cliente

En modo cliente:

```razor
<Table Items="Users">
```

El componente procesa en memoria:

```txt
Búsqueda
Filtros
SearchPanes
Ordenación
Paginación
Exportación
```

Esto es correcto para datasets pequeños o medianos.

## Modo servidor

En modo servidor:

```razor
<Table ServerSide="true"
       DataProvider="LoadUsersAsync">
```

El backend procesa:

```txt
Búsqueda
Filtros
Ordenación
Paginación
SearchPanes
Exportación
```

La tabla solo recibe la página actual.

## Recomendación por tamaño

| Tamaño aproximado | Recomendación |
|---:|---|
| 0 - 1.000 filas | Modo cliente. |
| 1.000 - 10.000 filas | Cliente posible, evaluar caso. |
| 10.000+ filas | Modo servidor recomendado. |
| 50.000+ filas | Modo servidor obligatorio. |

## Configuración recomendada para 50.000+ filas

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

## SearchDebounceMs

La búsqueda global debe usar debounce para evitar ejecutar consultas en cada tecla.

```razor
SearchDebounceMs="400"
```

Valores recomendados:

```txt
250 ms para datasets pequeños
400 ms para datasets grandes
500 ms si la consulta backend es costosa
```

## SearchPanes

SearchPanes pueden ser costosos si se calculan sobre muchas filas o columnas con muchos valores únicos.

Recomendaciones:

- usar SearchPanes solo en columnas con pocos valores únicos;
- evitar SearchPanes en nombres, emails, descripciones o identificadores;
- en modo servidor, calcular SearchPanes en backend;
- usar `SearchPanesInitialCollapsed="true"`;
- usar `RecalculateSearchPanesOnSearch="false"` en datasets grandes.

## EF Core

Para consultas de solo lectura:

```csharp
var query = DbContext.Users.AsNoTracking();
```

## PageSize

Limitar en frontend:

```razor
PageSizeOptions="new[] { 10, 25, 50, 100 }"
```

Limitar en backend:

```csharp
var pageSize = Math.Clamp(request.PageSize, 5, 100);
```

## Índices

Crear índices en columnas usadas para:

- búsqueda;
- filtros frecuentes;
- ordenación;
- joins;
- permisos multi-tenant.

Ejemplos habituales:

```txt
Name
Email
Role
CreatedAt
TenantId
IsActive
```

## Exportación

Para exportaciones grandes:

- usar `ExportProvider`;
- aplicar filtros en backend;
- limitar filas;
- considerar exportación asíncrona;
- evitar `TableExportMode.All` sin control.

## Buenas prácticas

```txt
Usar ServerSide=true para datasets grandes.
Usar RowKey siempre.
Usar Key en todas las columnas.
No pasar listas enormes mediante Items.
Evitar recalcular SearchPanes innecesariamente.
Usar AsNoTracking.
Limitar PageSize.
Controlar exportaciones.
```