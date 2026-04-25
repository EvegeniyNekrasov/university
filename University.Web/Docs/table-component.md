# Table Component

## 1. Descripción general

`Table` es un componente genérico para Blazor Web App diseñado para renderizar tablas de datos con funcionalidades avanzadas de búsqueda, filtrado, paginación, ordenación, selección, exportación y visualización.

El componente está inspirado en patrones de uso habituales de DataTables, pero está implementado de forma nativa en Blazor y no depende de jQuery ni de DataTables.js.

El objetivo principal es proporcionar una API declarativa y reutilizable para que los desarrolladores puedan crear tablas consistentes en toda la aplicación sin duplicar lógica de búsqueda, filtros, paginación, exportación o selección.

Ejemplo básico:

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

---

## 2. Ubicación y estructura de archivos

El componente está ubicado en:

```txt
Components/UiComponents/Table/
```

La estructura de archivos es:

```txt
Components/UiComponents/Table/Table.razor
Components/UiComponents/Table/Table.razor.cs
Components/UiComponents/Table/TableColumn.razor
Components/UiComponents/Table/TableModels.cs
Components/UiComponents/Table/Table.razor.css
wwwroot/js/tableInterop.js
```

### 2.1. `Table.razor`

Contiene únicamente el markup del componente.

Incluye:

```txt
Toolbar
Buscador global
SearchPanes
Tabla
Cabecera
Filtros por columna
Filas
Filas expandibles
Acciones por fila
Paginación
Templates
```

No debe contener lógica compleja dentro de `@code`.

### 2.2. `Table.razor.cs`

Contiene la lógica del componente en code-behind.

Incluye:

```txt
Parámetros públicos
Estado interno
Carga server-side
Búsqueda
Filtros
SearchPanes
Ordenación
Paginación
Selección
Expansión
Exportación
Persistencia en localStorage
JS interop
Métodos auxiliares
Modelos internos privados
```

La clase debe ser parcial:

```csharp
namespace TuProyecto.Components.UiComponents.Table;

public partial class Table<TItem> : ComponentBase, IAsyncDisposable
{
}
```

El namespace debe coincidir con el namespace real del proyecto.

### 2.3. `TableColumn.razor`

Define la configuración declarativa de cada columna.

Permite definir:

```txt
Key
Título
Valor
Template
Ordenación
Filtros
SearchPanes
Visibilidad
Exportación
Comportamiento cuando la columna está oculta
```

### 2.4. `TableModels.cs`

Contiene los modelos y enums usados por el componente:

```txt
TableDataRequest
TableDataResult<TItem>
TableSearchPane
TableSearchPaneOption
TableSearchPaneSelection
TableColumnFilter
TableSortDescriptor
TableFilterOption
TableFilterType
TableSortDirection
TableExportMode
```

### 2.5. `Table.razor.css`

Contiene los estilos aislados del componente.

Debe mantenerse en la misma carpeta y con el mismo nombre base que el componente:

```txt
Table.razor
Table.razor.css
```

Blazor aplicará estos estilos únicamente al componente correspondiente.

### 2.6. `tableInterop.js`

Ubicación:

```txt
wwwroot/js/tableInterop.js
```

Se usa para:

```txt
Exportar CSV usando Blob
Leer estado desde localStorage
Guardar estado en localStorage
Eliminar estado si se añade esa funcionalidad
```

La importación desde el code-behind normalmente tiene esta forma:

```csharp
_jsModule = await JS.InvokeAsync<IJSObjectReference>(
    "import",
    "./js/tableInterop.js");
```

Si se mueve el archivo JavaScript, debe actualizarse también la ruta del import.

---

## 3. Configuración inicial

En `_Imports.razor`, añadir el namespace del componente:

```razor
@using Universidad.Web.Components.UiComponents.Table
```

Sustituir `TuProyecto` por el namespace raíz real de la aplicación.

En páginas que usen la tabla de forma interactiva, añadir:

```razor
@rendermode InteractiveServer
```

Ejemplo:

```razor
@page "/usuarios"
@rendermode InteractiveServer
```

---

## 4. Conceptos principales

### 4.1. `TItem`

`TItem` representa el tipo de dato de cada fila.

Ejemplo:

```razor
<Table TItem="User" ...>
```

Si la tabla muestra usuarios, `TItem` será `User`.

### 4.2. `RowKey`

`RowKey` define una clave única para cada fila.

Ejemplo:

```razor
RowKey="@(u => u.Id)"
```

Debe usarse siempre que sea posible.

El componente utiliza `RowKey` para:

```txt
Identificar filas seleccionadas
Identificar filas expandidas
Optimizar renderizado
Mantener estado estable entre renders
Evitar comportamientos inconsistentes
```

Uso recomendado:

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)">
```

Uso no recomendado:

```razor
<Table TItem="User"
       Items="Users">
```

Si no se proporciona `RowKey`, el componente puede recurrir a mecanismos menos estables como `GetHashCode()`, lo cual no es recomendable para selección, expansión o persistencia.

### 4.3. `Key` en columnas

Cada columna debe tener un `Key` estable.

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)" />
```

El `Key` se utiliza para:

```txt
Filtros
Ordenación
SearchPanes
Visibilidad de columnas
Persistencia
Modo servidor
Exportación
```

Recomendado:

```razor
Key="createdAt"
Key="isActive"
Key="department"
```

No recomendado:

