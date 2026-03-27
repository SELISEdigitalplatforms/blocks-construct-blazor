using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.SalesOrders;

public class SalesOrderService : ISalesOrderService
{
    private readonly string _dataFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SalesOrderService(string webRootPath)
    {
        _dataFilePath = Path.Combine(webRootPath, "data", "sales-orders.json");
    }

    public async Task<IEnumerable<SalesOrder>> GetAllAsync()
    {
        return await ReadOrdersAsync();
    }

    public async Task<SalesOrder?> GetByIdAsync(string id)
    {
        var orders = await ReadOrdersAsync();
        return orders.FirstOrDefault(o => o.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status)
    {
        var orders = await ReadOrdersAsync();
        return orders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<List<SalesOrder>> ReadOrdersAsync()
    {
        await using var stream = File.OpenRead(_dataFilePath);
        return await JsonSerializer.DeserializeAsync<List<SalesOrder>>(stream, JsonOptions)
               ?? [];
    }
}
