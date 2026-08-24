namespace Gamestore.BLL.Enums;

public class BanDuration : DisplayOption<BanDuration>
{
    public static readonly BanDuration OneHour = new(nameof(OneHour), 1, "1 hour", TimeSpan.FromHours(1));
    public static readonly BanDuration OneDay = new(nameof(OneDay), 2, "1 day", TimeSpan.FromDays(1));
    public static readonly BanDuration OneWeek = new(nameof(OneWeek), 3, "1 week", TimeSpan.FromDays(7));
    public static readonly BanDuration OneMonth = new(nameof(OneMonth), 4, "1 month", TimeSpan.FromDays(30));
    public static readonly BanDuration Permanent = new(nameof(Permanent), 5, "permanent", TimeSpan.MaxValue);

    private BanDuration(string name, int value, string displayName, TimeSpan banPeriod)
        : base(name, value, displayName)
    {
        BanPeriod = banPeriod;
    }

    public TimeSpan BanPeriod { get; }

    public static BanDuration FromDisplayName(string displayName)
    {
        return List.FirstOrDefault(d => d.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException($"Invalid ban duration: '{displayName}'");
    }
}