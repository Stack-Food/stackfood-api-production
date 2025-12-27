using Microsoft.AspNetCore.Mvc;
using StackFood.Production.Application.UseCases;
using System.Diagnostics.CodeAnalysis;

namespace StackFood.Production.API.Controllers;

[ApiController]
[Route("api/production/queue")]
[ExcludeFromCodeCoverage]
public class ProductionQueueController : ControllerBase
{
    private readonly GetProductionQueueUseCase _getQueueUseCase;
    private readonly ILogger<ProductionQueueController> _logger;

    public ProductionQueueController(
        GetProductionQueueUseCase getQueueUseCase,
        ILogger<ProductionQueueController> logger)
    {
        _getQueueUseCase = getQueueUseCase;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetQueue()
    {
        try
        {
            var queue = await _getQueueUseCase.ExecuteAsync();
            return Ok(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production queue");
            return StatusCode(500, new { error = "Failed to retrieve production queue" });
        }
    }
}
