using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace University.Web.Components.UiComponents.Table;

public partial class Table<TItem>
{
    #region Parameters

    [Parameter]
    public IEnumerable<TItem> Items { get; set; } = [];

    [Parameter]
    public bool ServerSide { get; set; }

    [Parameter]
    public Func<TableDataRequest, Task<TableDataResult<TItem>>>? DataProvider { get; set; }

    [Parameter]
    public Func<TableDataRequest, Task<IReadOnlyList<TableSearchPane>>>? SearchPaneProvider { get; set; }

    [Parameter]
    public Func<TableDataRequest, Task<IReadOnlyList<TItem>>>? ExportProvider { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<TItem>? RowDetails { get; set; }

    [Parameter]
    public RenderFragment<TItem>? RowActions { get; set; }

    [Parameter]
    public RenderFragment? ToolbarActions { get; set; }

    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string EmptyText { get; set; } = "No hay nada que mostrar";

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public int LoadingRows { get; set; } = 5;

    [Parameter]
    public bool EnableGlobalSearch { get; set; } = true;

    [Parameter]
    public string GlobalSearchPlaceholder { get; set; } = "Buscar...";

    [Parameter]
    public int SearchDebounceMs { get; set; } = 300;

    [Parameter]
    public bool EnableColumnFilters { get; set; } = true;

    [Parameter]
    public bool EnableMultiSort { get; set; }

    [Parameter]
    public int InitialPageSize { get; set; } = 10;

    [Parameter]
    public int[] PageSizeOptions { get; set; } = [5, 10, 25, 50, 100];

    [Parameter]
    public bool ShowPageSizeSelector { get; set; } = true;

    [Parameter]
    public bool EnableSearchPanes { get; set; }

    [Parameter]
    public bool EnableSearchPaneSearch { get; set; } = true;

    [Parameter]
    public string SearchPanesTitle { get; set; } = "Filtros rápidos";

    [Parameter]
    public bool SearchPanesInitialCollapsed { get; set; }

    [Parameter]
    public bool SearchPanesCascade { get; set; } = true;

    [Parameter]
    public bool SearchPanesViewTotal { get; set; } = true;

    [Parameter]
    public double SearchPanesThreshold { get; set; } = 0.6;

    [Parameter]
    public int SearchPanesColumns { get; set; } = 3;

    [Parameter]
    public bool RowsClickable { get; set; }

    [Parameter]
    public Func<TItem, bool>? IsRowClickable { get; set; }

    [Parameter]
    public EventCallback<TItem> OnRowClick { get; set; }

    [Parameter]
    public Func<TItem, string>? RowCssClass { get; set; }

    [Parameter]
    public Func<TItem, bool>? IsRowExpandable { get; set; }

    [Parameter]
    public bool ExpandOnRowClick { get; set; }

    [Parameter]
    public bool AllowMultipleExpandedRows { get; set; } = true;

    [Parameter]
    public string ExpandColumnHeader { get; set; } = string.Empty;

    [Parameter]
    public Func<TItem, object?>? RowKey { get; set; }

    [Parameter]
    public bool ShowSelection { get; set; }

    [Parameter]
    public bool MultipleSelection { get; set; } = true;

    [Parameter]
    public IEnumerable<TItem>? SelectedItems { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> SelectedItemsChanged { get; set; }

    [Parameter]
    public bool ShowExportButton { get; set; }

    [Parameter]
    public string CsvFileName { get; set; } = "export.csv";

    [Parameter]
    public TableExportMode ExportMode { get; set; } = TableExportMode.Filtered;

    [Parameter]
    public bool ShowColumnVisibilityButton { get; set; } = true;

    [Parameter]
    public bool StickyHeader { get; set; }

    [Parameter]
    public string? MaxHeight { get; set; }

    [Parameter]
    public bool ResponsiveCards { get; set; }

    [Parameter]
    public bool PersistState { get; set; }

    [Parameter]
    public string StateKey { get; set; } = "table-state";

    #endregion

    private readonly List<TableColumn<TItem>> _columns = [];
    private readonly Dictionary<string, TableColumnFilterState> _columnFilters = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _searchPaneSelections = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly Dictionary<string, string> _searchPaneSearchTexts = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly HashSet<string> _expandedRowKeys = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly HashSet<string> _selectedRowKeys = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly HashSet<string> _hiddenColumnKeys = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly List<TableSortDescriptor> _sorts = [];

    private IReadOnlyList<TItem> _serverRows = [];
    private IReadOnlyList<TableSearchPane> _serverSearchPanes = [];
    private int _serverTotalItems;
    private bool _serverLoading;

    private string _searchText = string.Empty;
    private int _pageSize;
    private int _currentPage = 1;
    private bool _searchPanesCollapsed;
    private bool _showColumnMenu;
    private IJSObjectReference? _jsModule;
    private CancellationTokenSource? _debounceCts;

    private IReadOnlyList<TableColumn<TItem>> VisibleColumns =>
        [.. _columns.Where(c => c.Visible && !_hiddenColumnKeys.Contains(c.EffectiveKey))];

    private bool HasExpandableRows => RowDetails is not null;
    private bool HasRowActions => RowActions is not null;
    private bool IsBusy => IsLoading || _serverLoading;
    private int SelectedRowCount => _selectedRowKeys.Count;
    private int SelectedSearchPaneCount => _searchPaneSelections.Values.Sum(x => x.Count);
    private int SearchPaneSearchCount => _searchPaneSearchTexts.Values.Count(x => !string.IsNullOrWhiteSpace(x));

    protected override void OnInitialized()
    {
        _pageSize = InitialPageSize;
        _searchPanesCollapsed = SearchPanesInitialCollapsed;
    }

    protected override void OnParametersSet()
    {
        if (SelectedItems is not null)
        {
            _selectedRowKeys.Clear();

            foreach (var item in SelectedItems)
            {
                _selectedRowKeys.Add(GetRowKey(item));
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await EnsureJsModuleAsync();

        if (PersistState)
        {
            await LoadPersistedStateAsync();
        }

        if (ServerSide && DataProvider is not null)
        {
            await LoadServerDataAsync();
        }

        StateHasChanged();
    }

    internal void AddColumn(TableColumn<TItem> column)
    {
        if (!_columns.Contains(column))
        {
            _columns.Add(column);
            StateHasChanged();
        }
    }

    internal void RemoveColumn(TableColumn<TItem> column)
    {
        if (_columns.Remove(column))
        {
            _columnFilters.Remove(column.EffectiveKey);
            _searchPaneSelections.Remove(column.EffectiveKey);
            _searchPaneSearchTexts.Remove(column.EffectiveKey);
            _hiddenColumnKeys.Remove(column.EffectiveKey);
            _sorts.RemoveAll(s => s.ColumnKey == column.EffectiveKey);
            StateHasChanged();
        }
    }

    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _currentPage = 1;
        await RequestRefreshAsync(debounce: true);
    }

    private async Task OnPageSizeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var size))
        {
            _pageSize = size;
            _currentPage = 1;
            await RequestRefreshAsync();
        }
    }