```razor
Key="Fecha de alta"
Key="Activo?"
```

El `Key` debe ser un identificador técnico estable, no un texto visible ni traducible.

---

## 5. Uso mínimo en modo cliente

El modo cliente usa `Items`.

En este modo, el componente recibe todos los datos y realiza en memoria:

```txt
Búsqueda
Filtros
SearchPanes
Ordenación
Paginación
Exportación
```

Ejemplo:

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

        <TableColumn TItem="User"
                     Key="role"
                     Title="Rol"
                     Value="@(u => u.Role)" />
    </ChildContent>
</Table>

@code {
    private List<User> Users = new()
    {
        new(1, "Ana García", "ana@app.com", "Admin"),
        new(2, "Luis Pérez", "luis@app.com", "User"),
        new(3, "Marta López", "marta@app.com", "Manager")
    };

    private record User(
        int Id,
        string Name,
        string Email,
        string Role);
}
```

### 5.1. Cuándo usar modo cliente

Usar modo cliente cuando:

```txt
El dataset es pequeño o mediano
Los datos ya están cargados en memoria
No se necesita consultar base de datos por cada filtro
No hay problemas de seguridad al enviar todos los datos al cliente
```

### 5.2. Cuándo evitar modo cliente

Evitar modo cliente cuando:

```txt
Hay más de 10.000 registros
Hay 50.000 o más registros
Los datos son sensibles
La búsqueda debe hacerse en base de datos
Los SearchPanes tienen muchos valores únicos
La tabla debe escalar
```

---

## 6. Uso en modo servidor

El modo servidor se activa con:

```razor
ServerSide="true"
```

En este modo, la tabla no recibe todos los datos. En su lugar, envía un `TableDataRequest` al `DataProvider`, y el backend devuelve solo la página actual.

Ejemplo:

```razor
<Table TItem="User"
       ServerSide="true"
       DataProvider="LoadUsersAsync"
       SearchPaneProvider="LoadUserSearchPanesAsync"
       ExportProvider="ExportUsersAsync"
       RowKey="@(u => u.Id)">
```

### 6.1. Cuándo usar modo servidor

Usar modo servidor cuando:

```txt
El dataset es grande
La información está en base de datos
Hay filtros complejos
La tabla necesita escalar
La exportación debe controlarse en backend
Hay reglas de permisos o visibilidad de datos
```

Para tablas con 50.000 o más registros, el modo servidor debe considerarse obligatorio.

---

## 7. Estructura con contenido avanzado

Cuando se usan fragments como:

```txt
ToolbarActions
EmptyTemplate
RowDetails
RowActions
ChildContent
```

las columnas deben ir dentro de `<ChildContent>`.

Correcto:

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

Incorrecto:

```razor
<Table TItem="User"
       Items="Users">

    <RowDetails Context="u">
        <div>@u.Email</div>
    </RowDetails>

    <TableColumn TItem="User"
                 Key="name"
                 Title="Nombre"
                 Value="@(u => u.Name)" />
</Table>
```

El uso incorrecto puede provocar:

```txt
RZ9996: Unrecognized child content inside component 'Table'
```

---

## 8. API de `Table`

### 8.1. Datos

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Items` | `IEnumerable<TItem>` | Datos usados en modo cliente. |
| `ServerSide` | `bool` | Activa modo servidor. |
| `DataProvider` | `Func<TableDataRequest, Task<TableDataResult<TItem>>>` | Método que carga datos paginados en modo servidor. |
| `SearchPaneProvider` | `Func<TableDataRequest, Task<IReadOnlyList<TableSearchPane>>>` | Método que carga SearchPanes en modo servidor. |
| `ExportProvider` | `Func<TableDataRequest, Task<IReadOnlyList<TItem>>>` | Método usado para exportar datos en modo servidor. |

### 8.2. Presentación

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Title` | `string` | Título de la tabla. |
| `EmptyText` | `string` | Texto mostrado cuando no hay resultados. |
| `EmptyTemplate` | `RenderFragment` | Template personalizado para estado vacío. |
| `ToolbarActions` | `RenderFragment` | Contenido personalizado en la toolbar. |
| `IsLoading` | `bool` | Muestra estado de carga. |
| `LoadingRows` | `int` | Número de filas skeleton durante carga. |
| `StickyHeader` | `bool` | Fija la cabecera de la tabla. |
| `MaxHeight` | `string` | Altura máxima del contenedor de tabla. |
| `ResponsiveCards` | `bool` | Convierte filas en cards en pantallas pequeñas. |

### 8.3. Búsqueda

| Parámetro | Tipo | Descripción |
|---|---|---|
| `EnableGlobalSearch` | `bool` | Muestra u oculta el buscador global. |
| `GlobalSearchPlaceholder` | `string` | Placeholder del buscador global. |
| `SearchDebounceMs` | `int` | Tiempo de espera antes de aplicar la búsqueda. |

Ejemplo:

```razor
<Table EnableGlobalSearch="true"
       GlobalSearchPlaceholder="Buscar usuarios..."
       SearchDebounceMs="400">
