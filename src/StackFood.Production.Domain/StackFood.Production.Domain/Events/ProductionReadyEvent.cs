using System.Diagnostics.CodeAnalysis;

namespace StackFood.Production.Domain.Events;

[ExcludeFromCodeCoverage]
public class ProductionReadyEvent
{
    public string EventType => "ProductionReady";
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public ProductionReadyEvent()
    {
        Timestamp = DateTime.UtcNow;
    }
}
