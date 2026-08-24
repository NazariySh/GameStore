using Gamestore.BLL.Enums;
using Gamestore.BLL.Interfaces;

namespace Gamestore.BLL.Utilities;

public class OptionsProvider : IOptionsProvider
{
    public IReadOnlyList<string> GetPaginationOptions()
    {
        return GetAllDisplayNames<PaginationOption>();
    }

    public IReadOnlyList<string> GetPublishDateOptions()
    {
        return GetAllDisplayNames<PublishDateOption>();
    }

    public IReadOnlyList<string> GetGameSortOptions()
    {
        return GetAllDisplayNames<GameSortOption>();
    }

    public IReadOnlyList<string> GetBanDurationOptions()
    {
        return GetAllDisplayNames<BanDuration>();
    }

    private static List<string> GetAllDisplayNames<TOption>()
        where TOption : DisplayOption<TOption>
    {
        return DisplayOption<TOption>.List
            .OrderBy(x => x.Value)
            .Select(x => x.DisplayName)
            .ToList();
    }
}