```

### 8.4. Filtros por columna

| Parámetro | Tipo | Descripción |
|---|---|---|
| `EnableColumnFilters` | `bool` | Activa o desactiva la fila de filtros por columna. |

Ejemplo:

```razor
<Table EnableColumnFilters="true">
```

### 8.5. SearchPanes

| Parámetro | Tipo | Descripción |
|---|---|---|
| `EnableSearchPanes` | `bool` | Activa SearchPanes. |
| `SearchPanesInitialCollapsed` | `bool` | Muestra los SearchPanes cerrados por defecto. |
| `EnableSearchPaneSearch` | `bool` | Muestra buscador dentro de cada SearchPane. |
| `SearchPanesTitle` | `string` | Título del bloque de SearchPanes. |
| `SearchPanesCascade` | `bool` | Hace que los panes se filtren entre sí. |
| `SearchPanesViewTotal` | `bool` | Muestra contador visible/total. |
| `SearchPanesThreshold` | `double` | Ratio usado para generar panes automáticos. |
| `SearchPanesColumns` | `int` | Número de columnas visuales para los panes. |
| `RecalculateSearchPanesOnSearch` | `bool` | Indica si se recalculan los panes al buscar en modo cliente. |

Ejemplo:

```razor
<Table EnableSearchPanes="true"
       SearchPanesInitialCollapsed="true"
       SearchPanesCascade="true"
       SearchPanesViewTotal="true"
       SearchPanesColumns="3"
       RecalculateSearchPanesOnSearch="false">
```

### 8.6. Ordenación

| Parámetro | Tipo | Descripción |
|---|---|---|
| `EnableMultiSort` | `bool` | Permite ordenar por varias columnas usando Shift + Click. |

Ejemplo:

```razor
<Table EnableMultiSort="true">
```

### 8.7. Paginación

| Parámetro | Tipo | Descripción |
|---|---|---|
| `InitialPageSize` | `int` | Tamaño inicial de página. |
| `PageSizeOptions` | `int[]` | Opciones de tamaño de página. |
| `ShowPageSizeSelector` | `bool` | Muestra el selector de tamaño de página. |

Ejemplo:

```razor
<Table InitialPageSize="25"
       PageSizeOptions="new[] { 10, 25, 50, 100 }"
       ShowPageSizeSelector="true">
```

### 8.8. Selección

| Parámetro | Tipo | Descripción |
|---|---|---|
| `ShowSelection` | `bool` | Muestra checkboxes de selección. |
| `MultipleSelection` | `bool` | Permite seleccionar varias filas. |
| `SelectedItems` | `IEnumerable<TItem>` | Selección controlada externamente. |
| `SelectedItemsChanged` | `EventCallback<IReadOnlyList<TItem>>` | Callback cuando cambia la selección. |

Ejemplo:

```razor
<Table ShowSelection="true"
       MultipleSelection="true"
       SelectedItemsChanged="OnSelectionChanged">
```

### 8.9. Filas clicables

| Parámetro | Tipo | Descripción |
|---|---|---|
| `RowsClickable` | `bool` | Activa click en filas. |
| `IsRowClickable` | `Func<TItem, bool>` | Decide si una fila concreta es clicable. |
| `OnRowClick` | `EventCallback<TItem>` | Callback al hacer click en una fila. |
| `RowCssClass` | `Func<TItem, string>` | Permite añadir clases CSS por fila. |

Ejemplo:

```razor
<Table RowsClickable="true"
       IsRowClickable="@(u => u.IsActive)"
       OnRowClick="OnUserClicked">
```

### 8.10. Filas expandibles

| Parámetro | Tipo | Descripción |
|---|---|---|
| `RowDetails` | `RenderFragment<TItem>` | Contenido expandido de la fila. |
| `IsRowExpandable` | `Func<TItem, bool>` | Decide si una fila puede expandirse. |
| `ExpandOnRowClick` | `bool` | Expande la fila al hacer click. |
| `AllowMultipleExpandedRows` | `bool` | Permite varias filas expandidas a la vez. |
| `ExpandColumnHeader` | `string` | Texto de cabecera para la columna de expansión. |

### 8.11. Exportación

| Parámetro | Tipo | Descripción |
|---|---|---|
| `ShowExportButton` | `bool` | Muestra el botón de exportación CSV. |
| `CsvFileName` | `string` | Nombre del archivo CSV. |
| `ExportMode` | `TableExportMode` | Define qué filas se exportan. |
| `ExportProvider` | `Func<TableDataRequest, Task<IReadOnlyList<TItem>>>` | Método de exportación en modo servidor. |

Modos disponibles:

```csharp
TableExportMode.Filtered
TableExportMode.CurrentPage
TableExportMode.Selected
TableExportMode.All
```

### 8.12. Columnas

| Parámetro | Tipo | Descripción |
|---|---|---|
| `ShowColumnVisibilityButton` | `bool` | Muestra el desplegable para mostrar/ocultar columnas. |

### 8.13. Persistencia

| Parámetro | Tipo | Descripción |
|---|---|---|
| `PersistState` | `bool` | Guarda el estado de la tabla en localStorage. |
| `StateKey` | `string` | Clave única para persistir el estado. |

Ejemplo:

```razor
<Table PersistState="true"
       StateKey="users-table">
