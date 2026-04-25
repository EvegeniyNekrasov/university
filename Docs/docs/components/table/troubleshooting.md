# Troubleshooting

## Error RZ9996

### Síntoma

```txt
RZ9996: Unrecognized child content inside component 'Table'
```

### Causa

Se están usando `TableColumn` directamente dentro de `Table` junto con otros fragments como:

```txt
RowDetails
RowActions
ToolbarActions
EmptyTemplate
```

### Solución

Envolver las columnas dentro de `<ChildContent>`.

Correcto:

```razor
<Table TItem="User" Items="Users">
    <RowDetails Context="u">
        <div>@u.Email</div>
    </RowDetails>

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)" />
    </ChildContent>
</Table>
```

## SearchPanes no aparecen

Comprobar que la tabla tiene:

```razor
EnableSearchPanes="true"
```

Comprobar que la columna tiene:

```razor
SearchPane="true"
```

Comprobar si los panes están cerrados por defecto:

```razor
SearchPanesInitialCollapsed="true"
```

## Una columna oculta no aparece en SearchPanes

Comprobar:

```razor
SearchPane="true"
SearchPaneWhenHidden="true"
```

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="department"
             Title="Departamento"
             Value="@(u => u.Department)"
             ShowByDefault="false"
             SearchPane="true"
             SearchPaneWhenHidden="true" />
```

## Una columna oculta no participa en búsqueda global

Comprobar:

```razor
SearchableWhenHidden="true"
```

## Una columna oculta no participa en filtros

Comprobar:

```razor
FilterableWhenHidden="true"
```

## ShowByDefault no parece funcionar

Si la tabla usa:

```razor
PersistState="true"
```

el estado guardado en `localStorage` puede tener prioridad sobre `ShowByDefault`.

Soluciones:

```razor
StateKey="users-table-v2"
```

o:

```razor
PersistState="false"
```

## El buscador va lento

### Causa habitual

Se están cargando demasiados datos en modo cliente.

Ejemplo no recomendado:

```razor
<Table Items="UsersCon50000Registros">
```

### Solución

Usar modo servidor:

```razor
<Table ServerSide="true"
       DataProvider="LoadUsersAsync"
       SearchDebounceMs="400">
```

## El texto del buscador se comporta raro

Revisar que el componente use una variable separada para el texto del input y otra para el texto aplicado al filtro.

Debe existir una lógica equivalente a:

```txt
_searchInputText  => texto visible en el input
_searchText       => texto aplicado después del debounce
```

## Exportación lenta

En datasets grandes, no usar exportación cliente sobre todos los datos.

Usar:

```razor
ExportProvider="ExportUsersAsync"
```

Y aplicar límites si procede.

## La ordenación server-side no funciona

Comprobar que el backend tiene una lista blanca de columnas:

```csharp
ordered = sort.ColumnKey switch
{
    "name" => ApplyOrder(query, ordered, u => u.Name, asc),
    "email" => ApplyOrder(query, ordered, u => u.Email, asc),
    _ => ordered
};
```

## Los filtros server-side no funcionan

Comprobar que los `Key` de las columnas coinciden con los `ColumnKey` usados en backend.

Ejemplo columna:

```razor
<TableColumn Key="role" ... />
```

Backend:

```csharp
"role" => query.Where(u => u.Role == filter.Value)
```

## Problemas con CSS

Comprobar que existe:

```txt
Components/UiComponents/Table/Table.razor.css
```

El nombre debe coincidir con:

```txt
Table.razor
```

## Problemas con exportación o persistencia

Comprobar que existe:

```txt
wwwroot/js/tableInterop.js
```

Y que el import en code-behind apunta a:

```csharp
"./js/tableInterop.js"
```