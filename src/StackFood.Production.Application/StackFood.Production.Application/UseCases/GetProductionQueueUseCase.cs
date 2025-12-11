using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Application.UseCases;

public class GetProductionQueueUseCase
{
    private readonly IProductionRepository _repository;

    public GetProductionQueueUseCase(IProductionRepository repository)
    {
        _repository = repository;
    }

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
            InQueue = inQueue.Select(MapToDTO).ToList(),
            InProgress = inProgress.Select(MapToDTO).ToList(),
            Ready = ready.Select(MapToDTO).ToList(),
            TotalInQueue = inQueue.Count,
            TotalInProgress = inProgress.Count,
            TotalReady = ready.Count
        };
    }

    private ProductionOrderDTO MapToDTO(Domain.Entities.ProductionOrder order)
    {
        return new ProductionOrderDTO
        {
            Id = order.Id,
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            Items = order.GetItems().Select(i => new ProductionItemDTO
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductCategory = i.ProductCategory,
                Quantity = i.Quantity,
                PreparationNotes = i.PreparationNotes
            }).ToList(),
            Priority = order.Priority,
            EstimatedTime = order.EstimatedTime,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            StartedAt = order.StartedAt,
            ReadyAt = order.ReadyAt,
            DeliveredAt = order.DeliveredAt
        };
    }
}