```

---

## 9. API de `TableColumn`

### 9.1. Datos básicos

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Key` | `string` | Identificador estable de la columna. |
| `Title` | `string` | Texto visible en la cabecera. |
| `Value` | `Func<TItem, object?>` | Valor base de la columna. |
| `Template` | `RenderFragment<TItem>` | Template visual personalizado. |

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)" />
```

### 9.2. Template personalizado

Ejemplo:

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

### 9.3. Ordenación

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Sortable` | `bool` | Permite ordenar por la columna. |

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             Sortable="false" />
```

### 9.4. Filtros

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Filterable` | `bool` | La columna puede participar en filtros. |
| `ShowColumnFilter` | `bool` | Muestra el input/select de filtro en la cabecera. |
| `FilterType` | `TableFilterType` | Tipo de filtro. |
| `FilterOptions` | `IReadOnlyList<TableFilterOption>` | Opciones para filtros tipo select. |
| `ColumnFilterPlaceholder` | `string` | Placeholder personalizado del filtro. |

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="role"
             Title="Rol"
             Value="@(u => u.Role)"
             FilterType="TableFilterType.Select"
             FilterOptions="RoleOptions"
             ShowColumnFilter="true" />
```

### 9.5. Visibilidad

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Visible` | `bool` | Si es `false`, la columna queda fuera del componente. |
| `ShowByDefault` | `bool` | Si es `false`, la columna empieza oculta pero existe. |
| `ShowInColumnVisibilityMenu` | `bool` | Indica si aparece en el selector de columnas. |

Para ocultar una columna por defecto debe usarse `ShowByDefault="false"`.

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             ShowByDefault="false" />
```

No usar `Visible="false"` para ocultar una columna por defecto. `Visible="false"` debe reservarse para columnas que no deben formar parte de la tabla.

### 9.6. Columnas ocultas pero filtrables

| Parámetro | Tipo | Descripción |
|---|---|---|
| `SearchableWhenHidden` | `bool` | Permite búsqueda global aunque la columna esté oculta. |
| `FilterableWhenHidden` | `bool` | Permite filtros aunque la columna esté oculta. |
| `SearchPaneWhenHidden` | `bool` | Permite SearchPane aunque la columna esté oculta. |

Ejemplo:

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

### 9.7. SearchPanes

| Parámetro | Tipo | Descripción |
|---|---|---|
| `SearchPane` | `bool?` | `true` fuerza pane, `false` lo oculta, `null` automático. |
| `SearchPaneTitle` | `string` | Título personalizado del pane. |
| `SearchPaneThreshold` | `double?` | Threshold específico de columna. |
| `SearchPaneValue` | `Func<TItem, string>` | Valor usado para agrupar opciones. |

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="createdAt"
             Title="Alta"
             Value="@(u => u.CreatedAt)"
             SearchPane="true"
             SearchPaneTitle="Año"
             SearchPaneValue="@(u => u.CreatedAt.Year.ToString())" />
```

### 9.8. Exportación

| Parámetro | Tipo | Descripción |
|---|---|---|
| `Exportable` | `bool` | Incluye o excluye la columna del CSV. |
| `ExportValue` | `Func<TItem, object?>` | Valor alternativo para exportar. |

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="isActive"
             Title="Activo"
             Value="@(u => u.IsActive)"
             ExportValue='@(u => u.IsActive ? "Activo" : "Inactivo")' />
```

---

## 10. Tipos de filtro

El componente soporta los siguientes filtros:

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

### 10.1. `Text`

Filtro de texto parcial.

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)"
             FilterType="TableFilterType.Text" />
```

### 10.2. `Select`

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

### 10.3. `Boolean`

Filtro para valores booleanos.

```razor
<TableColumn TItem="User"
             Key="isActive"
             Title="Activo"
             Value="@(u => u.IsActive)"
             FilterType="TableFilterType.Boolean" />
```

### 10.4. `Date`

Filtro por fecha exacta.

```razor
<TableColumn TItem="User"
             Key="createdAt"
             Title="Alta"
             Value="@(u => u.CreatedAt)"
             FilterType="TableFilterType.Date" />
```

### 10.5. `DateRange`

Filtro por rango de fechas.

```razor
<TableColumn TItem="User"
             Key="createdAt"
             Title="Alta"
             Value="@(u => u.CreatedAt)"
             FilterType="TableFilterType.DateRange" />
```

### 10.6. `Number`

Filtro numérico exacto.

```razor
<TableColumn TItem="Product"
             Key="stock"
             Title="Stock"
             Value="@(p => p.Stock)"
             FilterType="TableFilterType.Number" />
```

### 10.7. `NumberRange`

Filtro por rango numérico.

```razor
<TableColumn TItem="Product"
             Key="price"
             Title="Precio"
             Value="@(p => p.Price)"
             FilterType="TableFilterType.NumberRange" />
```

---

## 11. SearchPanes

SearchPanes son paneles de filtrado rápido basados en valores únicos de una columna.

Ejemplo:

```razor
<Table EnableSearchPanes="true"
       SearchPanesInitialCollapsed="true"
       SearchPanesCascade="true"
       SearchPanesViewTotal="true"
       SearchPanesColumns="3">
```

Columna con SearchPane:

```razor
<TableColumn TItem="User"
             Key="role"
             Title="Rol"
             Value="@(u => u.Role)"
             SearchPane="true" />
```

### 11.1. Comportamiento de selección

Dentro de un mismo pane, varias opciones se combinan como OR.

Ejemplo:

```txt
Rol = Admin OR Manager
```

Entre panes distintos, las selecciones se combinan como AND.

Ejemplo:

```txt
Rol = Admin
AND
Activo = Sí
```

### 11.2. Cascading panes

Con:

```razor
SearchPanesCascade="true"
```

