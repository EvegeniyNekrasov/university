# Columnas

Las columnas se definen mediante el componente `TableColumn`.

## Ejemplo básico

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)" />
```

## Parámetros principales

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Key` | `string` | Identificador estable de la columna. |
| `Title` | `string` | Texto visible en la cabecera. |
| `Value` | `Func<TItem, object?>` | Valor base de la columna. |
| `Template` | `RenderFragment<TItem>` | Template visual personalizado. |
| `Sortable` | `bool` | Permite ordenar por la columna. |
| `Filterable` | `bool` | Permite filtrar por la columna. |
| `ShowColumnFilter` | `bool` | Muestra el filtro en cabecera. |
| `FilterType` | `TableFilterType` | Tipo de filtro. |
| `Visible` | `bool` | Incluye o excluye la columna del componente. |
| `ShowByDefault` | `bool` | La columna aparece visible inicialmente. |
| `ShowInColumnVisibilityMenu` | `bool` | Aparece en el selector de columnas. |
| `SearchPane` | `bool?` | Controla si la columna genera SearchPane. |
| `Exportable` | `bool` | Incluye la columna en exportación. |
| `ExportValue` | `Func<TItem, object?>` | Valor alternativo para exportación. |

## Template personalizado

Usar `Template` cuando el valor necesita renderizado HTML.

```razor
<TableColumn TItem="User"
             Key="isActive"
             Title="Activo"
             Value="@(u => u.IsActive)">
    <Template Context="u">
        <span class="badge @(u.IsActive ? "ok" : "off")">
            @(u.IsActive ? "Activo" : "Inactivo")
        </span>
    </Template>
</TableColumn>
```

## Ordenación

Las columnas son ordenables por defecto.

Para desactivar ordenación:

```razor
<TableColumn TItem="User"
             Key="actions"
             Title="Acciones"
             Value="@(_ => "")"
             Sortable="false" />
```

## Columna visible normal

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)" />
```

## Columna oculta por defecto

Para ocultar una columna inicialmente, usar `ShowByDefault="false"`.

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             ShowByDefault="false" />
```

La columna seguirá existiendo y podrá mostrarse desde el selector de columnas.

## No usar `Visible="false"` para ocultar por defecto

`Visible="false"` debe reservarse para columnas que no deben formar parte del componente.

No recomendado:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             Visible="false" />
```

Recomendado:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             ShowByDefault="false" />
```

## Columna técnica

Una columna técnica puede no mostrarse en tabla ni en selector, pero usarse para filtros o SearchPanes.

```razor
<TableColumn TItem="User"
             Key="internalStatus"
             Title="Estado interno"
             Value="@(u => u.InternalStatus)"
             ShowByDefault="false"
             ShowInColumnVisibilityMenu="false"
             SearchPane="true"
             SearchPaneWhenHidden="true"
             FilterableWhenHidden="true"
             SearchableWhenHidden="false" />
```

## Columnas ocultas pero filtrables

```razor
<TableColumn TItem="User"
             Key="department"
             Title="Departamento"
             Value="@(u => u.Department)"
             ShowByDefault="false"
             SearchPane="true"
             SearchPaneWhenHidden="true"
             SearchableWhenHidden="true"
             FilterableWhenHidden="true" />
```

## Exportación por columna

Excluir columna de exportación:

```razor
<TableColumn TItem="User"
             Key="actions"
             Title="Acciones"
             Value="@(_ => "")"
             Exportable="false" />
```

Valor alternativo para exportación:

```razor
<TableColumn TItem="User"
             Key="isActive"
             Title="Activo"
             Value="@(u => u.IsActive)"
             ExportValue='@(u => u.IsActive ? "Activo" : "Inactivo")' />
```

## Convenciones

Usar `Key` en camelCase:

```razor
Key="createdAt"
Key="isActive"
Key="department"
```

Evitar claves traducidas o con espacios:

```razor
Key="Fecha Alta"
Key="Activo?"
```