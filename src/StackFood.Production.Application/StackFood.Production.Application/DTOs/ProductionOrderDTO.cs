using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Application.DTOs;

public class ProductionOrderDTO
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ProductionItemDTO> Items { get; set; } = new();
    public int Priority { get; set; }
    public int? EstimatedTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class ProductionItemDTO
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? PreparationNotes { get; set; }
}