las opciones de cada pane se recalculan según lo seleccionado en otros panes.

### 11.3. View total

Con:

```razor
SearchPanesViewTotal="true"
```

cada opción puede mostrar contador visible y total.

Ejemplo:

```txt
Admin 10 / 32
```

### 11.4. Buscador dentro de panes

Con:

```razor
EnableSearchPaneSearch="true"
```

cada SearchPane muestra un buscador interno para filtrar sus opciones.

### 11.5. Ocultar SearchPanes por defecto

```razor
SearchPanesInitialCollapsed="true"
```

Si `PersistState="true"`, el estado guardado en `localStorage` puede prevalecer sobre este valor inicial.

Para resetear el estado guardado, cambiar el `StateKey`:

```razor
StateKey="users-table-v2"
```

---

## 12. Columnas visibles, ocultas y técnicas

### 12.1. Columna visible normal

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)" />
```

### 12.2. Columna oculta por defecto

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             ShowByDefault="false" />
```

La columna aparece desmarcada en el selector de columnas.

### 12.3. Columna oculta con SearchPane visible

```razor
<TableColumn TItem="User"
             Key="department"
             Title="Departamento"
             Value="@(u => u.Department)"
             ShowByDefault="false"
             SearchPane="true"
             SearchPaneWhenHidden="true" />
```

### 12.4. Columna técnica

Una columna técnica puede no mostrarse en la tabla ni en el selector, pero servir para filtrar o para SearchPanes.

```razor
<TableColumn TItem="User"
             Key="internalStatus"
             Title="Estado interno"
             Value="@(u => u.InternalStatus)"
             ShowByDefault="false"
             ShowInColumnVisibilityMenu="false"
             SearchPane="true"
             SearchPaneWhenHidden="true"
             SearchableWhenHidden="false"
             FilterableWhenHidden="true" />
```

---

## 13. Ordenación

### 13.1. Ordenación simple

Por defecto, una columna es ordenable.

```razor
<TableColumn TItem="User"
             Key="name"
             Title="Nombre"
             Value="@(u => u.Name)" />
```

Para desactivar ordenación:

```razor
<TableColumn TItem="User"
             Key="actions"
             Title="Acciones"
             Value="@(_ => "")"
             Sortable="false" />
```

### 13.2. Multi-sort

Activar:

```razor
<Table EnableMultiSort="true">
```

Uso:

```txt
Click normal      Ordena por una columna.
Shift + Click     Añade otra columna a la ordenación.
```

---

## 14. Paginación

Ejemplo:

```razor
<Table InitialPageSize="25"
       PageSizeOptions="new[] { 10, 25, 50, 100 }"
       ShowPageSizeSelector="true">
```

En modo servidor, el backend debe limitar también el tamaño de página:

```csharp
var page = Math.Max(1, request.Page);
var pageSize = Math.Clamp(request.PageSize, 5, 100);
```

---

## 15. Selección de filas

Activar selección:

```razor
<Table ShowSelection="true"
       MultipleSelection="true"
       SelectedItemsChanged="OnSelectionChanged">
```

Callback:

```csharp
private Task OnSelectionChanged(IReadOnlyList<User> selectedUsers)
{
    Console.WriteLine($"Seleccionados: {selectedUsers.Count}");
    return Task.CompletedTask;
}
```

Selección única:

```razor
MultipleSelection="false"
```

---

## 16. Filas clicables

Activar click en filas:

```razor
<Table RowsClickable="true"
       OnRowClick="OnUserClicked">
```

Callback:

```csharp
private Task OnUserClicked(User user)
{
    Console.WriteLine($"Click en {user.Name}");
    return Task.CompletedTask;
}
```

Condición por fila:

```razor
IsRowClickable="@(u => u.IsActive)"
```

---

## 17. Filas expandibles

Ejemplo:

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)"
       AllowMultipleExpandedRows="true">

    <RowDetails Context="u">
        <div class="user-details">
            <div><strong>ID:</strong> @u.Id</div>
            <div><strong>Email:</strong> @u.Email</div>
            <div><strong>Notas:</strong> @u.Notes</div>
        </div>
    </RowDetails>

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)" />
    </ChildContent>
</Table>
```

Expandir al hacer click en la fila:

```razor
ExpandOnRowClick="true"
```

Permitir solo una fila abierta:

```razor
AllowMultipleExpandedRows="false"
```

Condición por fila:

```razor
IsRowExpandable="@(u => u.IsActive)"
```

---

## 18. Acciones por fila

Ejemplo:

```razor
<RowActions Context="u">
    <button class="btn-mini primary" @onclick="() => EditUser(u)">
        Editar
    </button>

    <button class="btn-mini danger" @onclick="() => DeleteUser(u)">
        Eliminar
    </button>
</RowActions>
```

Los clicks dentro de `RowActions` no deben disparar `OnRowClick`.

---

## 19. Toolbar personalizada

Ejemplo:

```razor
<ToolbarActions>
    <button class="btn-mini primary" @onclick="CreateUser">
        Nuevo usuario
    </button>

    <button class="btn-mini secondary" @onclick="Refresh">
        Refrescar
    </button>
</ToolbarActions>
```

La toolbar personalizada se renderiza junto al buscador global, selector de página, columnas y exportación.

---

## 20. Exportación CSV

Activar exportación:

```razor
<Table ShowExportButton="true"
       CsvFileName="usuarios.csv"
       ExportMode="TableExportMode.Filtered">
