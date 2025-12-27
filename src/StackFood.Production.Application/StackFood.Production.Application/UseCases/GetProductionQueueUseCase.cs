using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases.Mappers;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Application.UseCases;

public class GetProductionQueueUseCase(IProductionRepository repository)
{
    private readonly IProductionRepository _repository = repository;

    public async Task<ProductionQueueDTO> ExecuteAsync()
    {
        var allOrders = await _repository.GetQueueAsync();

        var inQueue = allOrders
            .Where(o => o.Status == ProductionStatus.Received)
            .OrderBy(o => o.Priority)
            .ThenBy(o => o.CreatedAt)
            .ToList();

        var inProgress = allOrders
            .Where(o => o.Status == ProductionStatus.InProgress)
            .OrderBy(o => o.StartedAt)
            .ToList();

        var ready = allOrders
            .Where(o => o.Status == ProductionStatus.Ready)
            .OrderByDescending(o => o.ReadyAt)
            .ToList();

        return new ProductionQueueDTO
        {
            InQueue = [.. inQueue.Select(ProductionOrderMapper.MapToDTO)],
            InProgress = [.. inProgress.Select(ProductionOrderMapper.MapToDTO)],
            Ready = [.. ready.Select(ProductionOrderMapper.MapToDTO)],
            TotalInQueue = inQueue.Count,
            TotalInProgress = inProgress.Count,
            TotalReady = ready.Count
        };
    }
}
