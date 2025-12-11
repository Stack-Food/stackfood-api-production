using Microsoft.EntityFrameworkCore;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;
using StackFood.Production.Infrastructure.Data;

namespace StackFood.Production.Infrastructure.Repositories;

public class ProductionRepository : IProductionRepository
{
    private readonly ProductionDbContext _context;

    public ProductionRepository(ProductionDbContext context)
    {
        _context = context;
    }

    public async Task<ProductionOrder?> GetByIdAsync(Guid id)
    {
        return await _context.ProductionOrders
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProductionOrder?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.ProductionOrders
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<List<ProductionOrder>> GetByStatusAsync(ProductionStatus status)
    {
        return await _context.ProductionOrders
            .Where(p => p.Status == status)
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ProductionOrder>> GetQueueAsync()
    {
        return await _context.ProductionOrders
            .Where(p => p.Status == ProductionStatus.Received ||
                       p.Status == ProductionStatus.InProgress ||
                       p.Status == ProductionStatus.Ready)
            .OrderBy(p => p.Status)
            .ThenBy(p => p.Priority)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProductionOrder> CreateAsync(ProductionOrder order)
    {
        await _context.ProductionOrders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<ProductionOrder> UpdateAsync(ProductionOrder order)
    {
        _context.ProductionOrders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
}