    private async Task RequestRefreshAsync(bool debounce = false)
    {
        if (debounce && SearchDebounceMs > 0)
        {
            _debounceCts?.Cancel();
            var cts = new CancellationTokenSource();
            _debounceCts = cts;

            try
            {
                await Task.Delay(SearchDebounceMs, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        if (ServerSide && DataProvider is not null)
        {
            await LoadServerDataAsync();
        }

        await PersistStateAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadServerDataAsync()
    {
        if (DataProvider is null)
        {
            return;
        }

        _serverLoading = true;
        StateHasChanged();

        try
        {
            var request = CreateRequest(isExport: false);
            var result = await DataProvider.Invoke(request);

            _serverRows = result.Items;
            _serverTotalItems = result.TotalItems;

            if (EnableSearchPanes && SearchPaneProvider is not null)
            {
                _serverSearchPanes = await SearchPaneProvider.Invoke(request);
            }
        }
        finally
        {
            _serverLoading = false;
        }
    }

    private bool ShouldShowColumnFilterRow(IReadOnlyList<TableColumn<TItem>> visibleColumns) =>
        EnableColumnFilters &&
        visibleColumns.Any(column => column.Filterable && column.ShowColumnFilter);


    private string? GetColumnFilterValue(string columnKey) =>
        _columnFilters.TryGetValue(columnKey, out var value) ? value.Value : null;


    private string? GetColumnFilterFrom(string columnKey) =>
        _columnFilters.TryGetValue(columnKey, out var value) ? value.From : null;


    private string? GetColumnFilterTo(string columnKey) =>
        _columnFilters.TryGetValue(columnKey, out var value) ? value.To : null;


    private async Task SetColumnFilterValueAsync(
        string columnKey,
        TableFilterType type,
        string? value)
    {
        var state = GetOrCreateColumnFilter(columnKey, type);
        state.Value = value;
        RemoveEmptyFilter(columnKey);
        _currentPage = 1;
        await RequestRefreshAsync(debounce: type is TableFilterType.Text or TableFilterType.Number);
    }

    private async Task SetColumnFilterFromAsync(
        string columnKey,
        TableFilterType type,
        string? value)
    {
        var state = GetOrCreateColumnFilter(columnKey, type);
        state.From = value;
        RemoveEmptyFilter(columnKey);
        _currentPage = 1;
        await RequestRefreshAsync(debounce: type == TableFilterType.NumberRange);
    }

    private async Task SetColumnFilterToAsync(
        string columnKey,
        TableFilterType type,
        string? value)
    {
        var state = GetOrCreateColumnFilter(columnKey, type);
        state.To = value;
        RemoveEmptyFilter(columnKey);
        _currentPage = 1;
        await RequestRefreshAsync(debounce: type == TableFilterType.NumberRange);
    }

    private TableColumnFilterState GetOrCreateColumnFilter(
        string columnKey,
        TableFilterType type)
    {
        if (!_columnFilters.TryGetValue(columnKey, out var state))
        {
            state = new TableColumnFilterState { ColumnKey = columnKey };
            _columnFilters[columnKey] = state;
        }

        state.Type = type;
        return state;
    }

    private void RemoveEmptyFilter(string columnKey)
    {
        if (!_columnFilters.TryGetValue(columnKey, out var state))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(state.Value) &&
            string.IsNullOrWhiteSpace(state.From) &&
            string.IsNullOrWhiteSpace(state.To))
        {
            _columnFilters.Remove(columnKey);
        }
    }

    private IReadOnlyList<TableFilterOption> GetFilterOptions(TableColumn<TItem> column)
    {
        if (column.FilterOptions is not null)
        {
            return column.FilterOptions;
        }

        if (ServerSide)
        {
            return [];
        }

        return [.. (Items ?? [])
            .Select(column.TextValue)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => new TableFilterOption(x, x))];
    }

    private string GetSearchPaneSearchText(string columnKey) =>
        _searchPaneSearchTexts.TryGetValue(columnKey, out var value)
            ? value
            : string.Empty;


    private async Task SetSearchPaneSearchTextAsync(string columnKey, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _searchPaneSearchTexts.Remove(columnKey);
        }
        else
        {
            _searchPaneSearchTexts[columnKey] = value;
        }

        await RequestRefreshAsync(debounce: true);
    }

    private async Task ToggleSortAsync(TableColumn<TItem> column, MouseEventArgs args)
    {
        if (!column.Sortable)
        {
            return;
        }

        var existingIndex = _sorts.FindIndex(s => s.ColumnKey == column.EffectiveKey);
        var next = TableSortDirection.Asc;

        if (existingIndex >= 0)
        {
            next = _sorts[existingIndex].Direction switch
            {
                TableSortDirection.Asc => TableSortDirection.Desc,
                TableSortDirection.Desc => TableSortDirection.None,
                _ => TableSortDirection.Asc
            };
        }

        if (!EnableMultiSort || !args.ShiftKey)
        {
            _sorts.Clear();
        }
        else if (existingIndex >= 0)
        {
            _sorts.RemoveAt(existingIndex);
        }

        if (next != TableSortDirection.None)
        {
            _sorts.Add(new TableSortDescriptor(column.EffectiveKey, column.Title, next, _sorts.Count + 1));
        }

        ReorderSortPriorities();
        _currentPage = 1;
        await RequestRefreshAsync();
    }

    private void ReorderSortPriorities()
    {
        for (var i = 0; i < _sorts.Count; i++)
        {
            _sorts[i] = _sorts[i] with { Priority = i + 1 };
        }
    }

    private string GetSortIcon(TableColumn<TItem> column)
    {
        var sort = _sorts.FirstOrDefault(s => s.ColumnKey == column.EffectiveKey);

        if (sort is null)
        {
            return "↕";
        }

        var icon = sort.Direction == TableSortDirection.Asc ? "▲" : "▼";
        return EnableMultiSort && _sorts.Count > 1 ? $"{icon}{sort.Priority}" : icon;
    }

    private string GetAriaSort(TableColumn<TItem> column)
    {
        var sort = _sorts.FirstOrDefault(s => s.ColumnKey == column.EffectiveKey);

        return sort?.Direction switch
        {
            TableSortDirection.Asc => "ascending",
            TableSortDirection.Desc => "descending",
            _ => "none"
        };
    }

    private async Task ToggleSearchPanesAsync()
    {
        _searchPanesCollapsed = !_searchPanesCollapsed;
        await PersistStateAsync();
    }

    private async Task ToggleSearchPaneValueAsync(string columnKey, string value)
    {
        if (!_searchPaneSelections.TryGetValue(columnKey, out var selectedValues))
        {
            selectedValues = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            _searchPaneSelections[columnKey] = selectedValues;
        }

        if (!selectedValues.Add(value))
        {
            selectedValues.Remove(value);
        }

        if (selectedValues.Count == 0)
        {
            _searchPaneSelections.Remove(columnKey);
        }

        _currentPage = 1;
        await RequestRefreshAsync();
    }

    private async Task ClearSearchPanesAsync()
    {
        _searchPaneSelections.Clear();
        _searchPaneSearchTexts.Clear();
        _currentPage = 1;
        await RequestRefreshAsync();
    }

    private async Task ClearAllAsync()
    {
        _searchText = string.Empty;
        _columnFilters.Clear();
        _searchPaneSelections.Clear();
        _searchPaneSearchTexts.Clear();
        _expandedRowKeys.Clear();
        _sorts.Clear();
        _currentPage = 1;
        await RequestRefreshAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        _currentPage = Math.Max(1, page);
        await RequestRefreshAsync();
    }

    private async Task HandleRowClickAsync(TItem item)
    {
        if (ExpandOnRowClick && CanExpandRow(item))
        {
            ToggleRowExpanded(item);
        }

        if (CanInvokeRowClick(item) && OnRowClick.HasDelegate)
        {
            await OnRowClick.InvokeAsync(item);
        }
    }

    private bool CanInvokeRowClick(TItem item)
    {
        if (!RowsClickable && !OnRowClick.HasDelegate)
        {
            return false;
        }

        return IsRowClickable?.Invoke(item) ?? true;
    }

    private bool CanInteractWithRow(TItem item)
    {
        return CanInvokeRowClick(item) || (ExpandOnRowClick && CanExpandRow(item));
    }

    private bool CanExpandRow(TItem item)
    {
        if (!HasExpandableRows)
        {
            return false;
        }

        return IsRowExpandable?.Invoke(item) ?? true;
    }

    private void ToggleRowExpanded(TItem item)
    {
        if (!CanExpandRow(item))
        {
            return;
        }

        var key = GetRowKey(item);

        if (_expandedRowKeys.Contains(key))
        {
            _expandedRowKeys.Remove(key);
            return;
        }

        if (!AllowMultipleExpandedRows)
        {
            _expandedRowKeys.Clear();
        }

        _expandedRowKeys.Add(key);
    }

    private bool IsRowExpanded(TItem item) =>
        _expandedRowKeys.Contains(GetRowKey(item));


    private string GetRowKey(TItem item)
    {
        var key = RowKey?.Invoke(item);

        if (key is not null)
        {
            return Convert.ToString(key, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        return item?.GetHashCode().ToString(CultureInfo.InvariantCulture) ?? "null";
    }

    private string GetRowClass(TItem item)
    {
        var classes = new List<string>();

        if (CanInteractWithRow(item))
        {
            classes.Add("dt-row-clickable");
        }

        if (IsRowExpanded(item))
        {
            classes.Add("dt-row-expanded");
        }

        if (IsRowSelected(item))
        {
            classes.Add("dt-row-selected");
        }

        var custom = RowCssClass?.Invoke(item);

        if (!string.IsNullOrWhiteSpace(custom))
        {
            classes.Add(custom);
        }

        return string.Join(" ", classes);
    }

    private int GetColumnSpan(IReadOnlyList<TableColumn<TItem>> visibleColumns) =>
        visibleColumns.Count
               + (ShowSelection ? 1 : 0)
               + (HasExpandableRows ? 1 : 0)
               + (HasRowActions ? 1 : 0);


    private TableView CreateView(IReadOnlyList<TableColumn<TItem>> visibleColumns)
    {
        if (ServerSide)
        {
            var totalItems = _serverTotalItems;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)_pageSize));
            var currentPage = Math.Clamp(_currentPage, 1, totalPages);
            var start = totalItems == 0 ? 0 : ((currentPage - 1) * _pageSize) + 1;
            var end = Math.Min(currentPage * _pageSize, totalItems);

            return new TableView(_serverRows, totalItems, totalPages, currentPage, start, end);
        }

        var rows = ApplyQuery(visibleColumns).ToList();
        var localTotalItems = rows.Count;
        var localTotalPages = Math.Max(1, (int)Math.Ceiling(localTotalItems / (double)_pageSize));
        var localCurrentPage = Math.Clamp(_currentPage, 1, localTotalPages);

        var pageRows = rows
            .Skip((localCurrentPage - 1) * _pageSize)
            .Take(_pageSize)
            .ToList();

        var localStart = localTotalItems == 0 ? 0 : ((localCurrentPage - 1) * _pageSize) + 1;
        var localEnd = Math.Min(localCurrentPage * _pageSize, localTotalItems);

        return new TableView(pageRows, localTotalItems, localTotalPages, localCurrentPage, localStart, localEnd);
    }

