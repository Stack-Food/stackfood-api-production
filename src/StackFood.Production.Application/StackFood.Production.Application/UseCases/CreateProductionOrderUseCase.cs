using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Domain.Entities;

namespace StackFood.Production.Application.UseCases;

public class CreateProductionOrderUseCase
{
    private readonly IProductionRepository _repository;

    public CreateProductionOrderUseCase(IProductionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductionOrderDTO> ExecuteAsync(CreateProductionOrderRequest request)
    {
        var order = new ProductionOrder
        {
            OrderId = request.OrderId,
            OrderNumber = request.OrderNumber,
            Priority = request.Priority,
            EstimatedTime = request.EstimatedTime
        };

        var items = request.Items.Select(i => new ProductionItem
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductCategory = i.ProductCategory,
            Quantity = i.Quantity,
            PreparationNotes = i.PreparationNotes
        }).ToList();

        order.SetItems(items);

        var created = await _repository.CreateAsync(order);

        return MapToDTO(created);
    }

    private ProductionOrderDTO MapToDTO(ProductionOrder order)
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
