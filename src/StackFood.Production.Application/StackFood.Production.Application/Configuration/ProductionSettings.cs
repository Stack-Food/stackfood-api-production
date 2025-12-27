namespace StackFood.Production.Application.Configuration;

public class ProductionSettings
{
    public string SnsTopicArn { get; set; } = "arn:aws:sns:us-east-1:000000000000:sns-production-events";
}
