namespace Gamestore.BLL.Interfaces;

public interface IOptionsProvider
{
    IReadOnlyList<string> GetPaginationOptions();

    IReadOnlyList<string> GetPublishDateOptions();

    IReadOnlyList<string> GetGameSortOptions();

    IReadOnlyList<string> GetBanDurationOptions();
}