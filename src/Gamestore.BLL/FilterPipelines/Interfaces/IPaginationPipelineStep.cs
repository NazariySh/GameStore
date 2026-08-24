namespace Gamestore.BLL.FilterPipelines.Interfaces;

public interface IPaginationPipelineStep<T> : IQueryPipelineStep<T>
    where T : class;