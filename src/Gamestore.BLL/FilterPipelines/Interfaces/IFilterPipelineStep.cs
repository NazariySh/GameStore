namespace Gamestore.BLL.FilterPipelines.Interfaces;

public interface IFilterPipelineStep<T> : IQueryPipelineStep<T>
    where T : class;