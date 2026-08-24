using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Orders;

namespace Gamestore.BLL.FilterPipelines.Implementations.OrderFilters;

public class OrderSortByDateStep : ISortPipelineStep<Order>
{
    public IQueryable<Order> Apply(IQueryable<Order> query)
    {
        return query.OrderByDescending(o => o.Date);
    }
}