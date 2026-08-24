namespace Gamestore.BLL.FilterPipelines.Interfaces;

public interface IQueryPipelineStep<T>
    where T : class
{
    IQueryable<T> Apply(IQueryable<T> query);
}