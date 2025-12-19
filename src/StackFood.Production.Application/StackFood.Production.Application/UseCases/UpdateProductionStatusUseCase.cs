using StackFood.Production.Application.Configuration;
using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases.Mappers;
using StackFood.Production.Domain.Enums;
using StackFood.Production.Domain.Events;

namespace StackFood.Production.Application.UseCases;

public class UpdateProductionStatusUseCase
{
    private readonly IProductionRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly string _topicArn;

    public UpdateProductionStatusUseCase(
        IProductionRepository repository,
        IEventPublisher eventPublisher,
        ProductionSettings settings)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _topicArn = settings.SnsTopicArn;
    }

    public async Task<ProductionOrderDTO> ExecuteAsync(Guid id, UpdateStatusRequest request)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            throw new Exception($"Production order {id} not found");

        if (!Enum.TryParse<ProductionStatus>(request.Status, true, out var status))
            throw new Exception($"Invalid status: {request.Status}");

        switch (status)
        {
            case ProductionStatus.InProgress:
                if (request.EstimatedTime.HasValue)
                    order.EstimatedTime = request.EstimatedTime.Value;
                order.StartProduction();
                await PublishStartedEventAsync(order);
                break;

            case ProductionStatus.Ready:
                order.MarkAsReady();
                await PublishReadyEventAsync(order);
                break;

            case ProductionStatus.Delivered:
                order.MarkAsDelivered();
                await PublishDeliveredEventAsync(order);
                break;

            default:
                throw new Exception($"Cannot transition to status: {status}");
        }

        var updated = await _repository.UpdateAsync(order);
        return ProductionOrderMapper.MapToDTO(updated);
    }

    private async Task PublishStartedEventAsync(Domain.Entities.ProductionOrder order)
    {
        var productionEvent = new ProductionStartedEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            EstimatedTime = order.EstimatedTime
        };
        await _eventPublisher.PublishAsync(productionEvent, _topicArn);
    }

    private async Task PublishReadyEventAsync(Domain.Entities.ProductionOrder order)
    {
        var productionEvent = new ProductionReadyEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber
        };
        await _eventPublisher.PublishAsync(productionEvent, _topicArn);
    }

    private async Task PublishDeliveredEventAsync(Domain.Entities.ProductionOrder order)
    {
        var productionEvent = new ProductionDeliveredEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber
        };
        await _eventPublisher.PublishAsync(productionEvent, _topicArn);
    }
}
