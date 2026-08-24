namespace Gamestore.BLL.DTOs.Games;

public class PagedGameList<T>
{
    public PagedGameList()
    {
    }

    public PagedGameList(IReadOnlyList<T> games, int currentPage, int pageSize, int totalCount)
    {
        Games = games;
        CurrentPage = currentPage;
        TotalPages = CalculateTotalPages(totalCount, pageSize);
    }

    public IReadOnlyList<T> Games { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }

    private static int CalculateTotalPages(int totalCount, int pageSize)
    {
        return Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
    }
}