namespace StackFood.Production.Application.DTOs;

public class ProductionQueueDTO
{
    public List<ProductionOrderDTO> InQueue { get; set; } = new();
    public List<ProductionOrderDTO> InProgress { get; set; } = new();
    public List<ProductionOrderDTO> Ready { get; set; } = new();
    public int TotalInQueue { get; set; }
    public int TotalInProgress { get; set; }
    public int TotalReady { get; set; }
}
