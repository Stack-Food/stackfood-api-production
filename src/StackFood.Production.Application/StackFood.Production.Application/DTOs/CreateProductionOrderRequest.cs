namespace StackFood.Production.Application.DTOs;

public class CreateProductionOrderRequest
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public List<ProductionItemDTO> Items { get; set; } = new();
    public int Priority { get; set; } = 1;
    public int? EstimatedTime { get; set; }
}