```

Modos disponibles:

| Modo | Descripción |
|---|---|
| `Filtered` | Exporta los datos filtrados. |
| `CurrentPage` | Exporta solo la página actual. |
| `Selected` | Exporta las filas seleccionadas. |
| `All` | Exporta todos los datos disponibles. |

### 20.1. Exportación en modo servidor

En modo servidor, usar `ExportProvider`:

```razor
<Table ServerSide="true"
       ExportProvider="ExportUsersAsync"
       ShowExportButton="true">
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

En datasets grandes, la exportación debe controlarse mediante permisos, límites o procesos específicos de backend.

---

## 21. Persistencia de estado

Activar:

```razor
<Table PersistState="true"
       StateKey="users-table">
```

Se guarda en `localStorage`:

```txt
Búsqueda global
Página actual
Tamaño de página
Filtros por columna
SearchPanes
Columnas ocultas
Ordenación
```

Cambiar `StateKey` cuando cambie la configuración de la tabla:

```razor
StateKey="users-table-v2"
```

Esto fuerza a la tabla a usar un nuevo estado inicial.

---

## 22. `TableDataRequest`

En modo servidor, el componente envía al backend un `TableDataRequest`.

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

Campos:

| Campo | Descripción |
|---|---|
| `Page` | Página solicitada. |
| `PageSize` | Número de filas por página. |
| `SearchText` | Texto de búsqueda global. |
| `Sorts` | Ordenaciones activas. |
| `ColumnFilters` | Filtros por columna. |
| `SearchPaneSelections` | Selecciones activas de SearchPanes. |
| `VisibleColumnKeys` | Columnas visibles actualmente. |
| `IsExport` | Indica si la petición se usa para exportación. |

---

## 23. `DataProvider`

Ejemplo de `DataProvider` con EF Core:

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

---

## 24. Búsqueda server-side

Ejemplo:

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

Para producción, revisar sensibilidad a mayúsculas/minúsculas según proveedor de base de datos y collation.

---

## 25. Filtros server-side

Ejemplo:

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

Filtro de fecha:

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

---

## 26. SearchPanes server-side

Ejemplo básico:

```csharp
private async Task<IReadOnlyList<TableSearchPane>> LoadUserSearchPanesAsync(
    TableDataRequest request)
{
    var baseQuery = DbContext.Users.AsNoTracking();

    baseQuery = ApplyUserSearch(baseQuery, request);
    baseQuery = ApplyUserColumnFilters(baseQuery, request);

    var roleOptions = await baseQuery
        .GroupBy(u => u.Role)
        .Select(g => new
        {
            Value = g.Key,
            Count = g.Count()
        })
        .ToListAsync();

    var selectedRoles = request.SearchPaneSelections
        .FirstOrDefault(x => x.ColumnKey == "role")
        ?.Values
        .ToHashSet(StringComparer.CurrentCultureIgnoreCase)
        ?? new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

    var rolePane = new TableSearchPane(
        ColumnKey: "role",
        Title: "Rol",
        Options: roleOptions
            .Select(x => new TableSearchPaneOption(
                Value: x.Value,
                Label: x.Value,
                TotalCount: x.Count,
                VisibleCount: x.Count,
                Selected: selectedRoles.Contains(x.Value)))
            .ToList(),
        SelectedCount: selectedRoles.Count);

    return new List<TableSearchPane>
    {
        rolePane
    };
}
```

---

## 27. SearchPanes server-side con exclusión de la propia columna

Cuando se calculan opciones de un pane, normalmente se aplican los filtros de otros panes, pero no el filtro del propio pane.

Ejemplo conceptual:

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

---

## 28. Ordenación server-side segura

Ejemplo:

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

Método auxiliar:

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

---

## 29. Seguridad en modo servidor

El `TableDataRequest` viene del cliente. Debe tratarse como entrada no confiable.

### 29.1. Reglas obligatorias

```txt
No concatenar SQL manualmente con valores del request.
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

### 29.2. Ejemplo seguro

```csharp
query = filter.ColumnKey switch
{
    "name" => query.Where(u => u.Name.Contains(filter.Value)),
    "email" => query.Where(u => u.Email.Contains(filter.Value)),
    "role" => query.Where(u => u.Role == filter.Value),
    _ => query
};
```

### 29.3. Ejemplos inseguros

No hacer:

```csharp
var sql = $"SELECT * FROM Users WHERE Name LIKE '%{request.SearchText}%'";
```

No hacer:

```csharp
var sql = "ORDER BY " + request.Sorts.First().ColumnKey;
```

No hacer:

```csharp
query = query.OrderBy(request.Sorts.First().ColumnKey);
```

salvo que se use una librería que haga validación estricta y lista blanca explícita.

---

## 30. Rendimiento

### 30.1. Recomendaciones generales

Para datasets pequeños:

```razor
<Table Items="Users"
       RowKey="@(u => u.Id)">
```

Para datasets grandes:

```razor
<Table ServerSide="true"
       DataProvider="LoadUsersAsync"
       SearchDebounceMs="400"
       RowKey="@(u => u.Id)">
