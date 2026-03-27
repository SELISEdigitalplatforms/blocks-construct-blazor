using Services.SalesOrders;

namespace Test.Services;

public class SalesOrderServiceTests
{
    [Fact]
    public void SalesOrder_LineTotal_IsCalculatedCorrectly()
    {
        var item = new SalesOrderItem
        {
            Quantity = 3,
            UnitPrice = 10.00m
        };

        Assert.Equal(30.00m, item.LineTotal);
    }
}
