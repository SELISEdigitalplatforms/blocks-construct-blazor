using Microsoft.AspNetCore.Mvc;
using Services.SalesOrders;

namespace Server.Controllers;

/// <summary>
/// Provides REST endpoints for querying sales orders.
/// </summary>
[ApiController]
[Route("api/sales-orders")]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _salesOrderService;

    /// <summary>
    /// Creates a new controller instance.
    /// </summary>
    /// <param name="salesOrderService">Service used to query sales orders.</param>
    public SalesOrdersController(ISalesOrderService salesOrderService)
    {
        _salesOrderService = salesOrderService;
    }

    /// <summary>
    /// Returns all sales orders.
    /// </summary>
    /// <returns>A list of sales orders.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetAll()
    {
        var orders = await _salesOrderService.GetAllAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Returns a single sales order by identifier.
    /// </summary>
    /// <param name="id">The unique order identifier.</param>
    /// <returns>The matching sales order, if found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SalesOrder>> GetById(string id)
    {
        var order = await _salesOrderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Returns sales orders filtered by status.
    /// </summary>
    /// <param name="status">The order status filter.</param>
    /// <returns>A list of sales orders with the requested status.</returns>
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetByStatus(string status)
    {
        var orders = await _salesOrderService.GetByStatusAsync(status);
        return Ok(orders);
    }
}