```

### 30.2. Configuración recomendada para 50.000 o más filas

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

### 30.3. Buenas prácticas de rendimiento

```txt
Usar ServerSide=true para datasets grandes.
Limitar PageSize en frontend y backend.
Usar índices en columnas buscables.
Evitar SearchPanes sobre columnas con demasiados valores únicos.
No recalcular SearchPanes en cada pulsación.
Usar AsNoTracking en EF Core para lecturas.
Controlar exportaciones grandes.
Evitar pasar listas enormes mediante Items.
```

---

## 31. Ejemplo completo en modo cliente

```razor
@page "/usuarios-cliente"
@rendermode InteractiveServer

<Table TItem="User"
       Items="Users"
       Title="Usuarios"
       RowKey="@(u => u.Id)"
       EnableSearchPanes="true"
       EnableColumnFilters="true"
       EnableMultiSort="true"
       ShowSelection="true"
       ShowExportButton="true"
       CsvFileName="usuarios.csv"
       ResponsiveCards="true">

    <ToolbarActions>
        <button class="btn-mini primary" @onclick="CreateUser">
            Nuevo usuario
        </button>
    </ToolbarActions>

    <RowDetails Context="u">
        <div>
            <strong>Email:</strong> @u.Email
        </div>
    </RowDetails>

    <RowActions Context="u">
        <button @onclick="() => EditUser(u)">
            Editar
        </button>
    </RowActions>

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)"
                     SearchPane="false" />

        <TableColumn TItem="User"
                     Key="email"
                     Title="Email"
                     Value="@(u => u.Email)"
                     ShowByDefault="false" />

        <TableColumn TItem="User"
                     Key="role"
                     Title="Rol"
                     Value="@(u => u.Role)"
                     SearchPane="true"
                     FilterType="TableFilterType.Select"
                     FilterOptions="RoleOptions" />
    </ChildContent>
</Table>

@code {
    private IReadOnlyList<TableFilterOption> RoleOptions = new[]
    {
        new TableFilterOption("Admin", "Admin"),
        new TableFilterOption("Manager", "Manager"),
        new TableFilterOption("User", "User")
    };

    private List<User> Users = new()
    {
        new(1, "Ana García", "ana@app.com", "Admin"),
        new(2, "Luis Pérez", "luis@app.com", "User"),
        new(3, "Marta López", "marta@app.com", "Manager")
    };

    private void CreateUser()
    {
    }

    private void EditUser(User user)
    {
    }

    private record User(
        int Id,
        string Name,
        string Email,
        string Role);
}
```

---

## 32. Ejemplo completo en modo servidor

```razor
@page "/usuarios"
@rendermode InteractiveServer
@using System.Linq.Expressions

<Table TItem="User"
       ServerSide="true"
       DataProvider="LoadUsersAsync"
       SearchPaneProvider="LoadUserSearchPanesAsync"
       ExportProvider="ExportUsersAsync"
       SearchDebounceMs="400"
       RecalculateSearchPanesOnSearch="false"
       EnableSearchPanes="true"
       SearchPanesInitialCollapsed="true"
       EnableMultiSort="true"
       ShowExportButton="true"
       CsvFileName="usuarios.csv"
       ExportMode="TableExportMode.Filtered"
       RowKey="@(u => u.Id)"
       RowsClickable="true"
       OnRowClick="OnUserClicked"
       ShowSelection="true"
       MultipleSelection="true"
       SelectedItemsChanged="OnSelectionChanged"
       ShowColumnVisibilityButton="true"
       StickyHeader="true"
       MaxHeight="650px"
       ResponsiveCards="true">

    <ToolbarActions>
        <button class="btn-mini primary" @onclick="CreateUser">
            Nuevo usuario
        </button>
    </ToolbarActions>

    <EmptyTemplate>
        <div>
            <strong>No hay usuarios.</strong>
            <div>Prueba a limpiar los filtros.</div>
        </div>
    </EmptyTemplate>

    <RowDetails Context="u">
        <div class="user-details">
            <div><strong>ID:</strong> @u.Id</div>
            <div><strong>Email:</strong> @u.Email</div>
            <div><strong>Fecha alta:</strong> @u.CreatedAt.ToString("dd/MM/yyyy")</div>
            <div><strong>Notas:</strong> @u.Notes</div>
        </div>
    </RowDetails>

    <RowActions Context="u">
        <button class="btn-mini primary" @onclick="() => EditUser(u)">
            Editar
        </button>

        <button class="btn-mini danger" @onclick="() => DeleteUser(u)">
            Eliminar
        </button>
    </RowActions>

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)"
                     SearchPane="false"
                     ShowColumnFilter="false" />

        <TableColumn TItem="User"
                     Key="email"
                     Title="Email"
                     Value="@(u => u.Email)"
                     ShowByDefault="false"
                     SearchPane="false" />

        <TableColumn TItem="User"
                     Key="role"
                     Title="Rol"
                     Value="@(u => u.Role)"
                     SearchPane="true"
                     FilterType="TableFilterType.Select"
                     FilterOptions="RoleOptions"
                     ShowColumnFilter="false" />

        <TableColumn TItem="User"
                     Key="createdAt"
                     Title="Alta"
                     Value="@(u => u.CreatedAt)"
                     SearchPane="true"
                     SearchPaneTitle="Año"
                     SearchPaneValue="@(u => u.CreatedAt.Year.ToString())"
                     FilterType="TableFilterType.DateRange"
                     ShowColumnFilter="false">
            <Template Context="u">
                @u.CreatedAt.ToString("dd/MM/yyyy")
            </Template>
        </TableColumn>

        <TableColumn TItem="User"
                     Key="isActive"
                     Title="Activo"
                     Value="@(u => u.IsActive)"
                     SearchPane="true"
                     FilterType="TableFilterType.Boolean"
                     ShowColumnFilter="false"
                     ExportValue='@(u => u.IsActive ? "Activo" : "Inactivo")'>
            <Template Context="u">
                <span class="badge @(u.IsActive ? "ok" : "off")">
                    @(u.IsActive ? "Activo" : "Inactivo")
                </span>
            </Template>
        </TableColumn>
    </ChildContent>
