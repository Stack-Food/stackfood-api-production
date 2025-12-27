using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Application.DTOs;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace StackFood.Production.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class OrderQueueConsumer : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderQueueConsumer> _logger;
    private readonly string _queueUrl;

    public OrderQueueConsumer(
        IAmazonSQS sqsClient,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OrderQueueConsumer> logger)
    {
        _sqsClient = sqsClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueUrl = configuration["AWS:SQS:QueueUrl"] ?? "http://localhost:4566/000000000000/sqs-production-orders";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Queue Consumer started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

                if (response?.Messages?.Any() == true)
                {
                    foreach (var message in response.Messages)
                {
                    try
                    {
                        await ProcessMessageAsync(message);

                        await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                        {
                            QueueUrl = _queueUrl,
                            ReceiptHandle = message.ReceiptHandle
                        }, stoppingToken);

                        _logger.LogInformation("Message processed and deleted: {MessageId}", message.MessageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
                    }
                }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from queue");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Order Queue Consumer stopped");
    }

    private async Task ProcessMessageAsync(Message message)
    {
        // SNS wraps the message in a JSON envelope
        var snsMessage = JsonSerializer.Deserialize<SnsMessageWrapper>(message.Body);
        if (snsMessage?.Message == null)
        {
            _logger.LogWarning("Invalid SNS message format: {Body}", message.Body);
            return;
        }

        // Deserialize the actual order event
        var orderMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(snsMessage.Message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (orderMessage == null)
        {
            _logger.LogWarning("Failed to deserialize order message: {Message}", snsMessage.Message);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var createUseCase = scope.ServiceProvider.GetRequiredService<CreateProductionOrderUseCase>();

        var request = new CreateProductionOrderRequest
        {
            OrderId = orderMessage.OrderId,
            OrderNumber = orderMessage.OrderNumber,
            Items = orderMessage.Items.Select(i => new ProductionItemDTO
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductCategory = i.ProductCategory,
                Quantity = i.Quantity,
                PreparationNotes = i.PreparationNotes
            }).ToList(),
            Priority = orderMessage.Priority,
            EstimatedTime = orderMessage.EstimatedTime
        };

        var result = await createUseCase.ExecuteAsync(request);

        _logger.LogInformation(
            "Production order created: {ProductionId} for Order {OrderNumber}",
            result.Id,
            result.OrderNumber);
    }
}

public class OrderCreatedMessage
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public List<OrderItemMessage> Items { get; set; } = new();
    public int Priority { get; set; } = 1;
    public int? EstimatedTime { get; set; }
}

public class OrderItemMessage
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? PreparationNotes { get; set; }
}

// Helper class to deserialize SNS message wrapper
public class SnsMessageWrapper
{
    public string? Message { get; set; }
    public string? MessageId { get; set; }
    public string? TopicArn { get; set; }
    public string? Type { get; set; }
}
