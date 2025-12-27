using System.Diagnostics.CodeAnalysis;

namespace StackFood.Production.Domain.Events;

[ExcludeFromCodeCoverage]
public class ProductionDeliveredEvent
{
    public string EventType => "ProductionDelivered";
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public ProductionDeliveredEvent()
    {
        Timestamp = DateTime.UtcNow;
    }
}
