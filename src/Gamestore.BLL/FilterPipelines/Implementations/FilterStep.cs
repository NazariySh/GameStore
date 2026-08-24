using System.Linq.Expressions;
using Gamestore.BLL.FilterPipelines.Interfaces;

namespace Gamestore.BLL.FilterPipelines.Implementations;

public class FilterStep<T> : IFilterPipelineStep<T>
    where T : class
{
    private readonly Expression<Func<T, bool>> _filter;

    public FilterStep(Expression<Func<T, bool>> filter)
    {
        _filter = filter;
    }

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        return query.Where(_filter);
    }
}