    private IEnumerable<TItem> ApplyQuery(IReadOnlyList<TableColumn<TItem>> visibleColumns)
    {
        IEnumerable<TItem> query = Items ?? [];

        query = ApplyTextFilters(query, visibleColumns);
        query = ApplySearchPaneSelections(query, visibleColumns);
        query = ApplySorting(query, visibleColumns);

        return query;
    }

    private IEnumerable<TItem> ApplySorting(
        IEnumerable<TItem> source,
        IReadOnlyList<TableColumn<TItem>> visibleColumns)
    {
        IOrderedEnumerable<TItem>? ordered = null;

        foreach (var sort in _sorts.OrderBy(s => s.Priority))
        {
            var column = visibleColumns.FirstOrDefault(c => c.EffectiveKey == sort.ColumnKey);

            if (column is null)
            {
                continue;
            }

            ordered = ordered is null
                ? sort.Direction == TableSortDirection.Asc
                    ? source.OrderBy(item => column.SortValue(item), ObjectComparer.Instance)
                    : source.OrderByDescending(item => column.SortValue(item), ObjectComparer.Instance)
                : sort.Direction == TableSortDirection.Asc
                    ? ordered.ThenBy(item => column.SortValue(item), ObjectComparer.Instance)
                    : ordered.ThenByDescending(item => column.SortValue(item), ObjectComparer.Instance);
        }

        return ordered ?? source;
    }

