using Microsoft.AspNetCore.Mvc;
using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.UseCases;

namespace StackFood.Production.API.Controllers;

[ApiController]
[Route("api/production/orders")]
public class ProductionOrderController : ControllerBase
{
    private readonly CreateProductionOrderUseCase _createUseCase;
    private readonly GetProductionOrderUseCase _getUseCase;
    private readonly UpdateProductionStatusUseCase _updateStatusUseCase;
    private readonly ILogger<ProductionOrderController> _logger;

    public ProductionOrderController(
        CreateProductionOrderUseCase createUseCase,
        GetProductionOrderUseCase getUseCase,
        UpdateProductionStatusUseCase updateStatusUseCase,
        ILogger<ProductionOrderController> logger)
    {
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _updateStatusUseCase = updateStatusUseCase;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderRequest request)
    {
        try
        {
            var result = await _createUseCase.ExecuteAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating production order");
            return StatusCode(500, new { error = "Failed to create production order" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _getUseCase.ExecuteByIdAsync(id);
            if (result == null)
                return NotFound(new { error = $"Production order {id} not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production order {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve production order" });
        }
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId)
    {
        try
        {
            var result = await _getUseCase.ExecuteByOrderIdAsync(orderId);
            if (result == null)
                return NotFound(new { error = $"Production order for order {orderId} not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production order by OrderId {OrderId}", orderId);
            return StatusCode(500, new { error = "Failed to retrieve production order" });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var result = await _updateStatusUseCase.ExecuteAsync(id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating production order {Id} status to {Status}", id, request.Status);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
