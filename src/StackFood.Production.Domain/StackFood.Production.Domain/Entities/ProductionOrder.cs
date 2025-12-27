using StackFood.Production.Domain.Enums;
using System.Text.Json;

namespace StackFood.Production.Domain.Entities;

public class ProductionOrder
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public ProductionStatus Status { get; set; }

    // Items do pedido em JSON
    public string ItemsJson { get; set; } = "[]";

    public int Priority { get; set; }
    public int? EstimatedTime { get; set; } // em minutos

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public ProductionOrder()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = ProductionStatus.Received;
        Priority = 1;
        ItemsJson = "[]";
    }

    public void StartProduction()
    {
        Status = ProductionStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsReady()
    {
        Status = ProductionStatus.Ready;
        ReadyAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = ProductionStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public List<ProductionItem> GetItems()
    {
        try
        {
            return JsonSerializer.Deserialize<List<ProductionItem>>(ItemsJson) ?? new List<ProductionItem>();
        }
        catch
        {
            return new List<ProductionItem>();
        }
    }

    public void SetItems(List<ProductionItem> items)
    {
        ItemsJson = JsonSerializer.Serialize(items);
    }
}

public class ProductionItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? PreparationNotes { get; set; }
}
