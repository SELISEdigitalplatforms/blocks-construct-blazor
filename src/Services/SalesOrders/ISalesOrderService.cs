namespace Services.SalesOrders;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrder>> GetAllAsync();
    Task<SalesOrder?> GetByIdAsync(string id);
    Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status);
}
