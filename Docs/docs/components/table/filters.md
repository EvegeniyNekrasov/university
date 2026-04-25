# Filtros

El componente permite filtros por columna. Estos filtros pueden activarse globalmente y configurarse columna por columna.

## Activar filtros por columna

```razor
<Table EnableColumnFilters="true">
```

## Mostrar u ocultar filtro por columna

Mostrar filtro:

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)"
             ShowColumnFilter="true" />
```

Ocultar filtro:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             ShowColumnFilter="false" />
```

## Tipos de filtro

```csharp
public enum TableFilterType
{
    Text,
    Select,
    Boolean,
    Date,
    DateRange,
    Number,
    NumberRange
}
```

## Filtro de texto

Filtro parcial sobre texto.

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)"
             FilterType="TableFilterType.Text" />
```

## Filtro Select

Filtro por valor exacto.

```razor
<TableColumn TItem="User"
             Key="role"
             Title="Rol"
             Value="@(u => u.Role)"
             FilterType="TableFilterType.Select"
             FilterOptions="RoleOptions" />
```

```csharp
private IReadOnlyList<TableFilterOption> RoleOptions = new[]
{
    new TableFilterOption("Admin", "Admin"),
    new TableFilterOption("Manager", "Manager"),
    new TableFilterOption("User", "User")
};
```

## Filtro Boolean

```razor
<TableColumn TItem="User"
             Key="isActive"
             Title="Activo"
             Value="@(u => u.IsActive)"
             FilterType="TableFilterType.Boolean" />
```

## Filtro Date

```razor
<TableColumn TItem="User"
             Key="createdAt"
             Title="Alta"
             Value="@(u => u.CreatedAt)"
             FilterType="TableFilterType.Date" />
```

## Filtro DateRange

```razor
<TableColumn TItem="User"
             Key="createdAt"
             Title="Alta"
             Value="@(u => u.CreatedAt)"
             FilterType="TableFilterType.DateRange" />
```

## Filtro Number

```razor
<TableColumn TItem="Product"
             Key="stock"
             Title="Stock"
             Value="@(p => p.Stock)"
             FilterType="TableFilterType.Number" />
```

## Filtro NumberRange

```razor
<TableColumn TItem="Product"
             Key="price"
             Title="Precio"
             Value="@(p => p.Price)"
             FilterType="TableFilterType.NumberRange" />
```

## Placeholder personalizado

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)"
             ColumnFilterPlaceholder="Buscar nombre..." />
```

## Filtros en columnas ocultas

Una columna oculta puede seguir filtrando si se permite explícitamente.

```razor
<TableColumn TItem="User"
             Key="department"
             Title="Departamento"
             Value="@(u => u.Department)"
             ShowByDefault="false"
             FilterableWhenHidden="true" />
```

## Recomendaciones

- Usar filtros `Select` para columnas con pocos valores posibles.
- Usar `DateRange` para fechas.
- Usar `NumberRange` para importes, precios o cantidades.
- Evitar filtros de texto sobre columnas con bajo valor funcional.
- En modo servidor, validar todos los filtros en backend.