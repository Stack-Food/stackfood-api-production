using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Application.Interfaces;

public interface IProductionRepository
{
    Task<ProductionOrder?> GetByIdAsync(Guid id);
    Task<ProductionOrder?> GetByOrderIdAsync(Guid orderId);
    Task<List<ProductionOrder>> GetByStatusAsync(ProductionStatus status);
    Task<List<ProductionOrder>> GetQueueAsync();
    Task<ProductionOrder> CreateAsync(ProductionOrder order);
    Task<ProductionOrder> UpdateAsync(ProductionOrder order);
}
