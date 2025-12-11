using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using StackFood.Production.Application.Interfaces;
using System.Text.Json;

namespace StackFood.Production.Infrastructure.Services;

public class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _snsClient;

    public SnsEventPublisher(IAmazonSimpleNotificationService snsClient)
    {
        _snsClient = snsClient;
    }

    public async Task PublishAsync<T>(T eventData, string topicArn)
    {
        var message = JsonSerializer.Serialize(eventData);

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = message,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "EventType",
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = typeof(T).Name
                    }
                }
            }
        };

        await _snsClient.PublishAsync(request);
    }
}