</Table>
```

---

## 33. Problemas comunes

### 33.1. Error `RZ9996`

Causa:

```txt
Se han usado TableColumn directamente dentro de Table junto con otros fragments.
```

Solución:

```razor
<ChildContent>
    <TableColumn ... />
</ChildContent>
```

### 33.2. SearchPanes no aparecen

Comprobar:

```razor
EnableSearchPanes="true"
```

y en la columna:

```razor
SearchPane="true"
```

También comprobar si están ocultos inicialmente:

```razor
SearchPanesInitialCollapsed="true"
```

### 33.3. Una columna oculta no filtra

Comprobar:

```razor
SearchableWhenHidden="true"
FilterableWhenHidden="true"
SearchPaneWhenHidden="true"
```

### 33.4. Una columna aparece marcada aunque `ShowByDefault="false"`

Si `PersistState="true"`, el estado guardado tiene prioridad.

Soluciones:

```razor
StateKey="users-table-v2"
```

o:

```razor
PersistState="false"
```

### 33.5. El buscador va lento

Usar modo servidor:

```razor
ServerSide="true"
DataProvider="LoadUsersAsync"
SearchDebounceMs="400"
```

Evitar:

```razor
Items="ListaCon50000Elementos"
```

### 33.6. Exportación lenta

En datasets grandes:

```txt
Usar ExportProvider
Aplicar límites
Aplicar permisos
Evitar TableExportMode.All sin control
```

---

## 34. Checklist para nuevas tablas

Antes de añadir una tabla nueva al proyecto, comprobar:

```txt
La tabla tiene RowKey.
Todas las columnas tienen Key.
Se usa ServerSide=true si el dataset es grande.
PageSize está limitado en backend.
SearchDebounceMs está configurado.
No se usa SQL dinámico inseguro.
Los filtros server-side usan lista blanca.
La ordenación server-side usa lista blanca.
SearchPanes solo se usan en columnas razonables.
Las columnas ocultas usan ShowByDefault=false, no Visible=false.
StateKey es único si PersistState=true.
ExportMode es adecuado.
ExportProvider existe en modo servidor si hay muchos datos.
RowActions no disparan OnRowClick.
La tabla funciona correctamente en móvil si ResponsiveCards=true.
```

---

## 35. Convenciones recomendadas

### 35.1. Nombres de `Key`

Usar camelCase:

```razor
Key="createdAt"
Key="isActive"
Key="department"
```

Evitar espacios, símbolos y textos traducidos.

### 35.2. Métodos server-side

Usar nombres descriptivos:

```csharp
LoadUsersAsync
LoadUserSearchPanesAsync
ExportUsersAsync
ApplyUserSearch
ApplyUserColumnFilters
ApplyUserSearchPanes
ApplyUserSorting
```

### 35.3. Page size

Frontend:

```razor
PageSizeOptions="new[] { 10, 25, 50, 100 }"
```

Backend:

```csharp
var pageSize = Math.Clamp(request.PageSize, 5, 100);
```

### 35.4. Persistencia

Usar un `StateKey` único por tabla:

```razor
StateKey="users-table"
StateKey="orders-table"
StateKey="audit-log-table"
```

Cuando cambie la estructura de columnas, cambiar la versión:

```razor
StateKey="users-table-v2"
```

---

## 36. Configuración recomendada para producción

Para datasets pequeños:

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)"
       EnableSearchPanes="true"
       EnableColumnFilters="true">
```

Para datasets grandes:

```razor
<Table TItem="User"
       ServerSide="true"
       DataProvider="LoadUsersAsync"
       SearchPaneProvider="LoadUserSearchPanesAsync"
       ExportProvider="ExportUsersAsync"
       RowKey="@(u => u.Id)"
       SearchDebounceMs="400"
       EnableSearchPanes="true"
       SearchPanesInitialCollapsed="true"
       RecalculateSearchPanesOnSearch="false"
       EnableMultiSort="true"
       ShowExportButton="true"
       StickyHeader="true"
       MaxHeight="650px">
```

Para columnas secundarias:

```razor
ShowByDefault="false"
```

Para columnas técnicas:

```razor
ShowByDefault="false"
ShowInColumnVisibilityMenu="false"
SearchPane="true"
SearchPaneWhenHidden="true"
```

---

## 37. Resumen

`Table` debe usarse como componente estándar para tablas de datos en la aplicación.

Recomendaciones principales:

```txt
Usar RowKey siempre.
Usar Key en todas las columnas.
Usar ServerSide=true para datasets grandes.
No pasar listas enormes mediante Items.
Usar SearchDebounceMs en búsquedas.
Controlar PageSize en backend.
No construir SQL dinámico con datos del request.
Usar ShowByDefault=false para columnas ocultas por defecto.
Usar StateKey único cuando PersistState=true.
Usar ExportProvider para exportación server-side.
```