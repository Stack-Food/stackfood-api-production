using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;

namespace StackFood.Production.Application.UseCases;

public class GetProductionOrderUseCase
{
    private readonly IProductionRepository _repository;

    public GetProductionOrderUseCase(IProductionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductionOrderDTO?> ExecuteByIdAsync(Guid id)
    {
        var order = await _repository.GetByIdAsync(id);
        return order == null ? null : MapToDTO(order);
    }

    public async Task<ProductionOrderDTO?> ExecuteByOrderIdAsync(Guid orderId)
    {
        var order = await _repository.GetByOrderIdAsync(orderId);
        return order == null ? null : MapToDTO(order);
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
