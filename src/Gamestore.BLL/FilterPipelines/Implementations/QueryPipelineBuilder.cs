using Gamestore.BLL.FilterPipelines.Interfaces;

namespace Gamestore.BLL.FilterPipelines.Implementations;

public class QueryPipelineBuilder<T> : IQueryPipelineBuilder<T>
    where T : class
{
    private readonly List<IFilterPipelineStep<T>> _filterSteps = [];
    private readonly List<ISortPipelineStep<T>> _sortSteps = [];
    private IPaginationPipelineStep<T>? _paginationStep;

    public IQueryPipelineBuilder<T> AddFilterStep(IFilterPipelineStep<T> step)
    {
        _filterSteps.Add(step);
        return this;
    }

    public IQueryPipelineBuilder<T> AddSortStep(ISortPipelineStep<T> step)
    {
        _sortSteps.Add(step);
        return this;
    }

    public IQueryPipelineBuilder<T> SetPaginationStep(IPaginationPipelineStep<T> step)
    {
        _paginationStep = step;
        return this;
    }

    public List<IFilterPipelineStep<T>> GetFilterSteps()
    {
        return _filterSteps;
    }

    public List<ISortPipelineStep<T>> GetSortSteps()
    {
        return _sortSteps;
    }

    public IPaginationPipelineStep<T>? GetPaginationStep()
    {
        return _paginationStep;
    }

    public Func<IQueryable<T>, IQueryable<T>> Build()
    {
        return query =>
        {
            query = ApplySteps(query, _filterSteps);
            query = ApplySteps(query, _sortSteps);

            if (_paginationStep is not null)
            {
                query = _paginationStep.Apply(query);
            }

            return query;
        };
    }

    public IQueryPipelineBuilder<T> Combine(IQueryPipelineBuilder<T> other)
    {
        var newPipeline = new QueryPipelineBuilder<T>();
        var filterSteps = _filterSteps.Concat(other.GetFilterSteps()).ToList();
        var sortSteps = _sortSteps.Concat(other.GetSortSteps()).ToList();
        var paginationStep = other.GetPaginationStep() ?? _paginationStep;

        foreach (var step in filterSteps)
        {
            newPipeline.AddFilterStep(step);
        }

        foreach (var step in sortSteps)
        {
            newPipeline.AddSortStep(step);
        }

        if (paginationStep is not null)
        {
            newPipeline.SetPaginationStep(paginationStep);
        }

        return newPipeline;
    }

    public void Reset()
    {
        _filterSteps.Clear();
        _sortSteps.Clear();
        _paginationStep = null;
    }

    private static IQueryable<T> ApplySteps(IQueryable<T> query, IEnumerable<IQueryPipelineStep<T>> steps)
    {
        foreach (var step in steps)
        {
            query = step.Apply(query);
        }

        return query;
    }
}