    private IEnumerable<TItem> ApplyTextFilters(
        IEnumerable<TItem> source,
        IReadOnlyList<TableColumn<TItem>> visibleColumns)
    {
        IEnumerable<TItem> query = source;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();

            query = query.Where(item =>
                visibleColumns.Any(column =>
                    column.Filterable &&
                    column.TextValue(item).Contains(search, StringComparison.CurrentCultureIgnoreCase)));
        }

        foreach (var filter in _columnFilters.Values)
        {
            var column = visibleColumns.FirstOrDefault(c => c.EffectiveKey == filter.ColumnKey);

            if (column is null)
            {
                continue;
            }

            query = query.Where(item => MatchesColumnFilter(item, column, filter));
        }

        return query;
    }

    private bool MatchesColumnFilter(
        TItem item,
        TableColumn<TItem> column,
        TableColumnFilterState filter)
    {
        var raw = column.SortValue(item);
        var text = column.TextValue(item);

        return filter.Type switch
        {
            TableFilterType.Select => string.IsNullOrWhiteSpace(filter.Value) ||
                                      string.Equals(text, filter.Value, StringComparison.CurrentCultureIgnoreCase),

            TableFilterType.Boolean => string.IsNullOrWhiteSpace(filter.Value) ||
                                       MatchesBoolean(raw, filter.Value),

            TableFilterType.Date => string.IsNullOrWhiteSpace(filter.Value) ||
                                    MatchesDate(raw, filter.Value),

            TableFilterType.DateRange => MatchesDateRange(raw, filter.From, filter.To),

            TableFilterType.Number => string.IsNullOrWhiteSpace(filter.Value) ||
                                      MatchesNumber(raw, filter.Value),

            TableFilterType.NumberRange => MatchesNumberRange(raw, filter.From, filter.To),

            _ => string.IsNullOrWhiteSpace(filter.Value) ||
                 text.Contains(filter.Value.Trim(), StringComparison.CurrentCultureIgnoreCase)
        };
    }

    private static bool MatchesBoolean(object? raw, string? expected)
    {
        if (!bool.TryParse(expected, out var expectedBool))
        {
            return true;
        }

        if (raw is bool actualBool)
        {
            return actualBool == expectedBool;
        }

        return string.Equals(Convert.ToString(raw, CultureInfo.CurrentCulture), expected, StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool MatchesDate(object? raw, string? expected)
    {
        if (!TryDate(raw, out var actual) ||
            !DateTime.TryParse(expected, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var expectedDate))
        {
            return true;
        }

        return actual.Date == expectedDate.Date;
    }

    private static bool MatchesDateRange(object? raw, string? from, string? to)
    {
        if (!TryDate(raw, out var actual))
        {
            return true;
        }

        if (DateTime.TryParse(from, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var fromDate) &&
            actual.Date < fromDate.Date)
        {
            return false;
        }

        if (DateTime.TryParse(to, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var toDate) &&
            actual.Date > toDate.Date)
        {
            return false;
        }

        return true;
    }

    private static bool MatchesNumber(object? raw, string? expected)
    {
        if (!TryDecimal(raw, out var actual) ||
            !decimal.TryParse(expected, NumberStyles.Any, CultureInfo.CurrentCulture, out var expectedNumber))
        {
            return true;
        }

        return actual == expectedNumber;
    }

    private static bool MatchesNumberRange(object? raw, string? from, string? to)
    {
        if (!TryDecimal(raw, out var actual))
        {
            return true;
        }

        if (decimal.TryParse(from, NumberStyles.Any, CultureInfo.CurrentCulture, out var fromNumber) &&
            actual < fromNumber)
        {
            return false;
        }

        if (decimal.TryParse(to, NumberStyles.Any, CultureInfo.CurrentCulture, out var toNumber) &&
            actual > toNumber)
        {
            return false;
        }

        return true;
    }

    private static bool TryDate(object? raw, out DateTime date)
    {
        if (raw is DateTime dateTime)
        {
            date = dateTime;
            return true;
        }

        return DateTime.TryParse(
            Convert.ToString(raw, CultureInfo.CurrentCulture),
            CultureInfo.CurrentCulture,
            DateTimeStyles.AssumeLocal,
            out date);
    }

    private static bool TryDecimal(object? raw, out decimal number)
    {
        if (raw is decimal d)
        {
            number = d;
            return true;
        }

        if (raw is IConvertible)
        {
            try
            {
                number = Convert.ToDecimal(raw, CultureInfo.CurrentCulture);
                return true;
            }
            catch
            {
                // fallback
            }
        }

        return decimal.TryParse(
            Convert.ToString(raw, CultureInfo.CurrentCulture),
            NumberStyles.Any,
            CultureInfo.CurrentCulture,
            out number);
    }

    private IEnumerable<TItem> ApplySearchPaneSelections(
        IEnumerable<TItem> source,
        IReadOnlyList<TableColumn<TItem>> visibleColumns,
        string? excludeColumnKey = null)
    {
        IEnumerable<TItem> query = source;

        foreach (var selection in _searchPaneSelections)
        {
            if (selection.Key == excludeColumnKey || selection.Value.Count == 0)
            {
                continue;
            }

            var column = visibleColumns.FirstOrDefault(c => c.EffectiveKey == selection.Key);

            if (column is null)
            {
                continue;
            }

            query = query.Where(item => selection.Value.Contains(column.SearchPaneTextValue(item)));
        }

        return query;
    }

    private IReadOnlyList<TableSearchPane> GetSearchPanes(IReadOnlyList<TableColumn<TItem>> visibleColumns)
    {
        if (!EnableSearchPanes)
        {
            return [];
        }

        if (ServerSide)
        {
            return [.. _serverSearchPanes
                .Select(ApplyPaneSearch)
                .Where(pane => pane.Options.Any() || pane.SelectedCount > 0)];
        }

        return CreateClientSearchPaneViews(visibleColumns);
    }

    private TableSearchPane ApplyPaneSearch(TableSearchPane pane)
    {
        var search = GetSearchPaneSearchText(pane.ColumnKey);
        var selected = GetSelectedSearchPaneValues(pane.ColumnKey);

        var options = pane.Options
            .Where(o => string.IsNullOrWhiteSpace(search) ||
                        o.Label.Contains(search.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .Select(o => o with { Selected = selected.Contains(o.Value) })
            .ToList();

        return pane with
        {
            Options = options,
            SelectedCount = selected.Count
        };
    }

    private IReadOnlyList<TableSearchPane> CreateClientSearchPaneViews(IReadOnlyList<TableColumn<TItem>> visibleColumns)
    {
        var baseRows = ApplyTextFilters(Items ?? [], visibleColumns).ToList();

        return [.. visibleColumns
            .Where(column => ShouldShowSearchPane(column, baseRows))
            .Select(column => CreateClientSearchPaneView(column, visibleColumns, baseRows))
            .Where(pane => pane.Options.Any() || pane.SelectedCount > 0)];
    }

    private bool ShouldShowSearchPane(TableColumn<TItem> column, IReadOnlyList<TItem> baseRows)
    {
        if (column.SearchPane == false)
        {
            return false;
        }

        if (column.SearchPane == true)
        {
            return true;
        }

        if (baseRows.Count == 0)
        {
            return false;
        }

        var threshold = column.SearchPaneThreshold ?? SearchPanesThreshold;

        var uniqueCount = baseRows
            .Select(column.SearchPaneTextValue)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Count();

        var ratio = uniqueCount / (double)baseRows.Count;

        return ratio <= threshold;
    }

    private TableSearchPane CreateClientSearchPaneView(
        TableColumn<TItem> column,
        IReadOnlyList<TableColumn<TItem>> visibleColumns,
        IReadOnlyList<TItem> baseRows)
    {
        var totalCounts = CountSearchPaneValues(baseRows, column);

        var visibleRowsForPane = SearchPanesCascade || SearchPanesViewTotal
            ? ApplySearchPaneSelections(baseRows, visibleColumns, excludeColumnKey: column.EffectiveKey).ToList()
            : baseRows;

        var visibleCounts = CountSearchPaneValues(visibleRowsForPane, column);
        var selectedValues = GetSelectedSearchPaneValues(column.EffectiveKey);

        IEnumerable<string> values = totalCounts.Keys;

        if (SearchPanesCascade)
        {
            values = values.Where(value => visibleCounts.ContainsKey(value) || selectedValues.Contains(value));
        }

        var paneSearch = GetSearchPaneSearchText(column.EffectiveKey);

        if (!string.IsNullOrWhiteSpace(paneSearch))
        {
            values = values.Where(value =>
                FormatSearchPaneLabel(value).Contains(paneSearch.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        var options = values
            .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
            .Select(value =>
            {
                var totalCount = totalCounts.TryGetValue(value, out var total) ? total : 0;
                var visibleCount = visibleCounts.TryGetValue(value, out var visible) ? visible : 0;

                return new TableSearchPaneOption(
                    Value: value,
                    Label: FormatSearchPaneLabel(value),
                    TotalCount: totalCount,
                    VisibleCount: visibleCount,
                    Selected: selectedValues.Contains(value));
            })
            .ToList();

        return new TableSearchPane(
            ColumnKey: column.EffectiveKey,
            Title: column.SearchPaneTitle ?? column.Title,
            Options: options,
            SelectedCount: selectedValues.Count);
    }

    private static Dictionary<string, int> CountSearchPaneValues(
            IEnumerable<TItem> rows,
            TableColumn<TItem> column
        ) =>
        rows
            .GroupBy(item => column.SearchPaneTextValue(item), StringComparer.CurrentCultureIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.CurrentCultureIgnoreCase);


    private HashSet<string> GetSelectedSearchPaneValues(string columnKey) =>
        _searchPaneSelections.TryGetValue(columnKey, out var values)
            ? values
            : new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);


    private bool IsSearchPaneOptionSelected(string columnKey, string value) =>
        _searchPaneSelections.TryGetValue(columnKey, out var values) && values.Contains(value);


    private static string FormatSearchPaneLabel(string value) =>
        string.IsNullOrWhiteSpace(value) ? "(Vacío)" : value;


    private bool IsRowSelected(TItem item) => _selectedRowKeys.Contains(GetRowKey(item));

    private async Task ToggleRowSelectionAsync(TItem item)
    {
        var key = GetRowKey(item);

        if (_selectedRowKeys.Contains(key))
        {
            _selectedRowKeys.Remove(key);
        }
        else
        {
            if (!MultipleSelection)
            {
                _selectedRowKeys.Clear();
            }

            _selectedRowKeys.Add(key);
        }

        await NotifySelectionChangedAsync();
    }

    private bool IsAllPageSelected(IReadOnlyList<TItem> rows) => rows.Any() && rows.All(IsRowSelected);

    private async Task SetPageSelectionAsync(IReadOnlyList<TItem> rows, bool selected)
    {
        if (!MultipleSelection)
        {
            _selectedRowKeys.Clear();

            if (selected && rows.FirstOrDefault() is { } first)
            {
                _selectedRowKeys.Add(GetRowKey(first));
            }

            await NotifySelectionChangedAsync();
            return;
        }

        foreach (var row in rows)
        {
            var key = GetRowKey(row);

            if (selected)
            {
                _selectedRowKeys.Add(key);
            }
            else
            {
                _selectedRowKeys.Remove(key);
            }
        }

        await NotifySelectionChangedAsync();
    }

    private async Task ClearSelectionAsync()
    {
        _selectedRowKeys.Clear();
        await NotifySelectionChangedAsync();
    }

    private IReadOnlyList<TItem> GetSelectedItems()
    {
        var source = ServerSide ? _serverRows : Items ?? [];

        return [.. source.Where(item => _selectedRowKeys.Contains(GetRowKey(item)))];
    }

    private async Task NotifySelectionChangedAsync()
    {
        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(GetSelectedItems());
        }
    }

    private void ToggleColumnMenu()
    {
        _showColumnMenu = !_showColumnMenu;
    }

    private bool IsColumnShown(TableColumn<TItem> column) =>
        !_hiddenColumnKeys.Contains(column.EffectiveKey);


    private async Task SetColumnShownAsync(TableColumn<TItem> column, bool shown)
    {
        if (shown)
        {
            _hiddenColumnKeys.Remove(column.EffectiveKey);
        }
        else
        {
            _hiddenColumnKeys.Add(column.EffectiveKey);
        }

        _currentPage = 1;
        await RequestRefreshAsync();
    }

    private static bool IsChecked(ChangeEventArgs e) => e.Value is bool value && value;


    private async Task ExportCsvAsync(
        IReadOnlyList<TableColumn<TItem>> visibleColumns,
        IReadOnlyList<TItem> currentPageRows)
    {
        var rows = await GetExportRowsAsync(visibleColumns, currentPageRows);
        var csv = BuildCsv(visibleColumns, rows);

        await EnsureJsModuleAsync();
        await _jsModule!.InvokeVoidAsync(
            "downloadTextFile",
            CsvFileName,
            csv,
            "text/csv;charset=utf-8"
        );
    }

    private async Task<IReadOnlyList<TItem>> GetExportRowsAsync(
        IReadOnlyList<TableColumn<TItem>> visibleColumns,
        IReadOnlyList<TItem> currentPageRows)
    {
        if (ExportMode == TableExportMode.Selected)
        {
            return GetSelectedItems();
        }

        if (ExportMode == TableExportMode.CurrentPage)
        {
            return currentPageRows;
        }

        if (ServerSide)
        {
            if (ExportProvider is not null)
            {
                return await ExportProvider.Invoke(CreateRequest(isExport: true));
            }

            return _serverRows;
        }

        return ExportMode switch
        {
            TableExportMode.All => (Items ?? Enumerable.Empty<TItem>()).ToList(),
            _ => ApplyQuery(visibleColumns).ToList()
        };
    }

    private string BuildCsv(
        IReadOnlyList<TableColumn<TItem>> visibleColumns,
        IReadOnlyList<TItem> rows)
    {
        var columns = visibleColumns.Where(c => c.Exportable).ToList();
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(",", columns.Select(c => CsvCell(c.Title))));

        foreach (var item in rows)
        {
            sb.AppendLine(string.Join(",", columns.Select(c => CsvCell(c.ExportTextValue(item)))));
        }

        return sb.ToString();
    }

    private static string CsvCell(string? value)
    {
        value ??= string.Empty;
        value = value.Replace("\"", "\"\"");
        return $"\"{value}\"";
    }

    private TableDataRequest CreateRequest(bool isExport)
    {
        var visibleColumnKeys = VisibleColumns.Select(c => c.EffectiveKey).ToList();

        var filters = _columnFilters.Values
            .Select(f => new TableColumnFilter(f.ColumnKey, f.Type, f.Value, f.From, f.To))
            .ToList();

        var paneSelections = _searchPaneSelections
            .Select(x => new TableSearchPaneSelection(x.Key, [.. x.Value]))
            .ToList();

        return new TableDataRequest(
            Page: _currentPage,
            PageSize: _pageSize,
            SearchText: _searchText,
            Sorts: [.. _sorts],
            ColumnFilters: filters,
            SearchPaneSelections: paneSelections,
            VisibleColumnKeys: visibleColumnKeys,
            IsExport: isExport);
    }

    private string? GetTableWrapStyle()
    {
        if (!StickyHeader && string.IsNullOrWhiteSpace(MaxHeight))
        {
            return null;
        }

        var maxHeight = string.IsNullOrWhiteSpace(MaxHeight) ? "600px" : MaxHeight;
        return $"max-height:{maxHeight};";
    }

    private async Task EnsureJsModuleAsync()
    {
        if (_jsModule is null)
        {
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/tableInterop.js");
        }
    }

    private async Task PersistStateAsync()
    {
        if (!PersistState || _jsModule is null)
        {
            return;
        }

        var state = new TablePersistedState
        {
            SearchText = _searchText,
            PageSize = _pageSize,
            CurrentPage = _currentPage,
            SearchPanesCollapsed = _searchPanesCollapsed,
            ColumnFilters = [.. _columnFilters.Values],
            SearchPaneSelections = _searchPaneSelections.ToDictionary(x => x.Key, x => x.Value.ToList(), StringComparer.CurrentCultureIgnoreCase),
            SearchPaneSearchTexts = _searchPaneSearchTexts.ToDictionary(x => x.Key, x => x.Value, StringComparer.CurrentCultureIgnoreCase),
            HiddenColumnKeys = [.. _hiddenColumnKeys],
            Sorts = [.. _sorts]
        };

        var json = JsonSerializer.Serialize(state);
        await _jsModule.InvokeVoidAsync("setLocalState", StateKey, json);
    }

    private async Task LoadPersistedStateAsync()
    {
        if (!PersistState || _jsModule is null)
        {
            return;
        }

        var json = await _jsModule.InvokeAsync<string?>("getLocalState", StateKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            var state = JsonSerializer.Deserialize<TablePersistedState>(json);

            if (state is null)
            {
                return;
            }

            _searchText = state.SearchText ?? string.Empty;
            _pageSize = state.PageSize > 0 ? state.PageSize : InitialPageSize;
            _currentPage = state.CurrentPage > 0 ? state.CurrentPage : 1;
            _searchPanesCollapsed = state.SearchPanesCollapsed;

            _columnFilters.Clear();
            foreach (var filter in state.ColumnFilters ?? new())
            {
                _columnFilters[filter.ColumnKey] = filter;
            }

            _searchPaneSelections.Clear();
            foreach (var selection in state.SearchPaneSelections ?? new())
            {
                _searchPaneSelections[selection.Key] = selection.Value.ToHashSet(StringComparer.CurrentCultureIgnoreCase);
            }

            _searchPaneSearchTexts.Clear();
            foreach (var item in state.SearchPaneSearchTexts ?? new())
            {
                _searchPaneSearchTexts[item.Key] = item.Value;
            }

            _hiddenColumnKeys.Clear();
            foreach (var key in state.HiddenColumnKeys ?? new())
            {
                _hiddenColumnKeys.Add(key);
            }

            _sorts.Clear();
            _sorts.AddRange(state.Sorts ?? new());
        }
        catch
        {
            // Si el estado guardado cambió de formato, se ignora.
        }
    }

    private static void IgnoreClick()
    {
    }

    public async ValueTask DisposeAsync()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch
            {
                // Blazor Server puede cerrar el circuito antes.
            }
        }
    }

    #region Clases

    private sealed record TableView(
        IReadOnlyList<TItem> Rows,
        int TotalItems,
        int TotalPages,
        int CurrentPage,
        int Start,
        int End);

    private sealed class TableColumnFilterState
    {
        public string ColumnKey { get; set; } = string.Empty;
        public TableFilterType Type { get; set; } = TableFilterType.Text;
        public string? Value { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
    }

    private sealed class TablePersistedState
    {
        public string? SearchText { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public bool SearchPanesCollapsed { get; set; }
        public List<TableColumnFilterState>? ColumnFilters { get; set; }
        public Dictionary<string, List<string>>? SearchPaneSelections { get; set; }
        public Dictionary<string, string>? SearchPaneSearchTexts { get; set; }
        public List<string>? HiddenColumnKeys { get; set; }
        public List<TableSortDescriptor>? Sorts { get; set; }
    }

    private sealed class ObjectComparer : IComparer<object?>
    {
        public static readonly ObjectComparer Instance = new();

        public int Compare(object? x, object? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            if (x is IComparable comparable)
            {
                try
                {
                    return comparable.CompareTo(y);
                }
                catch
                {
                    // fallback
                }
            }

            return string.Compare(
                Convert.ToString(x, CultureInfo.CurrentCulture),
                Convert.ToString(y, CultureInfo.CurrentCulture),
                StringComparison.CurrentCultureIgnoreCase);
        }
    }

    #endregion
}
