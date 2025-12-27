using StackFood.Production.Application.DTOs;
using StackFood.Production.Domain.Entities;

namespace StackFood.Production.Application.UseCases.Mappers;

public static class ProductionOrderMapper
{
    public static ProductionOrderDTO MapToDTO(ProductionOrder order)
    {
        return new ProductionOrderDTO
        {
            Id = order.Id,
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            Items = [.. order.GetItems().Select(i => new ProductionItemDTO
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductCategory = i.ProductCategory,
                Quantity = i.Quantity,
                PreparationNotes = i.PreparationNotes
            })],
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
