namespace Gamestore.BLL.Enums;

public class PublishDateOption : DisplayOption<PublishDateOption>
{
    public static readonly PublishDateOption LastWeek = new(nameof(LastWeek), 1, "last week", TimeSpan.FromDays(7));
    public static readonly PublishDateOption LastMonth = new(nameof(LastMonth), 2, "last month", TimeSpan.FromDays(30));
    public static readonly PublishDateOption LastYear = new(nameof(LastYear), 3, "last year", TimeSpan.FromDays(365));
    public static readonly PublishDateOption TwoYears = new(nameof(TwoYears), 4, "2 years", TimeSpan.FromDays(2 * 365));
    public static readonly PublishDateOption ThreeYears = new(nameof(ThreeYears), 5, "3 years", TimeSpan.FromDays(3 * 365));

    private PublishDateOption(string name, int value, string displayName, TimeSpan publishPeriod)
        : base(name, value, displayName)
    {
        PublishPeriod = publishPeriod;
    }

    public TimeSpan PublishPeriod { get; }

    public static PublishDateOption? FromDisplayNameOrDefault(string? displayName)
    {
        return !string.IsNullOrEmpty(displayName)
            ? List.FirstOrDefault(x => x.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
            : null;
    }
}