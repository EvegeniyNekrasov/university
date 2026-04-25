# Ejemplos

## Ejemplo básico en modo cliente

```razor
@page "/usuarios-cliente"
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

    private record User(int Id, string Name, string Email);
}
```

## Ejemplo con SearchPanes

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)"
       EnableSearchPanes="true"
       SearchPanesInitialCollapsed="true"
       SearchPanesCascade="true"
       SearchPanesViewTotal="true">

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)"
                     SearchPane="false" />

        <TableColumn TItem="User"
                     Key="role"
                     Title="Rol"
                     Value="@(u => u.Role)"
                     SearchPane="true" />

        <TableColumn TItem="User"
                     Key="isActive"
                     Title="Activo"
                     Value="@(u => u.IsActive)"
                     SearchPane="true" />
    </ChildContent>
</Table>
```

## Ejemplo con columna oculta pero filtrable

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

## Ejemplo con fila expandible

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)"
       AllowMultipleExpandedRows="true">

    <RowDetails Context="u">
        <div>
            <strong>Email:</strong> @u.Email
        </div>
        <div>
            <strong>Notas:</strong> @u.Notes
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

## Ejemplo con acciones por fila

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)">

    <RowActions Context="u">
        <button @onclick="() => EditUser(u)">
            Editar
        </button>

        <button @onclick="() => DeleteUser(u)">
            Eliminar
        </button>
    </RowActions>

    <ChildContent>
        <TableColumn TItem="User"
                     Key="name"
                     Title="Nombre"
                     Value="@(u => u.Name)" />
    </ChildContent>
</Table>
```

## Ejemplo con exportación

```razor
<Table TItem="User"
       Items="Users"
       RowKey="@(u => u.Id)"
       ShowExportButton="true"
       CsvFileName="usuarios.csv"
       ExportMode="TableExportMode.Filtered">
```

## Ejemplo en modo servidor

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
       EnableMultiSort="true">
```

```csharp
private async Task<TableDataResult<User>> LoadUsersAsync(TableDataRequest request)
{
    var page = Math.Max(1, request.Page);
    var pageSize = Math.Clamp(request.PageSize, 5, 100);

    var query = DbContext.Users.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(request.SearchText))
    {
        var search = request.SearchText.Trim();

        query = query.Where(u =>
            u.Name.Contains(search) ||
            u.Email.Contains(search));
    }

    var total = await query.CountAsync();

    var rows = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new TableDataResult<User>(rows, total);
}
```