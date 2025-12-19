using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases.Mappers;

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
        return order == null ? null : ProductionOrderMapper.MapToDTO(order);
    }

    public async Task<ProductionOrderDTO?> ExecuteByOrderIdAsync(Guid orderId)
    {
        var order = await _repository.GetByOrderIdAsync(orderId);
        return order == null ? null : ProductionOrderMapper.MapToDTO(order);
    }
}
