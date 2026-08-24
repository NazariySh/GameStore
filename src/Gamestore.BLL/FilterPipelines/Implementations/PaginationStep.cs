using Gamestore.BLL.FilterPipelines.Interfaces;

namespace Gamestore.BLL.FilterPipelines.Implementations;

public class PaginationStep<T> : IPaginationPipelineStep<T>
    where T : class
{
    private readonly int _page;
    private readonly int _pageSize;

    public PaginationStep(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
    }

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        return query
            .Skip((_page - 1) * _pageSize)
            .Take(_pageSize);
    }
}