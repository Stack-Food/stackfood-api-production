namespace StackFood.Production.Application.DTOs;

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public int? EstimatedTime { get; set; }
}
