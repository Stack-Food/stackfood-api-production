using StackFood.Production.Application.Configuration;
using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Domain.Events;

namespace StackFood.Production.Application.UseCases;

public class StartProductionUseCase
{
    private readonly IProductionRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly string _topicArn;

    public StartProductionUseCase(
        IProductionRepository repository,
        IEventPublisher eventPublisher,
        ProductionSettings settings)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _topicArn = settings.SnsTopicArn;
    }

    public async Task<ProductionOrderDTO> ExecuteAsync(Guid id, int? estimatedTime = null)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            throw new Exception($"Production order {id} not found");

        if (estimatedTime.HasValue)
            order.EstimatedTime = estimatedTime.Value;

        order.StartProduction();
        var updated = await _repository.UpdateAsync(order);

        var productionEvent = new ProductionStartedEvent
        {
            OrderId = updated.OrderId,
            OrderNumber = updated.OrderNumber,
            EstimatedTime = updated.EstimatedTime
        };

        await _eventPublisher.PublishAsync(productionEvent, _topicArn);

        return MapToDTO(updated);
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
