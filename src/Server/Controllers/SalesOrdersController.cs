using Microsoft.AspNetCore.Mvc;
using Services.SalesOrders;

namespace Server.Controllers;

[ApiController]
[Route("api/sales-orders")]
public class SalesOrdersController(ISalesOrderService salesOrderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetAll()
    {
        var orders = await salesOrderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SalesOrder>> GetById(string id)
    {
        var order = await salesOrderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetByStatus(string status)
    {
        var orders = await salesOrderService.GetByStatusAsync(status);
        return Ok(orders);
    }
}
