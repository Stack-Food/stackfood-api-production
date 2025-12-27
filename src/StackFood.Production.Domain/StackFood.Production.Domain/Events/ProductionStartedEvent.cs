namespace StackFood.Production.Domain.Events;

public class ProductionStartedEvent
{
    public string EventType => "ProductionStarted";
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? EstimatedTime { get; set; }
    public DateTime Timestamp { get; set; }

    public ProductionStartedEvent()
    {
        Timestamp = DateTime.UtcNow;
    }
}
