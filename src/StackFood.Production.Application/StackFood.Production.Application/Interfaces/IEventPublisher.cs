namespace StackFood.Production.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T eventData, string topicArn);
}
