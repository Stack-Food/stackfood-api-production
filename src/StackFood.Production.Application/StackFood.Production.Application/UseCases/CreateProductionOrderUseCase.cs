using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases.Mappers;
using StackFood.Production.Domain.Entities;

namespace StackFood.Production.Application.UseCases;

public class CreateProductionOrderUseCase(IProductionRepository repository)
{
    private readonly IProductionRepository _repository = repository;

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

        return ProductionOrderMapper.MapToDTO(created);
    }
}
