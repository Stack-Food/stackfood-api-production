namespace StackFood.Production.Domain.Events;

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
