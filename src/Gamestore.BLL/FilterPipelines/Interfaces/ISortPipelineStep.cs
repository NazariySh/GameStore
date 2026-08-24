namespace Gamestore.BLL.FilterPipelines.Interfaces;

public interface ISortPipelineStep<T> : IQueryPipelineStep<T>
    where T : class;