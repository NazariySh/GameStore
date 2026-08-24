using Gamestore.BLL.FilterPipelines.Interfaces;
using Gamestore.Domain.Entities.Orders;

namespace Gamestore.BLL.FilterPipelines.Implementations.OrderFilters;

public class FilterByDateRange : IFilterPipelineStep<Order>
{
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;

    public FilterByDateRange(DateTime? startDate, DateTime? endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public IQueryable<Order> Apply(IQueryable<Order> query)
    {
        if (_startDate.HasValue)
        {
            query = query.Where(order => order.Date >= _startDate.Value);
        }

        if (_endDate.HasValue)
        {
            query = query.Where(order => order.Date <= _endDate.Value);
        }

        return query;
    }
}