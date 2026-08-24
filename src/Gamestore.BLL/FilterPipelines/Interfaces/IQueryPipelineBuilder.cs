namespace Gamestore.BLL.FilterPipelines.Interfaces;

public interface IQueryPipelineBuilder<T>
    where T : class
{
    IQueryPipelineBuilder<T> AddFilterStep(IFilterPipelineStep<T> step);

    IQueryPipelineBuilder<T> AddSortStep(ISortPipelineStep<T> step);

    IQueryPipelineBuilder<T> SetPaginationStep(IPaginationPipelineStep<T> step);

    List<IFilterPipelineStep<T>> GetFilterSteps();

    List<ISortPipelineStep<T>> GetSortSteps();

    IPaginationPipelineStep<T>? GetPaginationStep();

    Func<IQueryable<T>, IQueryable<T>> Build();

    IQueryPipelineBuilder<T> Combine(IQueryPipelineBuilder<T> other);

    void Reset();
}