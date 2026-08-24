using System.Linq.Expressions;
using Gamestore.BLL.FilterPipelines.Interfaces;

namespace Gamestore.BLL.FilterPipelines.Implementations;

public class SortByStep<T, TKey> : ISortPipelineStep<T>
    where T : class
{
    private readonly Expression<Func<T, TKey>> _keySelector;
    private readonly bool _isDescending;

    public SortByStep(Expression<Func<T, TKey>> keySelector, bool isDescending = false)
    {
        _keySelector = keySelector;
        _isDescending = isDescending;
    }

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        return _isDescending
            ? query.OrderByDescending(_keySelector)
            : query.OrderBy(_keySelector);
    }
}