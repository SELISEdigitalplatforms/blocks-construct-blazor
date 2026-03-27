namespace Services.SalesOrders;

public class SalesOrder
{
    public string Id { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public DateOnly OrderDate { get; set; }
    public DateOnly? ShippedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<SalesOrderItem> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

public class SalesOrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
