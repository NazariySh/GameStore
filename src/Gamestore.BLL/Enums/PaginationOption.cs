namespace Gamestore.BLL.Enums;

public class PaginationOption : DisplayOption<PaginationOption>
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 1;

    public static readonly PaginationOption Ten = new(nameof(Ten), 1, "10", 10);
    public static readonly PaginationOption Twenty = new(nameof(Twenty), 2, "20", 20);
    public static readonly PaginationOption Fifty = new(nameof(Fifty), 3, "50", 50);
    public static readonly PaginationOption Hundred = new(nameof(Hundred), 4, "100", 100);
    public static readonly PaginationOption All = new(nameof(All), 5, "all", int.MaxValue);

    private PaginationOption(string name, int value, string displayName, int pageSize)
        : base(name, value, displayName)
    {
        PageSize = pageSize;
    }

    public int PageSize { get; }

    public static PaginationOption FromDisplayName(string? displayName)
    {
        return !string.IsNullOrEmpty(displayName)
            ? List.FirstOrDefault(d => d.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase)) ?? Ten
            : Ten;
    }
}