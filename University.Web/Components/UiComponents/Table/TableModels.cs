
namespace University.Web.Components.UiComponents.Table;

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

public enum TableSortDirection
{
    None,
    Asc,
    Desc
}

public enum TableExportMode
{
    Filtered,
    CurrentPage,
    Selected,
    All
}

public sealed record TableFilterOption(
    string Value,
    string Text
);

public sealed record TableColumnFilter(
    string ColumnKey,
    TableFilterType Type,
    string? Value,
    string? From,
    string? To
);

public sealed record TableSearchPaneSelection(
    string ColumnKey,
    IReadOnlyList<string> Values
);

public sealed record TableSortDescriptor(
    string ColumnKey,
    string Title,
    TableSortDirection Direction,
    int Priority
);

public sealed record TableDataRequest(
    int Page,
    int PageSize,
    string? SearchText,
    IReadOnlyList<TableSortDescriptor> Sorts,
    IReadOnlyList<TableColumnFilter> ColumnFilters,
    IReadOnlyList<TableSearchPaneSelection> SearchPaneSelections,
    IReadOnlyList<string> VisibleColumnKeys,
    bool IsExport = false
);

public sealed record TableDataResult<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalItems
);

public sealed record TableSearchPaneOption(
    string Value,
    string Label,
    int TotalCount,
    int VisibleCount,
    bool Selected = false
);

public sealed record TableSearchPane(
    string ColumnKey,
    string Title,
    IReadOnlyList<TableSearchPaneOption> Options,
    int SelectedCount = 0
);